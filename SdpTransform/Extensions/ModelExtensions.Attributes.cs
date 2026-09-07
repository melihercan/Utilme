using System;
using System.Collections.Generic;
using System.Linq;
using UtilmeSdpTransform;

namespace Utilme.SdpTransform;

/// <summary>Converters for the "a=" attributes — one pair per attribute.</summary>
public static partial class ModelExtensions
{
    // Attributes.

    public static Group ToGroup(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(Group.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Group
        {
            Semantics = tokens[0].EnumFromDisplayName<GroupSemantics>(),
            SemanticsExtensions = tokens.Skip(1).ToArray()
        };
    }

    public static string ToText(this Group group) =>
        $"{Sdp.AttributeIndicator}{Group.Label}{group.Semantics.DisplayName()} " +
            $"{string.Join(" ", group.SemanticsExtensions)}" +
            $"{Sdp.CRLF}";

    public static MsidSemantic ToMsidSemantic(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(MsidSemantic.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new MsidSemantic
        {
            Token = tokens[0],
            IdList = tokens.Length > 1 ? tokens.Skip(1).ToArray() : null
        };
    }
    
    public static string ToText(this MsidSemantic msidSemantic) =>
        $"{Sdp.AttributeIndicator}{MsidSemantic.Label}" +
            $"{msidSemantic.Token} " +
            $"{(msidSemantic.IdList is not null ? string.Join(" ", msidSemantic.IdList) : string.Empty)}" +
            $"{Sdp.CRLF}";

    public static Mid ToMid(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(Mid.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Mid
        {
            Id = tokens[0]
        };
    }

    public static string ToText(this Mid mid) =>
        $"{Sdp.AttributeIndicator}{Mid.Label}" +
            $"{mid.Id}" +
            $"{Sdp.CRLF}";

    public static Msid ToMsid(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(Msid.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Msid
        {
            Id = tokens[0],
            AppData = tokens[1]
        };
    }
    
    public static string ToText(this Msid msid) =>
        $"{Sdp.AttributeIndicator}{Msid.Label}" +
            $"{msid.Id} {msid.AppData}" +
            $"{Sdp.CRLF}";

    public static IceUfrag ToIceUfrag(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(IceUfrag.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new IceUfrag
        {
            Ufrag = tokens[0]
        };
    }

    public static string ToText(this IceUfrag iceUfrag) =>
        $"{Sdp.AttributeIndicator}{IceUfrag.Label}" +
            $"{iceUfrag.Ufrag}" +
            $"{Sdp.CRLF}";

    public static IcePwd ToIcePwd(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(IcePwd.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new IcePwd
        {
            Password = tokens[0]
        };
    }

    public static string ToText(this IcePwd icePwd) =>
        $"{Sdp.AttributeIndicator}{IcePwd.Label}" +
            $"{icePwd.Password}" +
            $"{Sdp.CRLF}";

    public static IceOptions ToIceOptions(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(IceOptions.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new IceOptions
        {
            Tags = tokens
        };
    }

    public static string ToText(this IceOptions iceOptions) =>
        $"{Sdp.AttributeIndicator}{IceOptions.Label}" +
            $"{string.Join(" ", iceOptions.Tags)}" +
            $"{Sdp.CRLF}";

    public static Fingerprint ToFingerprint(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(Fingerprint.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Fingerprint
        {
            HashFunction = tokens[0].EnumFromDisplayName<HashFunction>(), 
            HashValue = HexadecimalStringToByteArray(tokens[1].Replace(":", string.Empty))
        };
    }

    public static string ToText(this Fingerprint fingerprint) =>
        $"{Sdp.AttributeIndicator}{Fingerprint.Label}" +
            $"{fingerprint.HashFunction.DisplayName()} " +
            $"{BitConverter.ToString(fingerprint.HashValue).Replace("-", ":")}" +
            $"{Sdp.CRLF}";

    public static Rtcp ToRtcp(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(Rtcp.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Rtcp
        {
            Port = int.Parse(tokens[0]),
            NetType = tokens.Length > 3 ? tokens[1].EnumFromDisplayName<NetType>() : null,
            AddrType = tokens.Length > 3 ? tokens[2].EnumFromDisplayName<AddrType>() : null,
            ConnectionAddress = tokens.Length > 3 ? tokens[3] : null
        };
    }

    public static string ToText(this Rtcp rtcp) =>
        $"{Sdp.AttributeIndicator}{Rtcp.Label}" +
            $"{rtcp.Port}{(rtcp.NetType.HasValue ? $" {rtcp.NetType.DisplayName()}" : string.Empty)}" +
            $"{(rtcp.AddrType.HasValue ? $" {rtcp.AddrType.DisplayName()}" : string.Empty)}" +
            $"{(rtcp.ConnectionAddress is not null ? $" {rtcp.ConnectionAddress}" : string.Empty)}" +
            $"{Sdp.CRLF}";

    public static Setup ToSetup(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(Setup.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Setup
        {
            Role = tokens[0].EnumFromDisplayName<SetupRole>()
        };
    }

    public static string ToText(this Setup setup) =>
        $"{Sdp.AttributeIndicator}{Setup.Label}" +
            $"{setup.Role.DisplayName()}" +
            $"{Sdp.CRLF}";

    public static SctpPort ToSctpPort(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(SctpPort.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new SctpPort
        {
            Port = int.Parse(tokens[0])
        };
    }

    public static string ToText(this SctpPort sctpPort) =>
        $"{Sdp.AttributeIndicator}{SctpPort.Label}" +
            $"{sctpPort.Port}" +
            $"{Sdp.CRLF}";

    public static MaxMessageSize ToMaxMessageSize(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(MaxMessageSize.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new MaxMessageSize
        {
            Size = int.Parse(tokens[0])
        };
    }

    public static string ToText(this MaxMessageSize size) =>
        $"{Sdp.AttributeIndicator}{MaxMessageSize.Label}" +
            $"{size.Size}" +
            $"{Sdp.CRLF}";

    public static Candidate ToCandidate(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(Candidate.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var candidate = new Candidate
        {
            Foundation = tokens[0],
            ComponentId = int.Parse(tokens[1]),
            Transport = tokens[2].EnumFromDisplayName<CandidateTransport>(),
            Priority = int.Parse(tokens[3]),
            ConnectionAddress = tokens[4],
            Port = int.Parse(tokens[5]),
            Type = tokens[7].EnumFromDisplayName<CandidateType>(),
        };
        var idx = 8;
        if (tokens.Length > 8 && tokens[8] == Candidate.Raddr)
        {
            candidate.RelAddr = tokens[9];
            candidate.RelPort = int.Parse(tokens[11]);
            idx += 4;
        }

        var extensions = new List <(string, string)>(); 
        while (tokens.Length > idx)
        {
            extensions.Add((tokens[idx++], tokens[idx++]));
        }
        if (extensions.Count > 0)
            candidate.Extensions = extensions.ToArray();
        return candidate;
    }

    public static string ToText(this Candidate candidate) =>
        $"{Sdp.AttributeIndicator}{Candidate.Label}" +
            $"{candidate.Foundation} {candidate.ComponentId} {candidate.Transport.DisplayName()} " +
            $"{candidate.Priority} {candidate.ConnectionAddress} {candidate.Port} {Candidate.Typ} " +
            $"{candidate.Type.DisplayName()}" +
            $"{(candidate.RelAddr is not null ? $" {Candidate.Raddr} {candidate.RelAddr}" : string.Empty)}" +
            $"{(candidate.RelPort.HasValue ? $" {Candidate.Rport} {candidate.RelPort}" : string.Empty)}" +
            $"{(candidate.Extensions is null ? string.Empty : string.Join("", candidate.Extensions.Select(pair => " " + pair.Item1 + " " + pair.Item2).ToArray()))}" +
            $"{Sdp.CRLF}";

    public static Ssrc ToSsrc(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(Ssrc.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var attributeAndValue = tokens[1].Split(':');
        return new Ssrc
        {
            Id = uint.Parse(tokens[0]),
            Attribute = attributeAndValue[0],
            Value = attributeAndValue.Length > 1 ? attributeAndValue[1] : null
         };
    }

    public static string ToText(this Ssrc ssrc) =>
        $"{Sdp.AttributeIndicator}{Ssrc.Label}" +
            $"{ssrc.Id} {ssrc.Attribute}{(ssrc.Value == null ? string.Empty : ":" + ssrc.Value)}" +
            $"{Sdp.CRLF}";

    public static SsrcGroup ToSsrcGroup(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(SsrcGroup.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new SsrcGroup
        {
            Semantics = tokens[0],
            SsrcIds = tokens.Skip(1).ToArray()
        };
    }

    public static string ToText(this SsrcGroup ssrcGroup) =>
        $"{Sdp.AttributeIndicator}{SsrcGroup.Label}" +
            $"{ssrcGroup.Semantics} {string.Join(" ", ssrcGroup.SsrcIds)}" +
            $"{Sdp.CRLF}";

    public static Rid ToRid(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(Rid.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Rid rid = new()
        {
            Id = tokens[0],
            Direction = tokens[1].EnumFromDisplayName<RidDirection>(),
        };

        if (tokens.Length > 2)
        {
            var subTokens = tokens[2].Split(';');

            var fmtSubToken = subTokens.SingleOrDefault(s => s.StartsWith("pt="));
            if (fmtSubToken is not null)
            {
                var fmtList = fmtSubToken
                    .Replace("pt=", string.Empty)
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                rid.FmtList = fmtList;
            }

            var restrictions = subTokens.Where(s => !s.StartsWith("pt=")).ToArray();
            if (restrictions.Length > 0)
                rid.Restrictions = restrictions;
        }

        return rid;
    }

    public static string ToText(this Rid rid)
    {
        // "pt=..." and the restrictions form a single ';'-separated list, so the separator belongs
        // between them rather than unconditionally in front of the restrictions.
        var parameters = new List<string>();
        if (rid.FmtList is not null)
            parameters.Add("pt=" + string.Join(",", rid.FmtList));
        if (rid.Restrictions is not null)
            parameters.AddRange(rid.Restrictions);

        return $"{Sdp.AttributeIndicator}{Rid.Label}" +
            $"{rid.Id} {rid.Direction.DisplayName()}" +
            $"{(parameters.Count > 0 ? " " + string.Join(";", parameters) : string.Empty)}" +
            $"{Sdp.CRLF}";
    }

    public static Simulcast ToSimulcast(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(Simulcast.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var subTokens = tokens[1].Split(';');
        return new Simulcast
        {
            Direction = tokens[0].EnumFromDisplayName<RidDirection>(),
            IdList = subTokens
        };

    }

    public static string ToText(this Simulcast simulcast) =>
        $"{Sdp.AttributeIndicator}{Simulcast.Label}" +
            $"{simulcast.Direction.DisplayName()} " +
            $"{string.Join(";", simulcast.IdList)}" +
            $"{Sdp.CRLF}";

    public static Rtpmap ToRtpmap(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(Rtpmap.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var subTokens = tokens[1].Split('/');
        return new Rtpmap
        {
            PayloadType = int.Parse(tokens[0]),
            EncodingName = subTokens[0],
            ClockRate = int.Parse(subTokens[1]),
            Channels = subTokens.Length > 2 ? int.Parse(subTokens[2]) : null
        };
    }

    public static string ToText(this Rtpmap rtpmap) =>
        $"{Sdp.AttributeIndicator}{Rtpmap.Label}" +
            $"{rtpmap.PayloadType} {rtpmap.EncodingName}/{rtpmap.ClockRate}" +
            $"{(rtpmap.Channels.HasValue ? "/" + rtpmap.Channels : string.Empty)}" +
            $"{Sdp.CRLF}";

    public static Fmtp ToFmtp(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.AttributeIndicator)
             .StripPrefix(Fmtp.Label)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Fmtp
        {
            PayloadType = int.Parse(tokens[0]),
            Value = tokens[1],
        };
    }

    // Dictionary
    //  {
    //      { "level-asymmetry-allowed", "1" },
    //      { "packetization-mode", "0" },
    //      { "profile-level-id", "42e01f" }
    //  }
    // Fmtp
    // {
    //  PayloadType = 108,
    //  Value = "level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f"
    // }
    // a=fmtp:108 level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f
    public static Fmtp ToFmtp(this Dictionary<string, object> dictionary, int payloadType)
    {
        Fmtp fmtp = new()
        {
            PayloadType = payloadType,
            Value = string.Empty
        };

        foreach (var key in dictionary.Keys)
        {
            if (!string.IsNullOrEmpty(fmtp.Value))
                fmtp.Value += ";";
            fmtp.Value += $"{key}={dictionary[key]}";
        }

        return fmtp;
    }

    public static string ToText(this Fmtp fmtp) =>
            $"{Sdp.AttributeIndicator}{Fmtp.Label}" +
                $"{fmtp.PayloadType} {fmtp.Value}" +
                $"{Sdp.CRLF}";

    // a=fmtp:108 level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f
    // Fmtp
    // {
    //  PayloadType = 108,
    //  Value = "level-asymmetry-allowed=1;packetization-mode=0;profile-level-id=42e01f"
    // }
    // Dictionary
    //  {
    //      { "level-asymmetry-allowed", "1" },
    //      { "packetization-mode", "0" },
    //      { "profile-level-id", "42e01f" }
    //  }
    public static Dictionary<string, object> ToDictionary(this Fmtp fmtp)
    {
        Dictionary<string, object/*string*/> dictionary = new();

        var tokens = fmtp.Value.Split(';');
        foreach (var token in tokens)
        {
            var subTokens = token.Split('=');
            if (subTokens.Length == 1)
                dictionary.Add(subTokens[0], null);
            else
            {
                if (int.TryParse(subTokens[1], out int n))
                    dictionary.Add(subTokens[0], n);
                else
                    dictionary.Add(subTokens[0], subTokens[1]);
            }
        }
        return dictionary;
    }

    public static RtcpFb ToRtcpFb(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(RtcpFb.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new RtcpFb
        {
            PayloadType = int.Parse(tokens[0]),
            Type = tokens[1],
            SubType = tokens.Length > 2 ? tokens[2] : null
        };
    }

    public static string ToText(this RtcpFb rtcpFb) =>
        $"{Sdp.AttributeIndicator}{RtcpFb.Label}" +
            $"{rtcpFb.PayloadType} {rtcpFb.Type}" +
            $"{(rtcpFb.SubType is not null ? " " + rtcpFb.SubType : string.Empty)}" +
            $"{Sdp.CRLF}";

    public static Extmap ToExtmap(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.AttributeIndicator)
            .StripPrefix(Extmap.Label)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var subtokens = tokens[0].Split('/');
        return new Extmap
        {
            Value = int.Parse(subtokens[0]),
            Direction = subtokens.Length > 1 ? subtokens[1].EnumFromDisplayName<Direction>() : null,
            Uri = new Uri(tokens[1]),
            ExtensionAttributes = tokens.Length > 2 ? tokens[2] : null 
        };
    }

    public static string ToText(this Extmap extmap) =>
        $"{Sdp.AttributeIndicator}{Extmap.Label}" +
             $"{extmap.Value}{(extmap.Direction.HasValue ? "/" + extmap.Direction.DisplayName() : string.Empty)} " +
             $"{extmap.Uri}" +
             $"{(extmap.ExtensionAttributes is not null ? " " + extmap.ExtensionAttributes : string.Empty)}" +
             $"{Sdp.CRLF}";
}
