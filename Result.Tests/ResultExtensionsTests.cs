using FluentAssertions;
using Utilme;
using Utilme.Functional;
using Xunit;

namespace UtilmeResult.Tests;

/// <summary>
/// Phase 2: the composition operators in <c>Utilme.Functional</c>. The behaviour that matters is
/// short-circuiting — on failure the delegate must not run and the status and message must survive
/// the hop to a different value type.
/// </summary>
public class ResultExtensionsTests
{
    // ---------------------------------------------------------------- Match

    [Fact]
    public void Match_takes_the_success_branch()
    {
        Result<int>.Ok(21).Match(v => v * 2, _ => -1).Should().Be(42);
    }

    [Fact]
    public void Match_takes_the_failure_branch_with_the_failed_result()
    {
        Result<int>.Fail(ResultStatus.Timeout, "slow")
            .Match(_ => "ok", f => f.ErrorMessage!)
            .Should().Be("slow");
    }

    [Fact]
    public void Switch_runs_exactly_one_side_effect()
    {
        var seen = "";
        Result<int>.Ok(1).Switch(v => seen = $"ok{v}", _ => seen = "err");
        seen.Should().Be("ok1");

        Result<int>.NotFound.Switch(v => seen = $"ok{v}", _ => seen = "err");
        seen.Should().Be("err");
    }

    // ------------------------------------------------------------------ Map

    [Fact]
    public void Map_transforms_a_successful_value()
    {
        Result<int>.Ok(21).Map(v => v * 2).Value.Should().Be(42);
    }

    [Fact]
    public void Map_changes_the_value_type()
    {
        Result<int>.Ok(42).Map(v => v.ToString()).Should().BeOfType<Result<string>>();
    }

    [Fact]
    public void Map_short_circuits_and_preserves_the_failure_verbatim()
    {
        var invoked = false;

        var mapped = Result<int>.Fail(ResultStatus.NetworkDown, "no route")
            .Map(v => { invoked = true; return v.ToString(); });

        invoked.Should().BeFalse();
        mapped.IsError.Should().BeTrue();
        mapped.Status.Should().Be(ResultStatus.NetworkDown);
        mapped.ErrorMessage.Should().Be("no route");
    }

    [Fact]
    public void Map_of_a_messageless_failure_reuses_the_cached_instance_of_the_new_type()
    {
        Result<int>.NotFound.Map(v => v.ToString()).Should().BeSameAs(Result<string>.NotFound);
    }

    // ----------------------------------------------------------------- Then

    [Fact]
    public void Then_chains_a_result_returning_operation()
    {
        Result<int>.Ok(4).Then(v => Result<string>.Ok($"v{v}")).Value.Should().Be("v4");
    }

    [Fact]
    public void Then_propagates_a_failure_produced_by_the_chained_operation()
    {
        var chained = Result<int>.Ok(4).Then(_ => Result<string>.Fail(ResultStatus.InvalidData, "bad"));

        chained.Status.Should().Be(ResultStatus.InvalidData);
        chained.ErrorMessage.Should().Be("bad");
    }

    [Fact]
    public void Then_short_circuits_on_an_already_failed_result()
    {
        var invoked = false;

        var chained = Result<int>.Cancelled
            .Then(_ => { invoked = true; return Result<string>.Ok("x"); });

        invoked.Should().BeFalse();
        chained.Status.Should().Be(ResultStatus.Cancelled);
    }

    [Fact]
    public void Chains_compose()
    {
        Result.Ok(5)
            .Map(v => v + 1)
            .Then(v => v > 3 ? Result.Ok(v * 2) : Result.Fail<int>(ResultStatus.InvalidData))
            .Map(v => $"={v}")
            .Value.Should().Be("=12");
    }

    // ------------------------------------------------------------ Tap / Else

    [Fact]
    public void Tap_observes_success_without_changing_the_result()
    {
        var seen = 0;
        var original = Result<int>.Ok(7);

        original.Tap(v => seen = v).Should().BeSameAs(original);
        seen.Should().Be(7);
    }

    [Fact]
    public void Tap_does_not_run_on_failure()
    {
        var seen = 0;
        Result<int>.NotFound.Tap(v => seen = v);
        seen.Should().Be(0);
    }

    [Fact]
    public void TapError_observes_failure_without_changing_the_result()
    {
        ResultStatus? seen = null;
        var original = Result<int>.Timeout;

        original.TapError(f => seen = f.Status).Should().BeSameAs(original);
        seen.Should().Be(ResultStatus.Timeout);
    }

    [Fact]
    public void TapError_does_not_run_on_success()
    {
        var invoked = false;
        Result<int>.Ok(1).TapError(_ => invoked = true);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Else_substitutes_a_computed_value_on_failure_only()
    {
        Result<int>.Ok(5).Else(_ => -1).Should().Be(5);
        Result<int>.NotFound.Else(_ => -1).Should().Be(-1);
        Result<int>.Fail(ResultStatus.Timeout, "slow").Else(f => f.ErrorMessage!.Length).Should().Be(4);
    }

    // ---------------------------------------------------------------- async

    [Fact]
    public async Task MapAsync_transforms_a_successful_value()
    {
        var mapped = await Result<int>.Ok(21).MapAsync(v => Task.FromResult(v * 2));
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task MapAsync_short_circuits_on_failure()
    {
        var invoked = false;

        var mapped = await Result<int>.NotFound
            .MapAsync(v => { invoked = true; return Task.FromResult(v); });

        invoked.Should().BeFalse();
        mapped.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task ThenAsync_chains_an_asynchronous_operation()
    {
        var chained = await Result<int>.Ok(4)
            .ThenAsync(v => Task.FromResult(Result<string>.Ok($"v{v}")));

        chained.Value.Should().Be("v4");
    }

    [Fact]
    public async Task Async_chains_compose_over_a_pending_result()
    {
        var value = await Task.FromResult(Result.Ok(5))
            .MapAsync(v => v + 1)
            .ThenAsync(v => Task.FromResult(Result.Ok(v * 2)))
            .MatchAsync(v => $"={v}", f => f.Status.ToString());

        value.Should().Be("=12");
    }

    [Fact]
    public async Task TapAsync_observes_a_pending_success()
    {
        var seen = 0;
        await Task.FromResult(Result.Ok(9)).TapAsync(v => seen = v);
        seen.Should().Be(9);
    }

    // -------------------------------------------------------- argument guards

    [Fact]
    public void Operators_reject_null_delegates()
    {
        var ok = Result<int>.Ok(1);

        ((Action)(() => ok.Map<int, int>(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.Then<int, int>(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.Tap(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.TapError(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.Else(null!))).Should().Throw<ArgumentNullException>();
    }
}
