using System;
using System.Text;

namespace Utilme.SdpTransform;

/// <summary>Turning an <see cref="Sdp"/> back into SDP text.</summary>
public static partial class ModelExtensions
{
    /// <summary>Writes every set attribute of <paramref name="attributes"/> to the builder.</summary>
    /// <remarks>
    /// Shared by the session and media-description blocks, mirroring
    /// <see cref="TryParseAttribute"/>: whatever one scope can parse, both can write. The order is
    /// fixed and is the order the attributes appear in the output.
    /// </remarks>
    static void WriteAttributes(StringBuilder sb, Attributes attributes)
    {
        // Binary attributes.
        if (attributes.ExtmapAllowMixed.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.ExtmapAllowMixedLabel}{Sdp.CRLF}");
        if (attributes.IceLite.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.IceLiteLabel}{Sdp.CRLF}");
        if (attributes.RtcpMux.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.RtcpMuxLabel}{Sdp.CRLF}");
        if (attributes.RtcpRsize.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.RtcpRsizeLabel}{Sdp.CRLF}");
        if (attributes.SendRecv.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.SendRecvLabel}{Sdp.CRLF}");
        if (attributes.SendOnly.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.SendOnlyLabel}{Sdp.CRLF}");
        if (attributes.RecvOnly.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.RecvOnlyLabel}{Sdp.CRLF}");

        // Value attributes.
        if (attributes.Group is not null)
            sb.Append(attributes.Group.ToText());
        if (attributes.MsidSemantic is not null)
            sb.Append(attributes.MsidSemantic.ToText());
        if (attributes.Mid is not null)
            sb.Append(attributes.Mid.ToText());
        if (attributes.Msid is not null)
            sb.Append(attributes.Msid.ToText());
        if (attributes.IceUfrag is not null)
            sb.Append(attributes.IceUfrag.ToText());
        if (attributes.IcePwd is not null)
            sb.Append(attributes.IcePwd.ToText());
        if (attributes.IceOptions is not null)
            sb.Append(attributes.IceOptions.ToText());
        if (attributes.Fingerprint is not null)
            sb.Append(attributes.Fingerprint.ToText());
        if (attributes.Rtcp is not null)
            sb.Append(attributes.Rtcp.ToText());
        if (attributes.Setup is not null)
            sb.Append(attributes.Setup.ToText());
        if (attributes.SctpPort is not null)
            sb.Append(attributes.SctpPort.ToText());
        if (attributes.MaxMessageSize is not null)
            sb.Append(attributes.MaxMessageSize.ToText());
        if (attributes.Simulcast is not null)
            sb.Append(attributes.Simulcast.ToText());
        if (attributes.Candidates is not null)
            foreach (var c in attributes.Candidates)
                sb.Append(c.ToText());
        if (attributes.EndOfCandidates.HasValue)
            sb.Append($"{Sdp.AttributeIndicator}{Attributes.EndOfCandidatesLabel}{Sdp.CRLF}");
        if (attributes.Ssrcs is not null)
            foreach (var s in attributes.Ssrcs)
                sb.Append(s.ToText());
        if (attributes.SsrcGroups is not null)
            foreach (var sg in attributes.SsrcGroups)
                sb.Append(sg.ToText());
        if (attributes.Rids is not null)
            foreach (var r in attributes.Rids)
                sb.Append(r.ToText());
        if (attributes.Rtpmaps is not null)
            foreach (var r in attributes.Rtpmaps)
                sb.Append(r.ToText());
        if (attributes.Fmtps is not null)
            foreach (var f in attributes.Fmtps)
                sb.Append(f.ToText());
        if (attributes.RtcpFbs is not null)
            foreach (var r in attributes.RtcpFbs)
                sb.Append(r.ToText());
        if (attributes.Extmaps is not null)
            foreach (var e in attributes.Extmaps)
                sb.Append(e.ToText());
    }

    public static string ToText(this Sdp sdp)
    {
        StringBuilder sb = new();

        // Attributes and media descriptions are optional; ToSdp only allocates Attributes when it
        // sees an "a=" line. Substituting an empty instance keeps an attribute-free SDP writable.
        var sessionAttributes = sdp.Attributes ?? NoAttributes;

        // Session fields.
        sb.Append(ToProtocolVersionText(sdp.ProtocolVersion));
        sb.Append(sdp.Origin.ToText());
        sb.Append(ToSessionNameText(sdp.SessionName));
        if (sdp.SessionInformation is not null)
            sb.Append(ToInformationText(sdp.SessionInformation));
        if (sdp.Uri is not null)
            sb.Append(sdp.Uri.ToText());
        if (sdp.EmailAddresses is not null)
            sb.Append(sdp.EmailAddresses.ToEmailAddressesText());
        if (sdp.PhoneNumbers is not null)
            sb.Append(sdp.PhoneNumbers.ToPhoneNumbersText());
        if (sdp.ConnectionData is not null)
            sb.Append(sdp.ConnectionData.ToText());
        if (sdp.Bandwidths is not null)
            foreach (var b in sdp.Bandwidths)
                sb.Append(b.ToText());
        foreach (var t in sdp.Timings)
            sb.Append(t.ToText());
        if (sdp.RepeatTimes is not null)
            foreach (var r in sdp.RepeatTimes)
                sb.Append(r.ToText());
        if (sdp.TimeZones is not null)
            sb.Append(sdp.TimeZones.ToText());
        if (sdp.EncryptionKey is not null)
            sb.Append(sdp.EncryptionKey.ToText());
        
        WriteAttributes(sb, sessionAttributes);

        // Media description fields.
        foreach (var md in sdp.MediaDescriptions ?? Array.Empty<MediaDescription>())
        {
            var mediaAttributes = md.Attributes ?? NoAttributes;

            sb.Append(md.ToText());

            if (md.Information is not null)
                sb.Append(md.Information.ToInformationText());
            if (md.ConnectionData is not null)
                sb.Append(md.ConnectionData.ToText());
            if (md.Bandwidths is not null)
                foreach (var b in md.Bandwidths)
                    sb.Append(b.ToText());
            if (md.EncryptionKey is not null)
                sb.Append(md.EncryptionKey.ToText());

            WriteAttributes(sb, mediaAttributes);
        }

        return sb.ToString();
    }
}
