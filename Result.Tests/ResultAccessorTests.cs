using FluentAssertions;
using Utilme;
using Xunit;

namespace UtilmeResult.Tests;

/// <summary>
/// Phase 2: the safe accessors added beside <see cref="Result{T}.Value"/>, plus the non-generic
/// <see cref="Result"/> factories and <see cref="Unit"/>.
/// </summary>
public class ResultAccessorTests
{
    [Fact]
    public void IsError_is_the_inverse_of_IsOk()
    {
        Result<int>.Ok(1).IsError.Should().BeFalse();
        Result<int>.NotFound.IsError.Should().BeTrue();
    }

    [Fact]
    public void TryGetValue_yields_the_value_on_success()
    {
        Result<int>.Ok(42).TryGetValue(out var value).Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetValue_reports_failure_and_yields_default()
    {
        Result<string>.NotFound.TryGetValue(out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void GetValueOrDefault_substitutes_only_on_failure()
    {
        Result<int>.Ok(42).GetValueOrDefault(-1).Should().Be(42);
        Result<int>.NotFound.GetValueOrDefault(-1).Should().Be(-1);
    }

    [Fact]
    public void ToString_describes_a_success()
    {
        Result<int>.Ok(42).ToString().Should().Be("Ok(42)");
    }

    [Fact]
    public void ToString_of_a_messageless_failure_is_just_the_status()
    {
        // Avoids the redundant "NotFound: NotFound".
        Result<int>.NotFound.ToString().Should().Be("NotFound");
    }

    [Fact]
    public void ToString_of_a_described_failure_includes_the_message()
    {
        Result<int>.Fail(ResultStatus.NotFound, "user 7").ToString()
            .Should().Be("NotFound: user 7");
    }

    // ------------------------------------------------- non-generic factories

    [Fact]
    public void Factory_infers_the_value_type()
    {
        var result = Result.Ok(42);

        result.Should().BeOfType<Result<int>>();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Factory_exposes_every_status_as_a_method()
    {
        // The consistent surface: unlike Result<T>, nothing here is a property.
        Result.NotFound<int>().Status.Should().Be(ResultStatus.NotFound);
        Result.Timeout<int>().Status.Should().Be(ResultStatus.Timeout);
        Result.Cancelled<int>().Status.Should().Be(ResultStatus.Cancelled);
        Result.NotSupported<int>().Status.Should().Be(ResultStatus.NotSupported);
        Result.InvalidData<int>().Status.Should().Be(ResultStatus.InvalidData);
        Result.NetworkUp<int>().Status.Should().Be(ResultStatus.NetworkUp);
        Result.NetworkDown<int>().Status.Should().Be(ResultStatus.NetworkDown);
        Result.Error<int>("boom").Status.Should().Be(ResultStatus.Error);
        Result.Fail<int>(ResultStatus.Timeout, "slow").ErrorMessage.Should().Be("slow");
    }

    [Fact]
    public void Factory_delegates_to_the_same_cached_instances()
    {
        Result.NotFound<int>().Should().BeSameAs(Result<int>.NotFound);
    }

    // ------------------------------------------------------------------ Unit

    [Fact]
    public void Unit_result_models_a_void_operation()
    {
        var result = Result.Ok();

        result.Should().BeOfType<Result<Unit>>();
        result.IsOk.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public void All_Units_are_equal()
    {
        Unit.Value.Should().Be(default(Unit));
        (Unit.Value == default).Should().BeTrue();
        (Unit.Value != default).Should().BeFalse();
        Unit.Value.GetHashCode().Should().Be(default(Unit).GetHashCode());
        Unit.Value.ToString().Should().Be("()");
    }
}
