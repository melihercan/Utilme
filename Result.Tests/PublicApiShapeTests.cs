using System.Reflection;
using FluentAssertions;
using Utilme;
using Xunit;

namespace UtilmeResult.Tests;

/// <summary>
/// Pins the exact shape of the public API surface.
///
/// The important — and awkward — fact captured here: NotFound/Timeout/Cancelled are
/// PROPERTIES while NotSupported/InvalidData/NetworkUp/NetworkDown are METHODS. C# forbids a
/// member name being both, so this inconsistency cannot be unified in place without a
/// source- and binary-breaking removal. The refactor must add a new consistent surface
/// beside these, leaving every member below exactly as it is.
/// </summary>
public class PublicApiShapeTests
{
    private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

    [Theory]
    [InlineData("NotFound")]
    [InlineData("Timeout")]
    [InlineData("Cancelled")]
    public void These_failures_are_static_properties(string name)
    {
        typeof(Result<int>).GetProperty(name, PublicStatic)
            .Should().NotBeNull($"'{name}' is consumed as a property and must stay one");

        typeof(Result<int>).GetMethod(name, PublicStatic)
            .Should().BeNull($"'{name}' must not become a method");
    }

    [Theory]
    [InlineData("NotSupported")]
    [InlineData("InvalidData")]
    [InlineData("NetworkUp")]
    [InlineData("NetworkDown")]
    public void These_failures_are_static_methods(string name)
    {
        typeof(Result<int>).GetMethod(name, PublicStatic)
            .Should().NotBeNull($"'{name}()' is consumed as a method and must stay one");

        typeof(Result<int>).GetProperty(name, PublicStatic)
            .Should().BeNull($"'{name}' must not become a property");
    }

    [Fact]
    public void Factory_methods_are_present_with_their_current_signatures()
    {
        typeof(Result<int>).GetMethod("Ok", PublicStatic, [typeof(int)])
            .Should().NotBeNull();

        typeof(Result<int>).GetMethod("Error", PublicStatic, [typeof(string)])
            .Should().NotBeNull();
    }

    [Fact]
    public void The_value_constructor_is_public()
    {
        typeof(Result<int>).GetConstructor([typeof(int)])
            .Should().NotBeNull("apps construct successes directly with new Result<T>(value)");
    }

    [Fact]
    public void Failure_constructors_stay_non_public()
    {
        typeof(Result<int>).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Should().ContainSingle("only the value constructor is public");
    }

    [Theory]
    [InlineData("Value")]
    [InlineData("IsOk")]
    [InlineData("Status")]
    [InlineData("ErrorMessage")]
    public void Instance_properties_are_public_and_get_only(string name)
    {
        var property = typeof(Result<int>).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

        property.Should().NotBeNull();
        property!.CanRead.Should().BeTrue();
        property.SetMethod.Should().BeNull($"'{name}' is immutable and must stay so");
    }

    [Fact]
    public void Nullable_annotations_did_not_change_the_runtime_signatures()
    {
        // Phase 1 annotated Value as T? and ErrorMessage as string?. Nullable annotations are
        // metadata only, so the IL signatures must be unchanged — in particular T? on an
        // unconstrained type parameter must NOT have become Nullable<T>.
        typeof(Result<int>).GetProperty("Value")!.PropertyType.Should().Be<int>();
        typeof(Result<string>).GetProperty("Value")!.PropertyType.Should().Be<string>();
        typeof(Result<int>).GetProperty("ErrorMessage")!.PropertyType.Should().Be<string>();
    }

    [Fact]
    public void Type_identity_is_frozen()
    {
        var type = typeof(Result<int>);

        type.IsClass.Should().BeTrue("turning Result<T> into a struct is binary breaking");
        type.Namespace.Should().Be("Utilme");
        type.GetGenericTypeDefinition().Name.Should().Be("Result`1");
        type.Assembly.GetName().Name.Should().Be("Utilme.Result");
    }

    [Fact]
    public void No_public_members_have_been_added_or_removed_unnoticed()
    {
        // A coarse net for accidental surface changes. When the refactor intentionally adds
        // members, update this list in the same commit so the addition is reviewed.
        var members = typeof(Result<int>)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal);

        members.Should().Equal(
            ".ctor",
            "Cancelled",
            "Error",
            "ErrorMessage",
            "Fail",              // added in Phase 2
            "GetValueOrDefault", // added in Phase 2
            "InvalidData",
            "IsError",           // added in Phase 2
            "IsOk",
            "NetworkDown",
            "NetworkUp",
            "NotFound",
            "NotSupported",
            "Ok",
            "Status",
            "Timeout",
            "ToString",          // added in Phase 2
            "TryGetValue",       // added in Phase 2
            "Value",
            "get_Cancelled",
            "get_ErrorMessage",
            "get_IsError",       // added in Phase 2
            "get_IsOk",
            "get_NotFound",
            "get_Status",
            "get_Timeout",
            "get_Value");
    }
}
