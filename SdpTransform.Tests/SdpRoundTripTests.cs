using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Characterization tests: they pin the CURRENT observable behaviour of the SDP parser and writer,
/// warts included, so that a refactor cannot silently change what consuming apps see.
///
/// A failure here means behaviour changed. That is only acceptable if the change was intended —
/// in which case update the test in the same commit and say so.
///
/// This file holds the end-to-end round trips, which are the safety net that matters most: they
/// exercise almost every parsing and writing path at once.
/// </summary>
public class SdpRoundTripTests
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Fact]
    public void The_offer_parses()
    {
        var sdp = SdpSamples.WebRtcOffer.ToSdp();

        sdp.Should().NotBeNull();
        sdp!.ProtocolVersion.Should().Be(0);
        sdp.SessionName.Should().Be("-");
        sdp.MediaDescriptions.Should().HaveCount(2);
        sdp.MediaDescriptions[0].Media.Should().Be(MediaType.Audio);
        sdp.MediaDescriptions[1].Media.Should().Be(MediaType.Video);
    }

    [Fact]
    public void Parsing_then_writing_reproduces_the_offer_exactly()
    {
        // Byte-for-byte since Phase 2 removed the writer's stray trailing whitespace. Before that
        // this could only be compared with each line trimmed.
        SdpSamples.WebRtcOffer.ToSdp()!.ToText().Should().Be(SdpSamples.WebRtcOffer);
    }

    [Fact]
    public void Writing_is_idempotent()
    {
        // The strongest property available without asserting exact whitespace: text produced by the
        // writer must parse back to something that writes identically, byte for byte.
        var once = SdpSamples.WebRtcOffer.ToSdp()!.ToText();
        var twice = once.ToSdp()!.ToText();

        twice.Should().Be(once);
    }

    [Fact]
    public void Json_round_trip_reproduces_the_same_text()
    {
        var sdp = SdpSamples.WebRtcOffer.ToSdp()!;

        var json = JsonSerializer.Serialize(sdp, JsonOptions);
        var fromJson = JsonSerializer.Deserialize<Sdp>(json, JsonOptions);

        fromJson.Should().NotBeNull();
        fromJson!.ToText().Should().Be(sdp.ToText());
    }

    [Fact]
    public void Json_uses_the_wire_names_from_the_enum_attributes()
    {
        var json = JsonSerializer.Serialize(SdpSamples.WebRtcOffer.ToSdp()!, JsonOptions);

        json.Should().Contain("\"IN\"")          // NetType.Internet
            .And.Contain("\"IP4\"")              // AddrType.Ip4
            .And.Contain("\"audio\"")            // MediaType.Audio
            .And.Contain("\"sha-512\"")          // HashFunction.Sha512
            .And.Contain("\"actpass\"")          // SetupRole.ActPass
            .And.Contain("\"BUNDLE\"");          // GroupSemantics.Bundle
    }

    [Fact]
    public void Session_level_attributes_are_parsed()
    {
        var sdp = SdpSamples.WebRtcOffer.ToSdp()!;

        sdp.Attributes.IceLite.Should().BeTrue();
        sdp.Attributes.Group!.Semantics.Should().Be(GroupSemantics.Bundle);
        sdp.Attributes.Group.SemanticsExtensions.Should().Equal("0", "1");
        sdp.Attributes.MsidSemantic!.Token.Should().Be("WMS");
        sdp.Attributes.Fingerprint!.HashFunction.Should().Be(HashFunction.Sha512);
        sdp.Attributes.Fingerprint.HashValue.Should().HaveCount(64);
    }

    [Fact]
    public void Media_level_attributes_are_parsed()
    {
        var audio = SdpSamples.WebRtcOffer.ToSdp()!.MediaDescriptions[0];

        audio.Port.Should().Be(7);
        audio.Proto.Should().Be("UDP/TLS/RTP/SAVPF");
        audio.Fmts.Should().Equal("100");

        audio.Attributes.RtcpMux.Should().BeTrue();
        audio.Attributes.RtcpRsize.Should().BeTrue();
        audio.Attributes.SendOnly.Should().BeTrue();
        audio.Attributes.EndOfCandidates.Should().BeTrue();
        audio.Attributes.Mid!.Id.Should().Be("0");
        audio.Attributes.IceUfrag!.Ufrag.Should().Be("hchivqia1vmdfg6o");
        audio.Attributes.Setup!.Role.Should().Be(SetupRole.ActPass);
        audio.Attributes.Candidates.Should().ContainSingle();
        audio.Attributes.Rtpmaps.Should().ContainSingle();
        audio.Attributes.Extmaps.Should().HaveCount(3);
    }

    [Fact]
    public void The_video_section_keeps_its_multiple_ssrcs_and_feedback_lines()
    {
        var video = SdpSamples.WebRtcOffer.ToSdp()!.MediaDescriptions[1];

        video.Fmts.Should().Equal("101", "102");
        video.Attributes.Ssrcs.Should().HaveCount(2);
        video.Attributes.SsrcGroups.Should().ContainSingle();
        video.Attributes.RtcpFbs.Should().HaveCount(4);
        video.Attributes.Rtpmaps.Should().HaveCount(2);
        video.Attributes.Extmaps.Should().HaveCount(5);
    }
}
