using FluentAssertions;
using Utilme;
using Xunit;

namespace UtilmeResult.Tests;

/// <summary>
/// Characterization tests: they pin the CURRENT observable behaviour of <see cref="Result{T}"/>,
/// warts included, so that the planned refactor cannot silently change what consuming apps see.
///
/// A failure here means behaviour changed. That is only acceptable if the change was intended —
/// in which case update the test in the same commit and say so.
/// </summary>
public class ResultCharacterizationTests
{
    // ---------------------------------------------------------------- success

    [Fact]
    public void Ok_carries_the_value_and_reports_success()
    {
        var result = Result<int>.Ok(42);

        result.IsOk.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Should().Be(42);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Public_constructor_behaves_identically_to_Ok()
    {
        var viaCtor = new Result<string>("hello");
        var viaOk = Result<string>.Ok("hello");

        viaCtor.IsOk.Should().Be(viaOk.IsOk);
        viaCtor.Status.Should().Be(viaOk.Status);
        viaCtor.Value.Should().Be(viaOk.Value);
        viaCtor.ErrorMessage.Should().Be(viaOk.ErrorMessage);
    }

    [Fact]
    public void Ok_accepts_null_and_still_reports_success()
    {
        // Known hole: a "successful" result can wrap null. Phase 2 must not start
        // throwing here, or apps that legitimately return Ok(null) would break.
        var result = Result<string?>.Ok(null);

        result.IsOk.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Should().BeNull();
    }

    // ------------------------------------------------------------ Error(string)

    [Fact]
    public void Error_carries_the_message_and_reports_failure()
    {
        var result = Result<int>.Error("boom");

        result.IsOk.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.ErrorMessage.Should().Be("boom");
    }

    [Fact]
    public void Error_always_forces_the_Error_status()
    {
        // This is the core modelling defect the refactor addresses: a message and a
        // non-Error status are mutually exclusive today. Fail(status, message) will be
        // ADDED alongside; Error(string) must keep collapsing to ResultStatus.Error.
        Result<int>.Error("not found, actually").Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void Error_accepts_a_null_message()
    {
        var result = Result<int>.Error(null!);

        result.IsOk.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.ErrorMessage.Should().BeNull();
    }

    // ------------------------------------------------- property-style failures

    public static TheoryData<Result<int>, ResultStatus> PropertyStyleFailures => new()
    {
        { Result<int>.NotFound, ResultStatus.NotFound },
        { Result<int>.Timeout, ResultStatus.Timeout },
        { Result<int>.Cancelled, ResultStatus.Cancelled },
    };

    [Theory]
    [MemberData(nameof(PropertyStyleFailures))]
    public void Property_style_failures_report_failure_with_the_status_name_as_message(
        Result<int> result, ResultStatus expected)
    {
        result.IsOk.Should().BeFalse();
        result.Status.Should().Be(expected);
        result.ErrorMessage.Should().Be(expected.ToString());
    }

    // --------------------------------------------------- method-style failures

    public static TheoryData<Result<int>, ResultStatus> MethodStyleFailures => new()
    {
        { Result<int>.NotSupported(), ResultStatus.NotSupported },
        { Result<int>.InvalidData(), ResultStatus.InvalidData },
        { Result<int>.NetworkUp(), ResultStatus.NetworkUp },
        { Result<int>.NetworkDown(), ResultStatus.NetworkDown },
    };

    [Theory]
    [MemberData(nameof(MethodStyleFailures))]
    public void Method_style_failures_report_failure_with_the_status_name_as_message(
        Result<int> result, ResultStatus expected)
    {
        result.IsOk.Should().BeFalse();
        result.Status.Should().Be(expected);
        result.ErrorMessage.Should().Be(expected.ToString());
    }

    [Fact]
    public void ErrorMessage_for_enum_failures_is_just_the_status_name()
    {
        // Documents that enum-based failures carry no real context — the motivation
        // for adding Fail(status, message) in Phase 2.
        Result<int>.NotFound.ErrorMessage.Should().Be("NotFound");
        Result<int>.NetworkDown().ErrorMessage.Should().Be("NetworkDown");
    }

    // ------------------------------------------------------- the polarity trap

    [Fact]
    public void NetworkUp_is_a_FAILURE_despite_its_name()
    {
        // DO NOT "FIX" THIS. NetworkUp() reads like a success but reports IsOk == false.
        // Flipping the polarity would silently invert branches in every consuming app,
        // which is the single most dangerous change available in this library.
        var result = Result<int>.NetworkUp();

        result.IsOk.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NetworkUp);
    }

    // ------------------------------------------------------- Value on failure

    [Fact]
    public void Value_on_a_failed_result_returns_default_and_does_not_throw()
    {
        // Phase 2 adds TryGetValue/GetValueOrDefault, but Value itself must keep
        // returning default rather than throwing.
        Result<int>.NotFound.Value.Should().Be(0);
        Result<string>.Timeout.Value.Should().BeNull();
        Result<int>.Error("boom").Value.Should().Be(0);
    }

    // -------------------------------------------- instance identity & equality

    [Fact]
    public void Message_less_failures_are_cached_and_shared()
    {
        // Changed deliberately in Phase 1 (was NotBeSameAs). These instances are immutable and
        // carry no caller data, so one shared instance per closed generic type is enough.
        Result<int>.NotFound.Should().BeSameAs(Result<int>.NotFound);
        Result<int>.Timeout.Should().BeSameAs(Result<int>.Timeout);
        Result<int>.Cancelled.Should().BeSameAs(Result<int>.Cancelled);
        Result<int>.NotSupported().Should().BeSameAs(Result<int>.NotSupported());
        Result<int>.InvalidData().Should().BeSameAs(Result<int>.InvalidData());
        Result<int>.NetworkUp().Should().BeSameAs(Result<int>.NetworkUp());
        Result<int>.NetworkDown().Should().BeSameAs(Result<int>.NetworkDown());
    }

    [Fact]
    public void Caching_is_per_closed_generic_type()
    {
        ReferenceEquals(Result<int>.NotFound, Result<string>.NotFound).Should().BeFalse();
    }

    [Fact]
    public void Results_that_carry_data_are_still_allocated_per_call()
    {
        // Ok and Error cannot be cached: they carry a value or a message.
        Result<int>.Ok(1).Should().NotBeSameAs(Result<int>.Ok(1));
        Result<int>.Error("boom").Should().NotBeSameAs(Result<int>.Error("boom"));
    }

    [Fact]
    public void Equality_is_reference_based()
    {
        // PHASE 4 (v2) MAY FLIP THIS. Two structurally identical results are not equal
        // today, so results are safe as reference-semantics dictionary keys.
        Result<int>.Ok(1).Equals(Result<int>.Ok(1)).Should().BeFalse();
        (Result<int>.Ok(1) == Result<int>.Ok(1)).Should().BeFalse();
    }

    [Fact]
    public void Result_is_a_reference_type_and_can_be_null()
    {
        // Pins the binary-compatibility constraint: Result<T> must not become a struct.
        Result<int>? maybe = null;
        maybe.Should().BeNull();
    }
}
