using System.ComponentModel;
using System.Reflection;
using FluentAssertions;
using Utilme;
using Xunit;

namespace UtilmeResult.Tests;

/// <summary>
/// Phase 3: the inconsistently-shaped per-status members are hidden from IntelliSense but remain
/// fully functional. Hidden is not deprecated — no <see cref="ObsoleteAttribute"/> is used, so
/// consuming apps compile without a single new warning.
/// </summary>
/// <remarks>
/// Note that <see cref="EditorBrowsableAttribute"/> is honoured by the IDE for members coming from
/// a referenced <em>assembly</em>. Projects inside this solution still see the members in
/// completion lists, which is why the test project can keep exercising them.
/// </remarks>
public class SoftDeprecationTests
{
    private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

    [Theory]
    [InlineData("NotFound")]
    [InlineData("Timeout")]
    [InlineData("Cancelled")]
    public void Legacy_status_properties_are_hidden_from_IntelliSense(string name)
    {
        var attribute = typeof(Result<int>)
            .GetProperty(name, PublicStatic)!
            .GetCustomAttribute<EditorBrowsableAttribute>();

        attribute.Should().NotBeNull($"'{name}' should steer new code elsewhere");
        attribute!.State.Should().Be(EditorBrowsableState.Never);
    }

    [Theory]
    [InlineData("NotSupported")]
    [InlineData("InvalidData")]
    [InlineData("NetworkUp")]
    [InlineData("NetworkDown")]
    public void Legacy_status_methods_are_hidden_from_IntelliSense(string name)
    {
        var attribute = typeof(Result<int>)
            .GetMethod(name, PublicStatic)!
            .GetCustomAttribute<EditorBrowsableAttribute>();

        attribute.Should().NotBeNull($"'{name}()' should steer new code elsewhere");
        attribute!.State.Should().Be(EditorBrowsableState.Never);
    }

    [Theory]
    [InlineData("Ok")]
    [InlineData("Error")]
    [InlineData("Fail")]
    public void The_replacement_surface_stays_visible(string name)
    {
        typeof(Result<int>)
            .GetMethod(name, PublicStatic)!
            .GetCustomAttribute<EditorBrowsableAttribute>()
            .Should().BeNull($"'{name}' is the surface new code should find");
    }

    [Fact]
    public void The_non_generic_factory_is_entirely_visible()
    {
        var hidden = typeof(Result)
            .GetMethods(PublicStatic)
            .Where(m => m.GetCustomAttribute<EditorBrowsableAttribute>() is
                { State: EditorBrowsableState.Never })
            .Select(m => m.Name);

        hidden.Should().BeEmpty("the consistent factory surface is what new code should use");
    }

    [Fact]
    public void Nothing_is_marked_Obsolete()
    {
        // Deliberate: [Obsolete] would spam warnings through apps that already use these members.
        // Revisit only at a major version.
        var obsolete = typeof(Result<int>)
            .GetMembers(PublicStatic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<ObsoleteAttribute>() is not null)
            .Select(m => m.Name);

        obsolete.Should().BeEmpty();
    }

    [Fact]
    public void Hidden_members_still_work_exactly_as_before()
    {
        // Hiding is an IDE affordance only; it changes no runtime behaviour.
        Result<int>.NotFound.Status.Should().Be(ResultStatus.NotFound);
        Result<int>.Timeout.Status.Should().Be(ResultStatus.Timeout);
        Result<int>.Cancelled.Status.Should().Be(ResultStatus.Cancelled);
        Result<int>.NotSupported().Status.Should().Be(ResultStatus.NotSupported);
        Result<int>.InvalidData().Status.Should().Be(ResultStatus.InvalidData);
        Result<int>.NetworkUp().Status.Should().Be(ResultStatus.NetworkUp);
        Result<int>.NetworkDown().Status.Should().Be(ResultStatus.NetworkDown);
    }
}
