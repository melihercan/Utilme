using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Phase 5: session-level and media-level attributes are handled by one shared parser dispatcher and
/// one shared writer, so the two scopes cannot drift apart again.
/// </summary>
/// <remarks>
/// Previously the session branch recognised only five attributes — <c>extmap-allow-mixed</c>,
/// <c>ice-lite</c>, <c>group</c>, <c>msid-semantic</c> and <c>fingerprint</c> — and everything else
/// valid at session level was dropped with a warning. This was the last entry in
/// <c>KnownDefectTests</c>, which is now empty and gone.
/// </remarks>
public class AttributeSymmetryTests
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
    public void Session_level_value_attributes_are_parsed()
    {
        var sdp = Sample("a=mid:0", "a=setup:actpass", "a=ice-ufrag:abc").ToSdp()!;

        sdp.Attributes.Mid!.Id.Should().Be("0");
        sdp.Attributes.Setup!.Role.Should().Be(SetupRole.ActPass);
        sdp.Attributes.IceUfrag!.Ufrag.Should().Be("abc");
    }

    [Fact]
    public void Session_level_binary_attributes_are_parsed()
    {
        var sdp = Sample("a=sendonly", "a=rtcp-mux").ToSdp()!;

        sdp.Attributes.SendOnly.Should().BeTrue();
        sdp.Attributes.RtcpMux.Should().BeTrue();
    }

    [Fact]
    public void Session_level_list_attributes_are_parsed()
    {
        var sdp = Sample("a=rtpmap:100 opus/48000/2", "a=rtpmap:101 VP8/90000").ToSdp()!;

        sdp.Attributes.Rtpmaps.Should().HaveCount(2);
    }

    [Fact]
    public void Ice_lite_still_works_and_is_now_accepted_at_media_level_too()
    {
        // ice-lite was the one attribute the session branch handled and the media branch did not.
        Sample("a=ice-lite").ToSdp()!.Attributes.IceLite.Should().BeTrue();

        Sample("m=audio 7 RTP/AVP 0", "a=ice-lite").ToSdp()!
            .MediaDescriptions[0].Attributes.IceLite.Should().BeTrue();
    }

    [Fact]
    public void Session_level_attributes_round_trip()
    {
        // The point of changing the writer at the same time: parsing more at session level would
        // otherwise silently drop those lines on the way back out.
        var text = Sample("a=sendonly", "a=mid:0", "a=setup:actpass");

        text.ToSdp()!.ToText().Should().Be(text);
    }

    [Fact]
    public void Media_level_attributes_are_unaffected()
    {
        var sdp = Sample(
            "m=audio 7 RTP/AVP 0",
            "a=rtcp-mux",
            "a=mid:0",
            "a=rtpmap:0 PCMU/8000").ToSdp()!;

        var media = sdp.MediaDescriptions[0];
        media.Attributes.RtcpMux.Should().BeTrue();
        media.Attributes.Mid!.Id.Should().Be("0");
        media.Attributes.Rtpmaps.Should().ContainSingle();
    }

    [Fact]
    public void An_unknown_attribute_is_still_reported_at_both_scopes()
    {
        Sample("a=totally-unknown:1").ToSdpResult()
            .Diagnostics.Should().ContainSingle()
            .Which.Message.Should().Be("Unsupported session attribute: totally-unknown:1");

        Sample("m=audio 7 RTP/AVP 0", "a=totally-unknown:1").ToSdpResult()
            .Diagnostics.Should().ContainSingle()
            .Which.Message.Should().Be("Unsupported media description attribute: totally-unknown:1");
    }

    [Fact]
    public void The_reference_offer_is_unchanged()
    {
        // The strongest guard on this refactor: sharing the dispatcher and the writer between the
        // two scopes must not alter a real document by a single byte.
        SdpSamples.WebRtcOffer.ToSdp()!.ToText().Should().Be(SdpSamples.WebRtcOffer);
    }
}
