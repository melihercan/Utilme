using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Characterization of the individual session-field converters — the paired
/// <c>ToXxx(this string)</c> / <c>ToText(this Xxx)</c> methods that <see cref="Sdp"/>-level parsing
/// is built from. Each converter is exercised directly so a failure points at one line of SDP
/// rather than at the whole document.
/// </summary>
public class FieldConversionTests
{
    static readonly DateTime NtpEpoch = new(1900, 1, 1);

    [Fact]
    public void ProtocolVersion()
    {
        "v=0".ToProtocolVersion().Should().Be(0);
        0.ToProtocolVersionText().Should().Be("v=0" + Sdp.CRLF);
    }

    [Fact]
    public void Origin()
    {
        var origin = "o=mediasoup-client 10000 2 IN IP4 0.0.0.0".ToOrigin();

        origin.UserName.Should().Be("mediasoup-client");
        origin.SessionId.Should().Be(10000UL);
        origin.SessionVersion.Should().Be(2U);
        origin.NetType.Should().Be(NetType.Internet);
        origin.AddrType.Should().Be(AddrType.Ip4);
        origin.UnicastAddress.Should().Be("0.0.0.0");

        origin.ToText().Should().Be("o=mediasoup-client 10000 2 IN IP4 0.0.0.0" + Sdp.CRLF);
    }

    [Fact]
    public void SessionName()
    {
        "s=-".ToSessionName().Should().Be("-");
        "-".ToSessionNameText().Should().Be("s=-" + Sdp.CRLF);
    }

    [Fact]
    public void Information()
    {
        "i=a session".ToInformation().Should().Be("a session");
        "a session".ToInformationText().Should().Be("i=a session" + Sdp.CRLF);
    }

    [Fact]
    public void Uri()
    {
        var uri = "u=http://example.com/".ToUri();

        uri.Should().Be(new Uri("http://example.com/"));
        uri.ToText().Should().Be("u=http://example.com/" + Sdp.CRLF);
    }

    [Fact]
    public void ConnectionData()
    {
        var connection = "c=IN IP4 127.0.0.1".ToConnectionData();

        connection.NetType.Should().Be(NetType.Internet);
        connection.AddrType.Should().Be(AddrType.Ip4);
        connection.ConnectionAddress.Should().Be("127.0.0.1");

        connection.ToText().Should().Be("c=IN IP4 127.0.0.1" + Sdp.CRLF);
    }

    [Fact]
    public void Bandwidth()
    {
        var bandwidth = "b=CT:512".ToBandwidth();

        bandwidth.Type.Should().Be(BandwidthType.ConferenceTotal);
        bandwidth.Value.Should().Be(512);

        bandwidth.ToText().Should().Be("b=CT:512" + Sdp.CRLF);
    }

    [Fact]
    public void Timing_is_measured_in_seconds_since_1900()
    {
        var timing = "t=0 0".ToTiming();

        timing.StartTime.Should().Be(NtpEpoch);
        timing.StopTime.Should().Be(NtpEpoch);
        timing.ToText().Should().Be("t=0 0" + Sdp.CRLF);
    }

    [Fact]
    public void Timing_with_real_values()
    {
        var timing = "t=3724394400 3724398000".ToTiming();

        timing.StartTime.Should().Be(NtpEpoch.AddSeconds(3724394400));
        (timing.StopTime - timing.StartTime).Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void RepeatTime_accepts_bare_seconds()
    {
        var repeat = "r=604800 3600 0 90000".ToRepeatTime();

        repeat.RepeatInterval.Should().Be(TimeSpan.FromSeconds(604800));
        repeat.ActiveDuration.Should().Be(TimeSpan.FromSeconds(3600));
        repeat.OffsetsFromStartTime.Should().Equal(
            TimeSpan.Zero, TimeSpan.FromSeconds(90000));

        repeat.ToText().Should().Be("r=604800 3600 0 90000" + Sdp.CRLF);
    }

    [Fact]
    public void RepeatTime_accepts_day_and_hour_suffixes()
    {
        var repeat = "r=7d 1h 0 25h".ToRepeatTime();

        repeat.RepeatInterval.Should().Be(TimeSpan.FromDays(7));
        repeat.ActiveDuration.Should().Be(TimeSpan.FromHours(1));
        repeat.OffsetsFromStartTime.Should().Equal(TimeSpan.Zero, TimeSpan.FromHours(25));
    }

    [Fact]
    public void TimeZones_are_parsed_in_pairs()
    {
        var zones = "z=2882844526 -1h 2882844526 0".ToTimeZones();

        zones.Should().HaveCount(2);
        zones[0].AdjustmentTime.Should().Be(NtpEpoch.AddSeconds(2882844526));
        zones[0].Offset.Should().Be(TimeSpan.FromHours(-1));
        zones[1].Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void TimeZones_reject_an_odd_number_of_tokens()
    {
        var act = () => "z=2882844526".ToTimeZones();

        act.Should().Throw<FormatException>()
            .WithMessage("Timezones should be specified in pairs");
    }

    [Fact]
    public void EncryptionKey()
    {
        var key = "k=base64:c2VjcmV0".ToEncryptionKey();

        key.Method.Should().Be(EncryptionKeyMethod.Base64);
        key.Value.Should().Be("c2VjcmV0");

        key.ToText().Should().Be("k=base64:c2VjcmV0" + Sdp.CRLF);
    }


    [Fact]
    public void MediaDescription()
    {
        var media = "m=video 7 UDP/TLS/RTP/SAVPF 101 102".ToMediaDescription();

        media.Media.Should().Be(MediaType.Video);
        media.Port.Should().Be(7);
        media.Proto.Should().Be("UDP/TLS/RTP/SAVPF");
        media.Fmts.Should().Equal("101", "102");

        media.ToText().Should().Be("m=video 7 UDP/TLS/RTP/SAVPF 101 102" + Sdp.CRLF);
    }

    [Fact]
    public void EmailAddresses_handle_the_three_documented_forms()
    {
        "e=x.y@z.org".ToEmailAddresses().Should().Equal("x.y@z.org");
        "e=x.y@z.org (Name Surname)".ToEmailAddresses().Should().Equal("x.y@z.org (Name Surname)");
        "e=Name Surname <x.y@z.org>".ToEmailAddresses().Should().Equal("Name Surname <x.y@z.org>");
    }
}
