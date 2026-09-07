using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UtilmeSdpTransform;

namespace Utilme.SdpTransform;

/// <summary>Converters for the session-level fields — one pair per SDP line type.</summary>
public static partial class ModelExtensions
{
    public static int ToProtocolVersion(this string str)
    {
        var token = str
            .StripPrefix(Sdp.ProtocolVersionIndicator)
            .TrimEnd();
        return int.Parse(token);
    }

    public static string ToProtocolVersionText(this int version) =>
        $"{Sdp.ProtocolVersionIndicator}{version}{Sdp.CRLF}";

    public static Origin ToOrigin(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.OriginIndicator)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Origin
        {
            UserName = tokens[0],
            SessionId = ulong.Parse(tokens[1]),
            SessionVersion = uint.Parse(tokens[2]),
            NetType = tokens[3].EnumFromDisplayName<NetType>(),
            AddrType = tokens[4].EnumFromDisplayName<AddrType>(),
            UnicastAddress = tokens[5]
        };
    }

    public static string ToText(this Origin origin) =>
        $"{Sdp.OriginIndicator}{origin.UserName} {origin.SessionId} {origin.SessionVersion} " +
            $"{origin.NetType.DisplayName()} {origin.AddrType.DisplayName()} {origin.UnicastAddress}" +
            $"{Sdp.CRLF}";

    public static string ToSessionName(this string str)
    {
        var token = str
            .StripPrefix(Sdp.SessionNameIndicator)
            .TrimEnd();
        return token;
    }

    public static string ToSessionNameText(this string name) =>
        $"{Sdp.SessionNameIndicator}{name}{Sdp.CRLF}";

    public static string ToInformation(this string str)
    {
        var token = str
            .StripPrefix(Sdp.InformationIndicator)
            .TrimEnd();
        return token;
    }

    public static string ToInformationText(this string info) =>
        $"{Sdp.InformationIndicator}{info}{Sdp.CRLF}";

    public static Uri ToUri(this string str)
    {
        var token = str
            .StripPrefix(Sdp.UriIndicator)
            .TrimEnd();
        return new Uri(token);
    }

    public static string ToText(this Uri uri) =>
        $"{Sdp.UriIndicator}{uri}{Sdp.CRLF}";

    public static List<string> ToEmailAddresses(this string str)
    {
        var token = str
            .StripPrefix(Sdp.EmailAddressIndicator)
            .TrimEnd();

        List<string> emails = new();

        // Email formats:
        //  x.y@z.org
        //  x.y@z.org (Name Surname)
        //  Name Surname <x.y@z.org>
        var groupA = token.Split(new char[] { ')', '>' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var a in groupA)
        {
            if (a.Contains("("))
            {
                var groupB = a.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
                var id = groupB[1];
                var groupC = groupB[0].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var emailWithId = $"{groupC[groupC.Length - 1]} ({id})";
                if (groupC.Length > 1)
                {
                    var groupD = groupC.Take(groupC.Length - 1).ToArray();
                    foreach (var d in groupD)
                        emails.Add(d);
                }
                emails.Add(emailWithId);
            }
            else if (a.Contains("<"))
            {
                var groupB = a.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                var groupC = groupB[0].Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var numPlainEmails = groupC.Where(g => g.Contains('@')).Count();
                var plainEmails = groupC.Take(numPlainEmails);
                var id = string.Join(" ", 
                    groupC.Skip(numPlainEmails).Take(groupC.Count() - numPlainEmails).ToArray());
                var email = groupB[1];
                var emailWithId = $"{id} <{email}>";
                foreach (var plainEmail in plainEmails)
                    emails.Add(plainEmail);
                emails.Add(emailWithId);
            }
            else
            {
                var groupB = a.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var b in groupB)
                    emails.Add(b);
            }
        }

        return emails;
    }

    public static string ToEmailAddressesText(this IList<string> emails) =>
        $"{Sdp.EmailAddressIndicator}{string.Join(" ", emails)}{Sdp.CRLF}";

    public static List<string> ToPhoneNumbers(this string str)
    {
        var token = str
             .StripPrefix(Sdp.PhoneNumberIndicator)
             .TrimEnd();

        List<string> phones = new();

        // Phone formats:
        //  +x.y@z.org
        //  x.y@z.org (Name Surname)
        //  Name Surname <x.y@z.org>
        var groupA = token.Split(new char[] { ')', '>' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var a in groupA)
        {
            if (a.Contains("("))
            {
                var groupB = a.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
                var id = groupB[1];
                var groupC = groupB[0].Trim().Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                var phoneWithId = $"+{groupC[groupC.Length - 1].Trim()} ({id})";
                if (groupC.Length > 1)
                {
                    var groupD = groupC.Take(groupC.Length - 1).ToArray();
                    foreach (var d in groupD)
                        phones.Add($"+{d.Trim()}");
                }
                phones.Add(phoneWithId);
            }
            else if (a.Contains("<"))
            {
                var groupB = a.Split(new char[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
                var groupC = Regex.Matches(groupB[0].Trim(), @"\+[\d -]+");
                var numPlainPhones = groupC.Count - 1;
                var lenPhones = 0;
                for (var i = 0; i < numPlainPhones; i++)
                {
                    lenPhones += groupC[i].Length;
                    phones.Add(groupC[i].Value.Trim());
                }
                lenPhones += groupC[numPlainPhones].Length;
                var id = groupB[0].Trim().Substring(lenPhones);
                var phone = groupC[1].Value.Trim();
                var phoneWithId = $"{id} <{phone.Trim()}>";
                phones.Add(phoneWithId);
            }
            else
            {
                var groupB = a.Trim().Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var b in groupB)
                    phones.Add($"+{b.Trim()}");
            }
        }

        return phones;
    }

    public static string ToPhoneNumbersText(this IList<string> phones) =>
        $"{Sdp.PhoneNumberIndicator}{string.Join(" ", phones)}{Sdp.CRLF}";

    public static ConnectionData ToConnectionData(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.ConnectionDataIndicator)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new ConnectionData
        {
            NetType = tokens[0].EnumFromDisplayName<NetType>(),
            AddrType = tokens[1].EnumFromDisplayName<AddrType>(),
            ConnectionAddress = tokens[2]
        };
    }

    public static string ToText(this ConnectionData connectionData) =>
        $"{Sdp.ConnectionDataIndicator}{connectionData.NetType.DisplayName()} " +
            $"{connectionData.AddrType.DisplayName()} {connectionData.ConnectionAddress}" +
            $"{Sdp.CRLF}";

    public static Bandwidth ToBandwidth(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.BandwidthIndicator)
             .Split(new char[] { ' ', ':', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Bandwidth
        {
            Type = tokens[0].EnumFromDisplayName<BandwidthType>(),
            Value = int.Parse(tokens[1])
        };
    }

    public static string ToText(this Bandwidth bandwidth) =>
        $"{Sdp.BandwidthIndicator}{bandwidth.Type.DisplayName()}:{bandwidth.Value}" +
            $"{Sdp.CRLF}";

    public static Timing ToTiming(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.TimingIndicator)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new Timing
        {
            StartTime = new DateTime(1900, 1, 1) + TimeSpan.FromSeconds(ulong.Parse(tokens[0])),
            StopTime = new DateTime(1900, 1, 1) + TimeSpan.FromSeconds(ulong.Parse(tokens[1]))
        };
    }

    public static string ToText(this Timing timing) =>
        $"{Sdp.TimingIndicator}{(timing.StartTime - new DateTime(1900, 1, 1)).TotalSeconds} " +
            $"{(timing.StopTime - new DateTime(1900, 1, 1)).TotalSeconds}" +
            $"{Sdp.CRLF}";

    public static RepeatTime ToRepeatTime(this string str)
    {
        var tokens = str
             .StripPrefix(Sdp.RepeatTimeIndicator)
             .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var offsetsStr = tokens.Skip(2);
        var offsets = new List<TimeSpan>();
        foreach (var offsetStr in offsetsStr)
            offsets.Add(TimeSpan.FromSeconds(Regex.IsMatch(offsetStr, @"[dhms]$") ?
                ToSeconds(offsetStr) : double.Parse(offsetStr)));

        return new RepeatTime
        {
            RepeatInterval = TimeSpan.FromSeconds(Regex.IsMatch(tokens[0], @"[dhms]$") ?
                ToSeconds(tokens[0]) : double.Parse(tokens[0])),
            ActiveDuration = TimeSpan.FromSeconds(Regex.IsMatch(tokens[1], @"[dhms]$") ?
                ToSeconds(tokens[1]) : double.Parse(tokens[1])),
            OffsetsFromStartTime = offsets
        };
    }

    public static string ToText(this RepeatTime repeatTime) =>
        $"{Sdp.RepeatTimeIndicator}{repeatTime.RepeatInterval.TotalSeconds} " +
            $"{repeatTime.ActiveDuration.TotalSeconds} " +
            $"{string.Join(" ", repeatTime.OffsetsFromStartTime.Select(o => o.TotalSeconds).ToArray())}" +
            $"{Sdp.CRLF}";

    public static List<TimeZone> ToTimeZones(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.TimeZoneIndicator)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length % 2 != 0)
            throw new FormatException("Timezones should be specified in pairs");
        var pairs = tokens
            .Select((token, idx) => new { idx, token })
            .GroupBy(p => p.idx / 2, p => p.token);

        List<TimeZone> timeZones = new();

        foreach (var pair in pairs)
        {
            var array = pair.ToArray();
            timeZones.Add(new TimeZone 
            { 
                AdjustmentTime = new DateTime(1900, 1, 1) + TimeSpan.FromSeconds(double.Parse(array[0])),
                Offset = TimeSpan.FromSeconds(Regex.IsMatch(array[1], @"[dhms]$") ?
                    ToSeconds(array[1]) : double.Parse(array[1]))
            });
        }

        return timeZones;
    }

    public static string ToText(this IList<TimeZone> timeZones) =>
        $"{Sdp.TimeZoneIndicator}" +
            $"{string.Join(" ", timeZones.Select(z => (z.AdjustmentTime - new DateTime(1900, 1, 1)).TotalSeconds.ToString() + " " + z.Offset.TotalSeconds.ToString()))}" +
            $"{Sdp.CRLF}";

    public static EncryptionKey ToEncryptionKey(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.EncryptionKeyIndicator)
            .Split(new char[] { ':', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        return new EncryptionKey
        {
            Method = tokens[0].EnumFromDisplayName<EncryptionKeyMethod>(),
            // "k=prompt" is valid and carries no value.
            Value = tokens.Length > 1 ? tokens[1] : null
        };
    }

    public static string ToText(this EncryptionKey encryptionKey) =>
        $"{Sdp.EncryptionKeyIndicator}{encryptionKey.Method.DisplayName()}" +
            $"{(encryptionKey.Value is not null ? ":" + encryptionKey.Value : string.Empty)}" +
            $"{Sdp.CRLF}";

    public static MediaDescription ToMediaDescription(this string str)
    {
        var tokens = str
            .StripPrefix(Sdp.MediaDescriptionIndicator)
            .Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new MediaDescription
        {
            Media = tokens[0].EnumFromDisplayName<MediaType>(),
            Port = int.Parse(tokens[1]),
            Proto = tokens[2],
            Fmts = tokens.Skip(3).ToList()
        };
    }

    public static string ToText(this MediaDescription mediaDescription) =>
        $"{Sdp.MediaDescriptionIndicator}" +
            $"{mediaDescription.Media.DisplayName()} {mediaDescription.Port} {mediaDescription.Proto} " +
            $"{string.Join(" ", mediaDescription.Fmts.ToArray())}" +
            $"{Sdp.CRLF}";
}
