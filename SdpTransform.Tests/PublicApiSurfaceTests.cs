using System.Reflection;
using System.Text;
using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Pins the entire public API surface of <c>Utilme.SdpTransform</c> against an approved baseline.
///
/// Real applications depend on this package, so the refactor is additive: nothing may be renamed,
/// removed, or have its type changed. This test makes that guarantee mechanical instead of
/// aspirational — any change to a public signature, to an enum's numeric values, or to a <c>const</c>
/// literal fails here.
///
/// The <c>const</c> literals matter as much as the signatures: they are the SDP grammar tokens
/// (<c>"candidate:"</c>, <c>"o="</c>, …) that both the parser and the writer are built from.
///
/// When a change is intentional, review the diff and copy <c>PublicApi.received.txt</c> from the test
/// output directory over <c>PublicApi.approved.txt</c>. Do not weaken the assertion.
/// </summary>
public class PublicApiSurfaceTests
{
    [Fact]
    public void The_public_api_matches_the_approved_baseline()
    {
        var actual = Describe(typeof(Sdp).Assembly);
        var approvedPath = Path.Combine(AppContext.BaseDirectory, "PublicApi.approved.txt");
        var receivedPath = Path.Combine(AppContext.BaseDirectory, "PublicApi.received.txt");

        var approved = File.Exists(approvedPath)
            ? File.ReadAllText(approvedPath).ReplaceLineEndings("\n").TrimEnd() + "\n"
            : string.Empty;

        if (actual != approved)
        {
            File.WriteAllText(receivedPath, actual);
        }
        else if (File.Exists(receivedPath))
        {
            File.Delete(receivedPath);
        }

        actual.Should().Be(approved,
            $"the public API must not change. Review the diff, then copy {receivedPath} over the " +
            "approved baseline if the change is intentional.");
    }

    // ------------------------------------------------------------------ rendering

    static string Describe(Assembly assembly)
    {
        var sb = new StringBuilder();

        foreach (var type in assembly.GetExportedTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            sb.Append(type.IsEnum ? "enum " : type.IsValueType ? "struct " : "class ")
              .AppendLine(type.FullName);

            foreach (var line in Members(type).OrderBy(l => l, StringComparer.Ordinal))
            {
                sb.Append("    ").AppendLine(line);
            }

            sb.AppendLine();
        }

        return sb.ToString().ReplaceLineEndings("\n").TrimEnd() + "\n";
    }

    static IEnumerable<string> Members(Type type)
    {
        const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        if (type.IsEnum)
        {
            foreach (var name in Enum.GetNames(type))
            {
                // Numeric values are part of the contract: they may have been persisted as ints.
                yield return $"{name} = {Convert.ToInt64(Enum.Parse(type, name)):D}";
            }

            yield break;
        }

        foreach (var field in type.GetFields(Flags))
        {
            yield return field.IsLiteral
                ? $"const {Name(field.FieldType)} {field.Name} = {Literal(field.GetRawConstantValue())}"
                : $"field {Name(field.FieldType)} {field.Name}";
        }

        foreach (var property in type.GetProperties(Flags))
        {
            var accessors = (property.GetMethod is not null ? "get; " : string.Empty)
                + (property.SetMethod is not null ? "set; " : string.Empty);
            yield return $"{Name(property.PropertyType)} {property.Name} {{ {accessors}}}";
        }

        foreach (var ctor in type.GetConstructors(Flags))
        {
            yield return $".ctor({Parameters(ctor)})";
        }

        foreach (var method in type.GetMethods(Flags).Where(m => !m.IsSpecialName))
        {
            var generics = method.IsGenericMethodDefinition
                ? "<" + string.Join(", ", method.GetGenericArguments().Select(a => a.Name)) + ">"
                : string.Empty;
            var modifier = method.IsStatic ? "static " : string.Empty;
            yield return
                $"{modifier}{Name(method.ReturnType)} {method.Name}{generics}({Parameters(method)})";
        }
    }

    static string Parameters(MethodBase method) =>
        string.Join(", ", method.GetParameters().Select(p =>
            (p.IsOut ? "out " : p.ParameterType.IsByRef ? "ref " : string.Empty)
            + Name(p.ParameterType) + " " + p.Name));

    static string Literal(object? value) => value switch
    {
        null => "null",
        string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                            .Replace("\r", "\\r").Replace("\n", "\\n") + "\"",
        _ => value.ToString() ?? "null",
    };

    static string Name(Type type)
    {
        if (type.IsByRef)
        {
            return Name(type.GetElementType()!);
        }

        if (type.IsArray)
        {
            return Name(type.GetElementType()!) + "[]";
        }

        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return Name(underlying) + "?";
        }

        if (type.IsGenericType)
        {
            var name = type.Name[..type.Name.IndexOf('`')];
            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(Name))}>";
        }

        return type.Name;
    }
}
