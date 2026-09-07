using FluentAssertions;
using Utilme.SdpTransform;
using Xunit;

namespace UtilmeSdpTransform.Tests;

/// <summary>
/// Parser defects fixed in Phase 3. Each test here was previously in
/// <see cref="KnownDefectTests"/> asserting the broken behaviour; it was rewritten rather than
/// deleted so the fix is visible in the diff, and it now guards against regression.
/// </summary>
/// <remarks>
/// Every fix in this phase is additive in effect — input that parsed before still parses to the same
/// value — with one deliberate exception, covered by
/// <see cref="Field_indicators_are_stripped_only_from_the_start"/>.
/// </remarks>
public class ParserRegressionTests
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

    // ------------------------------------------------------- case-insensitive enums

    [Fact]
    public void Enum_matching_is_case_insensitive()
    {
        // Was: only the exact Display casing was accepted, so RFC 5245's "UDP" threw.
        "a=candidate:1 1 udp 1 192.0.2.3 4 typ host".ToCandidate()
            .Transport.Should().Be(CandidateTransport.Udp);
        "a=candidate:1 1 UDP 1 192.0.2.3 4 typ host".ToCandidate()
            .Transport.Should().Be(CandidateTransport.Udp);
        "a=candidate:1 1 Udp 1 192.0.2.3 4 typ HOST".ToCandidate()
            .Type.Should().Be(CandidateType.Host);
    }

    [Fact]
    public void Enums_are_still_written_in_their_canonical_casing()
    {
        // Parse leniently, write canonically: accepting "UDP" must not let it leak into output.
        "a=candidate:1 1 UDP 1 192.0.2.3 4 typ host".ToCandidate().ToText()
            .Should().Be("a=candidate:1 1 udp 1 192.0.2.3 4 typ host" + Sdp.CRLF);

        "c=in ip4 127.0.0.1".ToConnectionData().ToText()
            .Should().Be("c=IN IP4 127.0.0.1" + Sdp.CRLF);
    }

    // ------------------------------------------------------------ whitespace lines

    [Fact]
    public void Whitespace_only_lines_are_ignored_silently()
    {
        // Was: split with RemoveEmptyEntries happened before Trim, so a line of spaces survived as a
        // token that was empty by the time anything looked at it, and was reported as unsupported.
        var original = Console.Out;
        var captured = new StringWriter();
        Sdp? sdp;
        try
        {
            Console.SetOut(captured);
            sdp = SdpSamples.Text("v=0", "o=- 1 1 IN IP4 127.0.0.1", "s=-", "   ", "t=0 0", "   ")
                .ToSdp();
        }
        finally
        {
            Console.SetOut(original);
        }

        sdp.Should().NotBeNull();
        sdp!.SessionName.Should().Be("-");
        captured.ToString().Should().BeEmpty();
    }

    // ------------------------------------------------------------ anchored indicators

    [Fact]
    public void Field_indicators_are_stripped_only_from_the_start()
    {
        // Was: string.Replace is global, so an indicator inside a value was stripped too and
        // "s=a s=b" parsed to "a b". This is the one Phase 3 change that alters the result for input
        // that already parsed, rather than merely widening what is accepted.
        "s=a s=b".ToSessionName().Should().Be("a s=b");
    }

    [Fact]
    public void A_uri_keeps_a_query_string_that_looks_like_an_indicator()
    {
        // The practical consequence: "u=" inside a query string used to be deleted.
        "u=http://example.com/?u=1&s=2".ToUri()
            .Should().Be(new Uri("http://example.com/?u=1&s=2"));
    }

    [Fact]
    public void An_attribute_value_keeps_text_that_looks_like_its_own_label()
    {
        "a=mid:mid:0".ToMid().Id.Should().Be("mid:0");
    }

    // -------------------------------------------------------------- k=prompt

    [Fact]
    public void EncryptionKey_without_a_value_is_accepted()
    {
        // Was: IndexOutOfRangeException. "k=prompt" is valid SDP; the method takes no value.
        var key = "k=prompt".ToEncryptionKey();

        key.Method.Should().Be(EncryptionKeyMethod.Prompt);
        key.Value.Should().BeNull();
        key.ToText().Should().Be("k=prompt" + Sdp.CRLF);
    }

    [Fact]
    public void EncryptionKey_without_a_value_round_trips_through_a_session()
    {
        var text = Sample("k=prompt");

        text.ToSdp()!.ToText().Should().Be(text);
    }

    // -------------------------------------------------------------- ToSeconds

    [Theory]
    [InlineData("2d", 2 * 86400)]
    [InlineData("3h", 3 * 3600)]
    [InlineData("30m", 30 * 60)]
    [InlineData("45s", 45)]
    public void ToSeconds_handles_every_documented_suffix(string value, long expected)
    {
        // Was: the 'm' and 's' branches trimmed 'h' and multiplied by 3600, so long.Parse was handed
        // "30m" and threw. Minutes and seconds were unusable, not merely mis-scaled.
        value.ToSeconds().Should().Be(expected);
    }

    [Fact]
    public void ToSeconds_rejects_a_bare_number()
    {
        // Unchanged: callers only reach ToSeconds after matching [dhms]$.
        var act = () => "30".ToSeconds();

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void RepeatTime_now_accepts_minute_and_second_suffixes()
    {
        var repeat = "r=7d 1h 0 30m".ToRepeatTime();

        repeat.RepeatInterval.Should().Be(TimeSpan.FromDays(7));
        repeat.ActiveDuration.Should().Be(TimeSpan.FromHours(1));
        repeat.OffsetsFromStartTime.Should().Equal(TimeSpan.Zero, TimeSpan.FromMinutes(30));
    }
}
