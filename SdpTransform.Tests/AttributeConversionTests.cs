using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Characterization of the individual attribute converters. Each attribute is exercised through its
/// own <c>ToXxx</c> / <c>ToText</c> pair, with the text form written out in full so the expected
/// SDP is visible in the test rather than inferred.
/// </summary>
public class AttributeConversionTests
{
    static string Line(string text) => text + Sdp.CRLF;

    [Fact]
    public void Group()
    {
        var group = "a=group:BUNDLE 0 1".ToGroup();

        group.Semantics.Should().Be(GroupSemantics.Bundle);
        group.SemanticsExtensions.Should().Equal("0", "1");
        group.ToText().Should().Be(Line("a=group:BUNDLE 0 1"));
    }

    [Fact]
    public void MsidSemantic()
    {
        var semantic = "a=msid-semantic:WMS *".ToMsidSemantic();

        semantic.Token.Should().Be("WMS");
        semantic.IdList.Should().Equal("*");
        semantic.ToText().Should().Be(Line("a=msid-semantic:WMS *"));
    }

    [Fact]
    public void Mid()
    {
        "a=mid:0".ToMid().Id.Should().Be("0");
        "a=mid:0".ToMid().ToText().Should().Be(Line("a=mid:0"));
    }

    [Fact]
    public void Msid()
    {
        var msid = "a=msid:stream-id track-id".ToMsid();

        msid.Id.Should().Be("stream-id");
        msid.AppData.Should().Be("track-id");
        msid.ToText().Should().Be(Line("a=msid:stream-id track-id"));
    }

    [Fact]
    public void IceUfrag_and_IcePwd()
    {
        "a=ice-ufrag:abc123".ToIceUfrag().Ufrag.Should().Be("abc123");
        "a=ice-ufrag:abc123".ToIceUfrag().ToText().Should().Be(Line("a=ice-ufrag:abc123"));

        "a=ice-pwd:secret".ToIcePwd().Password.Should().Be("secret");
        "a=ice-pwd:secret".ToIcePwd().ToText().Should().Be(Line("a=ice-pwd:secret"));
    }

    [Fact]
    public void IceOptions()
    {
        var options = "a=ice-options:trickle renomination".ToIceOptions();

        options.Tags.Should().Equal("trickle", "renomination");
        options.ToText().Should().Be(Line("a=ice-options:trickle renomination"));
    }

    [Fact]
    public void Fingerprint_is_hex_pairs_separated_by_colons()
    {
        var fingerprint = "a=fingerprint:sha-256 4E:14:FF:D2".ToFingerprint();

        fingerprint.HashFunction.Should().Be(HashFunction.Sha256);
        fingerprint.HashValue.Should().Equal(0x4E, 0x14, 0xFF, 0xD2);
        fingerprint.ToText().Should().Be(Line("a=fingerprint:sha-256 4E:14:FF:D2"));
    }

    [Fact]
    public void Rtcp_port_only()
    {
        var rtcp = "a=rtcp:9".ToRtcp();

        rtcp.Port.Should().Be(9);
        rtcp.NetType.Should().BeNull();
        rtcp.ToText().Should().Be(Line("a=rtcp:9"));
    }

    [Fact]
    public void Rtcp_with_a_connection_address()
    {
        var rtcp = "a=rtcp:53020 IN IP4 126.16.64.4".ToRtcp();

        rtcp.Port.Should().Be(53020);
        rtcp.NetType.Should().Be(NetType.Internet);
        rtcp.AddrType.Should().Be(AddrType.Ip4);
        rtcp.ConnectionAddress.Should().Be("126.16.64.4");
        rtcp.ToText().Should().Be(Line("a=rtcp:53020 IN IP4 126.16.64.4"));
    }

    [Fact]
    public void Setup()
    {
        "a=setup:actpass".ToSetup().Role.Should().Be(SetupRole.ActPass);
        "a=setup:actpass".ToSetup().ToText().Should().Be(Line("a=setup:actpass"));
    }

    [Fact]
    public void SctpPort_and_MaxMessageSize()
    {
        "a=sctp-port:5000".ToSctpPort().Port.Should().Be(5000);
        "a=sctp-port:5000".ToSctpPort().ToText().Should().Be(Line("a=sctp-port:5000"));

        "a=max-message-size:262144".ToMaxMessageSize().Size.Should().Be(262144);
        "a=max-message-size:262144".ToMaxMessageSize().ToText()
            .Should().Be(Line("a=max-message-size:262144"));
    }

    [Fact]
    public void Candidate_host()
    {
        var candidate = "a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host"
            .ToCandidate();

        candidate.Foundation.Should().Be("udpcandidate");
        candidate.ComponentId.Should().Be(1);
        candidate.Transport.Should().Be(CandidateTransport.Udp);
        candidate.Priority.Should().Be(1076302079);
        candidate.ConnectionAddress.Should().Be("192.168.1.48");
        candidate.Port.Should().Be(46186);
        candidate.Type.Should().Be(CandidateType.Host);
        candidate.RelAddr.Should().BeNull();
        candidate.Extensions.Should().BeNull();

        candidate.ToText().Should()
            .Be(Line("a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host"));
    }

    [Fact]
    public void Candidate_srflx_with_related_address()
    {
        var text = "a=candidate:2 1 udp 1694498815 192.0.2.3 45664 typ srflx raddr 10.0.1.1 rport 8998";
        var candidate = text.ToCandidate();

        candidate.Type.Should().Be(CandidateType.Srflx);
        candidate.RelAddr.Should().Be("10.0.1.1");
        candidate.RelPort.Should().Be(8998);

        candidate.ToText().Should().Be(Line(text));
    }

    [Fact]
    public void Ssrc_with_and_without_a_value()
    {
        var withValue = "a=ssrc:399475718 cname:abc".ToSsrc();
        withValue.Id.Should().Be(399475718U);
        withValue.Attribute.Should().Be("cname");
        withValue.Value.Should().Be("abc");
        withValue.ToText().Should().Be(Line("a=ssrc:399475718 cname:abc"));

        var bare = "a=ssrc:399475718 cname".ToSsrc();
        bare.Value.Should().BeNull();
        bare.ToText().Should().Be(Line("a=ssrc:399475718 cname"));
    }

    [Fact]
    public void SsrcGroup()
    {
        var group = "a=ssrc-group:FID 613668621 613668622".ToSsrcGroup();

        group.Semantics.Should().Be("FID");
        group.SsrcIds.Should().Equal("613668621", "613668622");
        group.ToText().Should().Be(Line("a=ssrc-group:FID 613668621 613668622"));
    }

    [Fact]
    public void Rid_with_a_payload_type_list()
    {
        var rid = "a=rid:hi send pt=97,98".ToRid();

        rid.Id.Should().Be("hi");
        rid.Direction.Should().Be(RidDirection.Send);
        rid.FmtList.Should().Equal("97", "98");
        rid.Restrictions.Should().BeNull();

        rid.ToText().Should().Be(Line("a=rid:hi send pt=97,98"));
    }

    [Fact]
    public void Simulcast()
    {
        var simulcast = "a=simulcast:send hi;mid;lo".ToSimulcast();

        simulcast.Direction.Should().Be(RidDirection.Send);
        simulcast.IdList.Should().Equal("hi", "mid", "lo");
        simulcast.ToText().Should().Be(Line("a=simulcast:send hi;mid;lo"));
    }

    [Fact]
    public void Rtpmap_with_and_without_channels()
    {
        var stereo = "a=rtpmap:100 opus/48000/2".ToRtpmap();
        stereo.PayloadType.Should().Be(100);
        stereo.EncodingName.Should().Be("opus");
        stereo.ClockRate.Should().Be(48000);
        stereo.Channels.Should().Be(2);
        stereo.ToText().Should().Be(Line("a=rtpmap:100 opus/48000/2"));

        var video = "a=rtpmap:101 VP8/90000".ToRtpmap();
        video.Channels.Should().BeNull();
        video.ToText().Should().Be(Line("a=rtpmap:101 VP8/90000"));
    }

    [Fact]
    public void Fmtp_keeps_its_parameter_string_verbatim()
    {
        var fmtp = "a=fmtp:100 minptime=10;useinbandfec=1".ToFmtp();

        fmtp.PayloadType.Should().Be(100);
        fmtp.Value.Should().Be("minptime=10;useinbandfec=1");
        fmtp.ToText().Should().Be(Line("a=fmtp:100 minptime=10;useinbandfec=1"));
    }

    [Fact]
    public void Fmtp_converts_to_and_from_a_dictionary()
    {
        var dictionary = "a=fmtp:108 level-asymmetry-allowed=1;profile-level-id=42e01f"
            .ToFmtp()
            .ToDictionary();

        // Numeric-looking values are boxed as int, everything else stays a string.
        dictionary.Should().HaveCount(2);
        dictionary["level-asymmetry-allowed"].Should().Be(1);
        dictionary["profile-level-id"].Should().Be("42e01f");

        dictionary.ToFmtp(108).Value
            .Should().Be("level-asymmetry-allowed=1;profile-level-id=42e01f");
    }

    [Fact]
    public void RtcpFb_with_and_without_a_subtype()
    {
        var withSubType = "a=rtcp-fb:101 nack pli".ToRtcpFb();
        withSubType.PayloadType.Should().Be(101);
        withSubType.Type.Should().Be("nack");
        withSubType.SubType.Should().Be("pli");

        var bare = "a=rtcp-fb:101 transport-cc".ToRtcpFb();
        bare.SubType.Should().BeNull();

        withSubType.ToText().Should().Be(Line("a=rtcp-fb:101 nack pli"));
        bare.ToText().Should().Be(Line("a=rtcp-fb:101 transport-cc"));
    }

    [Fact]
    public void Extmap_without_a_direction()
    {
        var extmap = "a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid".ToExtmap();

        extmap.Value.Should().Be(1);
        extmap.Direction.Should().BeNull();
        extmap.Uri.Should().Be(new Uri("urn:ietf:params:rtp-hdrext:sdes:mid"));
        extmap.ExtensionAttributes.Should().BeNull();

        extmap.ToText().Should().Be(Line("a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid"));
    }
}
