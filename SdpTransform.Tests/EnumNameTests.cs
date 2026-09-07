using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Every enum member carries its SDP token twice: <see cref="JsonStringEnumMemberNameAttribute"/>
/// for the JSON wire form and <see cref="DisplayAttribute"/> for the SDP text form, maintained
/// independently.
/// </summary>
/// <remarks>
/// They have already drifted once — <c>BandwidthType.ApplicationSpecific</c> shipped with an empty
/// JSON name against a <c>Display</c> of <c>AS</c>, found during the .NET 10 migration. These tests
/// make that class of bug impossible to reintroduce without a red build.
/// </remarks>
public class EnumNameTests
{
    public static TheoryData<Type> SdpEnums
    {
        get
        {
            // Every public enum except SdpDiagnosticSeverity, which describes this library's own
            // diagnostics rather than an SDP token and so carries neither attribute. Excluded by
            // name on purpose: a rule like "enums that already have [Display]" would silently skip a
            // new grammar enum that forgot them, which is exactly what these tests exist to catch.
            var data = new TheoryData<Type>();
            foreach (var type in typeof(Sdp).Assembly.GetExportedTypes()
                         .Where(t => t.IsEnum && t != typeof(SdpDiagnosticSeverity))
                         .OrderBy(t => t.Name, StringComparer.Ordinal))
            {
                data.Add(type);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SdpEnums))]
    public void Every_member_declares_both_names(Type type)
    {
        foreach (var field in Members(type))
        {
            field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()
                .Should().NotBeNull($"{type.Name}.{field.Name} needs a JSON wire name");

            field.GetCustomAttribute<DisplayAttribute>()
                .Should().NotBeNull($"{type.Name}.{field.Name} needs an SDP text name");
        }
    }

    [Theory]
    [MemberData(nameof(SdpEnums))]
    public void The_two_names_agree(Type type)
    {
        foreach (var field in Members(type))
        {
            var json = field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()!.Name;
            var display = field.GetCustomAttribute<DisplayAttribute>()!.Name;

            json.Should().Be(display,
                $"{type.Name}.{field.Name} must spell its SDP token the same way in both attributes");
        }
    }

    [Theory]
    [MemberData(nameof(SdpEnums))]
    public void No_name_is_blank(Type type)
    {
        foreach (var field in Members(type))
        {
            field.GetCustomAttribute<DisplayAttribute>()!.Name
                .Should().NotBeNullOrWhiteSpace($"{type.Name}.{field.Name}");
        }
    }

    [Theory]
    [MemberData(nameof(SdpEnums))]
    public void Names_are_unique_even_ignoring_case(Type type)
    {
        // Parsing is case-insensitive since Phase 3, so two members differing only in case would be
        // ambiguous — whichever the reflection loop reached first would silently win.
        var names = Members(type)
            .Select(f => f.GetCustomAttribute<DisplayAttribute>()!.Name!)
            .ToArray();

        names.Should().OnlyHaveUniqueItems();
        names.Select(n => n.ToUpperInvariant()).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [MemberData(nameof(SdpEnums))]
    public void Every_member_round_trips_through_its_sdp_name(Type type)
    {
        foreach (var value in Enum.GetValues(type))
        {
            var name = type.GetField(value.ToString()!)!.GetCustomAttribute<DisplayAttribute>()!.Name!;

            // Both the canonical spelling and an upper-cased one must resolve back to this member.
            Resolve(type, name).Should().Be(value);
            Resolve(type, name.ToUpperInvariant()).Should().Be(value);
        }
    }

    static IEnumerable<FieldInfo> Members(Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.Static);

    /// <summary>Calls the library's internal EnumFromDisplayName through a known consumer.</summary>
    static object Resolve(Type type, string name) =>
        typeof(ModelExtensions).Assembly
            .GetType("UtilmeSdpTransform.UtilityExtensions")!
            .GetMethod("EnumFromDisplayName", BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(type)
            .Invoke(null, [name])!;
}
