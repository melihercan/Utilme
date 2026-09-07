using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Phase 4: <see cref="ModelExtensions.ToSdpResult"/> reports what the parser could not handle,
/// replacing the <c>Console.WriteLine</c> diagnostics and the silently swallowed exception.
/// </summary>
/// <remarks>
/// <see cref="ModelExtensions.ToSdp"/> is unchanged — same signature, same <see langword="null"/> on
/// failure — and is now a thin wrapper over <c>ToSdpResult</c>. The compatibility tests here are as
/// important as the new-behaviour ones.
/// </remarks>
public class DiagnosticsTests
{
    const string MinimalSession =
        """
        v=0
        o=- 1 1 IN IP4 127.0.0.1
        s=-
        t=0 0
        """;

    static string Sample(params string[] extra) =>
        SdpSamples.Text([.. MinimalSession.Split('\n').Select(l => l.TrimEnd('\r')), .. extra]);

    // ------------------------------------------------------------- compatibility

    [Fact]
    public void ToSdp_still_returns_null_for_input_it_cannot_parse()
    {
        SdpSamples.Text("v=0", "o=truncated", "s=-", "t=0 0").ToSdp().Should().BeNull();
    }

    [Fact]
    public void ToSdp_returns_exactly_what_ToSdpResult_produced()
    {
        SdpSamples.WebRtcOffer.ToSdp()!.ToText()
            .Should().Be(SdpSamples.WebRtcOffer.ToSdpResult().Sdp!.ToText());
    }

    [Fact]
    public void Nothing_is_written_to_the_console_any_more()
    {
        // Was: unknown fields and attributes went to Console.WriteLine, where the caller could not
        // act on them and a hosted application got unwanted output.
        var original = Console.Out;
        var captured = new StringWriter();
        try
        {
            Console.SetOut(captured);
            Sample("a=some-unknown-attribute:1", "x=not-a-real-field").ToSdp();
            SdpSamples.Text("v=0", "o=truncated").ToSdp();
        }
        finally
        {
            Console.SetOut(original);
        }

        captured.ToString().Should().BeEmpty();
    }

    // ------------------------------------------------------------------ success

    [Fact]
    public void A_clean_parse_reports_nothing()
    {
        var result = SdpSamples.WebRtcOffer.ToSdpResult();

        result.IsValid.Should().BeTrue();
        result.Sdp.Should().NotBeNull();
        result.HasDiagnostics.Should().BeFalse();
        result.Diagnostics.Should().BeEmpty();
    }

    // ----------------------------------------------------------------- warnings

    [Fact]
    public void An_unrecognised_line_is_a_warning_and_parsing_continues()
    {
        var result = Sample("x=not-a-real-field").ToSdpResult();

        result.IsValid.Should().BeTrue("an unrecognised line is skipped, not fatal");
        result.Sdp!.SessionName.Should().Be("-");

        result.Diagnostics.Should().ContainSingle();
        var diagnostic = result.Diagnostics[0];
        diagnostic.Severity.Should().Be(SdpDiagnosticSeverity.Warning);
        diagnostic.Message.Should().Be("Unsupported session field: x=not-a-real-field");
        diagnostic.Line.Should().Be("x=not-a-real-field");
    }

    [Fact]
    public void Diagnostics_carry_the_line_number_from_the_original_text()
    {
        // Blank lines count, so the number points at the real text the caller passed in.
        var result = SdpSamples
            .Text("v=0", "o=- 1 1 IN IP4 127.0.0.1", "s=-", "", "t=0 0", "x=bad")
            .ToSdpResult();

        result.Diagnostics.Should().ContainSingle();
        result.Diagnostics[0].LineNumber.Should().Be(6);
    }

    [Fact]
    public void Session_attribute_warnings_name_the_session_scope()
    {
        // Was: the session branch reported "unsupported media description attribute", pointing the
        // reader at the wrong part of the document.
        // Note "a=mid:0" is no longer a valid example — since Phase 5 it parses at session level.
        var result = Sample("a=totally-unknown:1").ToSdpResult();

        result.Diagnostics.Should().ContainSingle();
        result.Diagnostics[0].Message.Should().Be("Unsupported session attribute: totally-unknown:1");
    }

    [Fact]
    public void Media_description_warnings_name_the_media_scope()
    {
        var result = Sample(
            "m=audio 7 RTP/AVP 0",
            "a=totally-unknown:1",
            "q=not-a-real-field").ToSdpResult();

        result.IsValid.Should().BeTrue();
        result.Diagnostics.Select(d => d.Message).Should().Equal(
            "Unsupported media description attribute: totally-unknown:1",
            "Unsupported media description field: q=not-a-real-field");
    }

    [Fact]
    public void Every_unrecognised_line_is_reported_in_document_order()
    {
        // "z=" is NOT free to use here — it is the time-zone indicator, so it parses rather than
        // being unrecognised. q, x and y are genuinely unassigned.
        var result = Sample("x=first", "y=second", "q=third").ToSdpResult();

        result.Diagnostics.Should().HaveCount(3);
        result.Diagnostics.Select(d => d.LineNumber).Should().BeInAscendingOrder();
        result.Diagnostics.Should().OnlyContain(d => d.Severity == SdpDiagnosticSeverity.Warning);
    }

    [Fact]
    public void A_malformed_known_field_is_an_error_not_a_warning()
    {
        // A recognised indicator whose value does not parse aborts the whole document — the
        // long-standing all-or-nothing behaviour, now with the reason attached.
        var result = Sample("z=2882844526").ToSdpResult();

        result.IsValid.Should().BeFalse();
        result.Diagnostics.Should().ContainSingle();
        result.Diagnostics[0].Severity.Should().Be(SdpDiagnosticSeverity.Error);
        result.Diagnostics[0].Message
            .Should().Be("FormatException: Timezones should be specified in pairs");
    }

    // ------------------------------------------------------------------- errors

    [Fact]
    public void An_unparseable_line_is_an_error_and_yields_no_sdp()
    {
        // Was: the exception was caught, its message assigned to an unused local, and null returned.
        var result = SdpSamples.Text("v=0", "o=truncated", "s=-", "t=0 0").ToSdpResult();

        result.IsValid.Should().BeFalse();
        result.Sdp.Should().BeNull();

        result.Diagnostics.Should().ContainSingle();
        result.Diagnostics[0].Severity.Should().Be(SdpDiagnosticSeverity.Error);
        result.Diagnostics[0].Message.Should().StartWith("IndexOutOfRangeException");
    }

    [Fact]
    public void ToSdpResult_never_throws()
    {
        var act = () => new[] { "", "garbage", "v=", "o=", "m=", "a=" }
            .Select(line => SdpSamples.Text(line).ToSdpResult())
            .ToList();

        act.Should().NotThrow();
    }

    // ---------------------------------------------------------------- rendering

    [Fact]
    public void A_diagnostic_renders_usefully()
    {
        new SdpDiagnostic(SdpDiagnosticSeverity.Warning, 6, "x=bad", "Unsupported session field: x=bad")
            .ToString().Should().Be("Warning (line 6): Unsupported session field: x=bad");

        new SdpDiagnostic(SdpDiagnosticSeverity.Error, 0, "", "FormatException: bad")
            .ToString().Should().Be("Error: FormatException: bad");
    }
}
