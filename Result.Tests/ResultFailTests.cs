using FluentAssertions;
using Utilme;
using Xunit;

namespace UtilmeResult.Tests;

/// <summary>
/// Phase 2: <see cref="Result{T}.Fail"/> is the factory that finally lets a specific status carry
/// a caller-supplied message — the modelling defect <c>Error(string)</c> could not express.
/// </summary>
public class ResultFailTests
{
    [Fact]
    public void Fail_pairs_a_status_with_a_message()
    {
        var result = Result<int>.Fail(ResultStatus.NotFound, "user 7 is not in the directory");

        result.IsOk.Should().BeFalse();
        result.IsError.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
        result.ErrorMessage.Should().Be("user 7 is not in the directory");
    }

    [Fact]
    public void Fail_without_a_message_falls_back_to_the_status_name()
    {
        var result = Result<int>.Fail(ResultStatus.Timeout);

        result.Status.Should().Be(ResultStatus.Timeout);
        result.ErrorMessage.Should().Be("Timeout");
    }

    [Fact]
    public void Fail_without_a_message_returns_the_same_cached_instance_as_the_legacy_member()
    {
        Result<int>.Fail(ResultStatus.NotFound).Should().BeSameAs(Result<int>.NotFound);
        Result<int>.Fail(ResultStatus.Timeout).Should().BeSameAs(Result<int>.Timeout);
        Result<int>.Fail(ResultStatus.Cancelled).Should().BeSameAs(Result<int>.Cancelled);
        Result<int>.Fail(ResultStatus.NotSupported).Should().BeSameAs(Result<int>.NotSupported());
        Result<int>.Fail(ResultStatus.InvalidData).Should().BeSameAs(Result<int>.InvalidData());
        Result<int>.Fail(ResultStatus.NetworkUp).Should().BeSameAs(Result<int>.NetworkUp());
        Result<int>.Fail(ResultStatus.NetworkDown).Should().BeSameAs(Result<int>.NetworkDown());
    }

    [Fact]
    public void Fail_with_a_message_allocates_because_it_carries_data()
    {
        Result<int>.Fail(ResultStatus.NotFound, "gone")
            .Should().NotBeSameAs(Result<int>.NotFound);
    }

    [Fact]
    public void Fail_rejects_Ok_because_it_is_not_a_failure()
    {
        var act = () => Result<int>.Fail(ResultStatus.Ok);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("status");
    }

    [Fact]
    public void Fail_rejects_Ok_even_when_a_message_is_supplied()
    {
        var act = () => Result<int>.Fail(ResultStatus.Ok, "nonsense");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(ResultStatus.Error)]
    [InlineData(ResultStatus.NotFound)]
    [InlineData(ResultStatus.Timeout)]
    [InlineData(ResultStatus.Cancelled)]
    [InlineData(ResultStatus.NotSupported)]
    [InlineData(ResultStatus.InvalidData)]
    [InlineData(ResultStatus.NetworkUp)]
    [InlineData(ResultStatus.NetworkDown)]
    public void Every_failure_status_round_trips_through_Fail(ResultStatus status)
    {
        var result = Result<string>.Fail(status, "context");

        result.IsOk.Should().BeFalse();
        result.Status.Should().Be(status);
        result.ErrorMessage.Should().Be("context");
    }
}
