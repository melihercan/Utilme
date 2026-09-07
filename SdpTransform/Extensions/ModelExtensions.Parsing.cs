using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilme.SdpTransform;

/// <summary>Turning SDP text into an <see cref="Sdp"/>: the entry points and the attribute dispatcher.</summary>
public static partial class ModelExtensions
{
    // https://www.iana.org/assignments/sdp-parameters/sdp-parameters.xhtml#sdp-parameters-12

    /// <summary>Stand-in for a missing Attributes block; every property on it is unset.</summary>
    static readonly Attributes NoAttributes = new();

    /// <summary>
    /// Parses one attribute line body (everything after "a=") into <paramref name="attributes"/>.
    /// </summary>
    /// <returns><see langword="false"/> when the attribute is not recognised.</returns>
    /// <remarks>
    /// Shared by the session and media-description branches. They used to carry separate chains, and
    /// the session one recognised only five attributes, so anything else valid at session level was
    /// silently dropped. One dispatcher keeps the two scopes in step by construction.
    /// </remarks>
    static bool TryParseAttribute(Attributes attributes, string attr)
    {
            // Binary attributes.
            if (attr.StartsWith(Attributes.ExtmapAllowMixedLabel))
                attributes.ExtmapAllowMixed = true;
            else if (attr.StartsWith(Attributes.IceLiteLabel))
                attributes.IceLite = true;
            else if (attr.StartsWith(Attributes.RtcpMuxLabel))
                attributes.RtcpMux = true;
            else if (attr.StartsWith(Attributes.RtcpRsizeLabel))
                attributes.RtcpRsize = true;
            else if (attr.StartsWith(Attributes.SendRecvLabel))
                attributes.SendRecv = true;
            else if (attr.StartsWith(Attributes.SendOnlyLabel))
                attributes.SendOnly = true;
            else if (attr.StartsWith(Attributes.RecvOnlyLabel))
                attributes.RecvOnly = true;
            else if (attr.StartsWith(Attributes.EndOfCandidatesLabel))
                attributes.EndOfCandidates = true;

            // Value attributes.
            else if (attr.StartsWith(Group.Label))
                attributes.Group = attr.ToGroup();
            else if (attr.StartsWith(MsidSemantic.Label))
                attributes.MsidSemantic = attr.ToMsidSemantic();
            else if (attr.StartsWith(Mid.Label))
                attributes.Mid = attr.ToMid();
            else if (attr.StartsWith(Msid.Label))
                attributes.Msid = attr.ToMsid();
            else if (attr.StartsWith(IceUfrag.Label))
                attributes.IceUfrag = attr.ToIceUfrag();
            else if (attr.StartsWith(IcePwd.Label))
                attributes.IcePwd = attr.ToIcePwd();
            else if (attr.StartsWith(IceOptions.Label))
                attributes.IceOptions = attr.ToIceOptions();
            else if (attr.StartsWith(Fingerprint.Label))
                attributes.Fingerprint = attr.ToFingerprint();
            else if (attr.StartsWith(Rtcp.Label))
                attributes.Rtcp = attr.ToRtcp();
            else if (attr.StartsWith(Setup.Label))
                attributes.Setup = attr.ToSetup();
            else if (attr.StartsWith(SctpPort.Label))
                attributes.SctpPort = attr.ToSctpPort();
            else if (attr.StartsWith(MaxMessageSize.Label))
                attributes.MaxMessageSize = attr.ToMaxMessageSize();
            else if (attr.StartsWith(Simulcast.Label))
                attributes.Simulcast = attr.ToSimulcast();
            else if (attr.StartsWith(Candidate.Label))
            {
                attributes.Candidates ??= new List<Candidate>();
                attributes.Candidates.Add(attr.ToCandidate());
            }
            else if (attr.StartsWith(Ssrc.Label))
            {
                attributes.Ssrcs ??= new List<Ssrc>();
                attributes.Ssrcs.Add(attr.ToSsrc());
            }
            else if (attr.StartsWith(SsrcGroup.Label))
            {
                attributes.SsrcGroups ??= new List<SsrcGroup>();
                attributes.SsrcGroups.Add(attr.ToSsrcGroup());
            }
            else if (attr.StartsWith(Rid.Label))
            {
                attributes.Rids ??= new List<Rid>();
                attributes.Rids.Add(attr.ToRid());
            }
            else if (attr.StartsWith(Rtpmap.Label))
            {
                attributes.Rtpmaps ??= new List<Rtpmap>();
                attributes.Rtpmaps.Add(attr.ToRtpmap());
            }
            else if (attr.StartsWith(Fmtp.Label))
            {
                attributes.Fmtps ??= new List<Fmtp>();
                attributes.Fmtps.Add(attr.ToFmtp());
            }
            else if (attr.StartsWith(RtcpFb.Label))
            {
                attributes.RtcpFbs ??= new List<RtcpFb>();
                attributes.RtcpFbs.Add(attr.ToRtcpFb());
            }
            else if (attr.StartsWith(Extmap.Label))
            {
                attributes.Extmaps ??= new List<Extmap>();
                attributes.Extmaps.Add(attr.ToExtmap());
            }

            else
                return false;

        return true;
    }

    /// <summary>Parses SDP text, returning <see langword="null"/> if it cannot be parsed.</summary>
    /// <param name="str">The SDP text.</param>
    /// <remarks>
    /// Problems are discarded. Use <see cref="ToSdpResult"/> when you need to know why parsing
    /// failed or which lines were skipped.
    /// </remarks>
    public static Sdp ToSdp(this string str) => str.ToSdpResult().Sdp;

    /// <summary>Parses SDP text, reporting every problem encountered.</summary>
    /// <param name="str">The SDP text.</param>
    /// <returns>
    /// A result carrying the parsed <see cref="Sdp"/> — or <see langword="null"/> when parsing could
    /// not complete — together with a diagnostic for every line that was not understood.
    /// </returns>
    /// <remarks>
    /// Never throws. Parsing is all-or-nothing in the same way <see cref="ToSdp"/> has always been:
    /// an unparseable line aborts the parse and yields no <see cref="Sdp"/>. Lines that are merely
    /// unrecognised are skipped and reported as warnings.
    /// </remarks>
    public static SdpParseResult ToSdpResult(this string str)
    {
        var diagnostics = new List<SdpDiagnostic>();

        void Warn(int lineNumber, string line, string message) =>
            diagnostics.Add(new SdpDiagnostic(
                SdpDiagnosticSeverity.Warning, lineNumber, line, message));

        try
        {
            Sdp sdp = new();

            // Trim first, then discard blanks: splitting with RemoveEmptyEntries beforehand lets a
            // whitespace-only line survive as a token that is empty by the time anything looks at it.
            // Line numbers are kept so diagnostics can point at the original text.
            var tokens = str
                .Split(new string[] { Sdp.CRLF }, StringSplitOptions.None)
                .Select((line, index) => (Text: line.Trim(), Number: index + 1))
                .Where(t => t.Text.Length > 0)
                .ToArray();

            var idx = 0;

            // Session fields.
            foreach (var (token, lineNumber) in tokens)
            {
                if (token.StartsWith(Sdp.ProtocolVersionIndicator))
                    sdp.ProtocolVersion = token.ToProtocolVersion();
                else if (token.StartsWith(Sdp.OriginIndicator))
                    sdp.Origin = token.ToOrigin();
                else if (token.StartsWith(Sdp.SessionNameIndicator))
                    sdp.SessionName = token.ToSessionName();
                else if (token.StartsWith(Sdp.InformationIndicator))
                    sdp.SessionInformation = token.ToInformation();
                else if (token.StartsWith(Sdp.UriIndicator))
                    sdp.Uri = token.ToUri();
                else if (token.StartsWith(Sdp.EmailAddressIndicator))
                    sdp.EmailAddresses = token.ToEmailAddresses();
                else if (token.StartsWith(Sdp.PhoneNumberIndicator))
                    sdp.PhoneNumbers = token.ToPhoneNumbers();
                else if (token.StartsWith(Sdp.ConnectionDataIndicator))
                    sdp.ConnectionData = token.ToConnectionData();
                else if (token.StartsWith(Sdp.BandwidthIndicator))
                {
                    sdp.Bandwidths ??= new List<Bandwidth>();
                    sdp.Bandwidths.Add(token.ToBandwidth());
                }
                else if (token.StartsWith(Sdp.TimingIndicator))
                {
                    sdp.Timings ??= new List<Timing>();
                    sdp.Timings.Add(token.ToTiming());
                }
                else if (token.StartsWith(Sdp.RepeatTimeIndicator))
                {
                    sdp.RepeatTimes ??= new List<RepeatTime>();
                    sdp.RepeatTimes.Add(token.ToRepeatTime());
                }
                else if (token.StartsWith(Sdp.TimeZoneIndicator))
                    sdp.TimeZones = token.ToTimeZones();
                else if (token.StartsWith(Sdp.EncryptionKeyIndicator))
                    sdp.EncryptionKey = token.ToEncryptionKey();
                else if (token.StartsWith(Sdp.AttributeIndicator))
                {
                    sdp.Attributes ??= new Attributes();
                    var attr = token.Substring(Sdp.AttributeIndicator.Length);

                    if (!TryParseAttribute(sdp.Attributes, attr))
                        Warn(lineNumber, token, $"Unsupported session attribute: {attr}");
                }
                else if (token.StartsWith(Sdp.MediaDescriptionIndicator))
                    break;
                else
                    Warn(lineNumber, token, $"Unsupported session field: {token}");

                idx++;
            }

            // Media description fields.
            tokens = tokens.Skip(idx).ToArray();
            sdp.MediaDescriptions = new List<MediaDescription>();

            MediaDescription md = null;

            foreach (var (token, lineNumber) in tokens)
            {
                if (token.StartsWith(Sdp.MediaDescriptionIndicator))
                {
                    if (md is not null)
                        sdp.MediaDescriptions.Add(md);
                    md = token.ToMediaDescription();
                }
                else if (token.StartsWith(Sdp.InformationIndicator))
                    md.Information = token.ToInformation();
                else if (token.StartsWith(Sdp.ConnectionDataIndicator))
                    md.ConnectionData = token.ToConnectionData();
                else if (token.StartsWith(Sdp.BandwidthIndicator))
                {
                    md.Bandwidths ??= new List<Bandwidth>();
                    md.Bandwidths.Add(token.ToBandwidth());
                }
                else if (token.StartsWith(Sdp.EncryptionKeyIndicator))
                    md.EncryptionKey = token.ToEncryptionKey();
                else if (token.StartsWith(Sdp.AttributeIndicator))
                {
                    md.Attributes ??= new Attributes();
                    var attr = token.Substring(Sdp.AttributeIndicator.Length);

                    if (!TryParseAttribute(md.Attributes, attr))
                        Warn(lineNumber, token, $"Unsupported media description attribute: {attr}");
                }
                else
                    Warn(lineNumber, token, $"Unsupported media description field: {token}");

            }

            if (md is not null)
                sdp.MediaDescriptions.Add(md);

            return new SdpParseResult(sdp, diagnostics);
        }
        catch (Exception ex)
        {
            // Parsing stays all-or-nothing so ToSdp keeps returning null exactly when it always has;
            // what changes is that the reason is no longer thrown away.
            diagnostics.Add(new SdpDiagnostic(
                SdpDiagnosticSeverity.Error, 0, string.Empty,
                $"{ex.GetType().Name}: {ex.Message}"));
            return new SdpParseResult(null, diagnostics);
        }
    }
}
