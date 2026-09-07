using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Writer defects fixed in Phase 2. Each test here was previously in
/// <see cref="KnownDefectTests"/> asserting the broken behaviour; it was rewritten rather than
/// deleted so the fix is visible in the diff, and it now guards against regression.
/// </summary>
public class WriterRegressionTests
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

    [Fact]
    public void Bandwidth_survives_a_round_trip()
    {
        // Was: ToText wrote the raw enum name and a space ("b=ApplicationSpecific 1024").
        // Now it writes the Display name and a ':', matching what ToBandwidth reads.
        var sdp = Sample("b=AS:1024").ToSdp()!;

        sdp.Bandwidths[0].Type.Should().Be(BandwidthType.ApplicationSpecific);
        sdp.ToText().Should().Contain("b=AS:1024");

        // And the output now parses back to the same value.
        sdp.ToText().ToSdp()!.Bandwidths[0].Type.Should().Be(BandwidthType.ApplicationSpecific);
    }

    [Fact]
    public void Writing_an_sdp_with_no_attributes_succeeds()
    {
        // Was: NullReferenceException, because ToText dereferenced Attributes unconditionally
        // while ToSdp only allocates it on seeing an "a=" line.
        var sdp = Sample().ToSdp()!;

        sdp.Attributes.Should().BeNull();
        sdp.ToText().Should().Be(Sample());
    }

    [Fact]
    public void Writing_a_session_with_no_media_descriptions_succeeds()
    {
        // Media descriptions are optional in SDP, and a hand-built Sdp may have none at all.
        var sdp = new Sdp
        {
            ProtocolVersion = 0,
            Origin = "o=- 1 1 IN IP4 127.0.0.1".ToOrigin(),
            SessionName = "-",
            Timings = [.. new[] { "t=0 0".ToTiming() }],
        };

        sdp.ToText().Should().Be(Sample());
    }

    [Fact]
    public void Writer_emits_no_trailing_whitespace()
    {
        // Was: 12 lines with one or two trailing spaces, from extmap and rtcp-fb always appending
        // their optional trailing part.
        var lines = SdpSamples.WebRtcOffer.ToSdp()!.ToText()
            .Split(Sdp.CRLF, StringSplitOptions.RemoveEmptyEntries);

        lines.Should().OnlyContain(l => l == l.TrimEnd());
    }

    [Fact]
    public void Extmap_keeps_its_direction_separator()
    {
        // Was: "a=extmap:1sendonly …" — value and direction were concatenated with no '/'.
        var extmap = "a=extmap:1/sendonly urn:ietf:params:rtp-hdrext:sdes:mid".ToExtmap();

        extmap.Value.Should().Be(1);
        extmap.Direction.Should().Be(Direction.SendOnly);

        var text = extmap.ToText();
        text.Should().Be("a=extmap:1/sendonly urn:ietf:params:rtp-hdrext:sdes:mid" + Sdp.CRLF);

        // And it parses back identically.
        var reparsed = text.ToExtmap();
        reparsed.Value.Should().Be(1);
        reparsed.Direction.Should().Be(Direction.SendOnly);
    }

    [Fact]
    public void Rid_restrictions_without_a_payload_list_are_written_cleanly()
    {
        // Was: "a=rid:hi send ;max-width=1280" — the ';' was emitted unconditionally in front of
        // the restrictions, even with no "pt=" list before them.
        var rid = "a=rid:hi send max-width=1280".ToRid();

        rid.FmtList.Should().BeNull();
        rid.ToText().Should().Be("a=rid:hi send max-width=1280" + Sdp.CRLF);
    }

    [Fact]
    public void Rid_writes_a_payload_list_and_restrictions_as_one_parameter_list()
    {
        var rid = "a=rid:hi send pt=97,98;max-width=1280".ToRid();

        rid.FmtList.Should().Equal("97", "98");
        rid.Restrictions.Should().Equal("max-width=1280");
        rid.ToText().Should().Be("a=rid:hi send pt=97,98;max-width=1280" + Sdp.CRLF);
    }
}
