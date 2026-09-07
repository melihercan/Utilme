using FluentAssertions;
using Utilme;
using Xunit;

namespace UtilmeResult.Tests;

/// <summary>
/// Pins the <see cref="ResultStatus"/> wire format. The numeric values matter because a
/// consuming app may have persisted or transmitted them as ints — so new members may only
/// ever be APPENDED, never inserted or reordered.
/// </summary>
public class ResultStatusCharacterizationTests
{
    [Theory]
    [InlineData(ResultStatus.Ok, 0)]
    [InlineData(ResultStatus.Error, 1)]
    [InlineData(ResultStatus.NotFound, 2)]
    [InlineData(ResultStatus.Timeout, 3)]
    [InlineData(ResultStatus.Cancelled, 4)]
    [InlineData(ResultStatus.NotSupported, 5)]
    [InlineData(ResultStatus.InvalidData, 6)]
    [InlineData(ResultStatus.NetworkUp, 7)]
    [InlineData(ResultStatus.NetworkDown, 8)]
    public void Status_numeric_values_are_frozen(ResultStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void Adding_a_status_must_be_a_deliberate_act()
    {
        // If this fails you added (or removed) a status. That is fine — but confirm it was
        // appended at the END of the enum, then bump this count.
        Enum.GetValues<ResultStatus>().Should().HaveCount(9);
    }

    [Fact]
    public void Status_names_are_frozen()
    {
        Enum.GetNames<ResultStatus>().Should().Equal(
            "Ok",
            "Error",
            "NotFound",
            "Timeout",
            "Cancelled",
            "NotSupported",
            "InvalidData",
            "NetworkUp",
            "NetworkDown");
    }

    [Fact]
    public void Underlying_type_is_int()
    {
        Enum.GetUnderlyingType(typeof(ResultStatus)).Should().Be<int>();
    }
}
