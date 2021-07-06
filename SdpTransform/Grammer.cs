using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilmeSdpTransform
{
    class Grammer
    {
        public class Rule
        {
            public string Name { get; set; }
            public string Push { get; set; }
            public Regex Reg { get; set; }
            public string[] Names { get; set; }
            public char[] Types { get; set; }
            public string Format { get; set; }
            public Func<string, string> FormatFunc { get; set; }
        };

        //public readonly Dictionary<char, List<Rule>> Rules;
        public readonly Dictionary<char, Rule[]> Rules;

        public Grammer()
        {
            Rules = new()
            {
                {
                    'v',
                    // v=0
                    new Rule[]
                    {
                        new()
                        {
                            Name = "version",
                            Reg = new Regex("^(\\d*)$")
                        }
                    }
                }
                /***
                                ,
                                {
                                    'o',
                                    // o=- 20518 0 IN IP4 203.0.113.1
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "origin",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(\\S*) (\\d*) (\\d*) (\\S*) IP(\\d) (\\S*)"),
                                            Names = new()
                                            { "username", "sessionId", "sessionVersion", "netType", "ipVer", "address" },
                                            Types = new()
                                            { 's', 'u', 'u', 's', 'd', 's' },
                                            Format =
                                        "%s %d %d %s IP%d %s"
                                        }
                                    }
                                },

                                {
                                    's',
                                    // s=-
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "name",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"
                                        }
                                    }
                                },

                                {
                                    'i',
                                    // i=foo
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "description",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"
                                        }
                                    }
                                },

                                {
                                    'u',
                                    // u=https://foo.com
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "uri",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"
                                        }
                                    }
                                },

                                {
                                    'e',
                                    // e=alice@foo.com
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "email",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"
                                        }
                                    }
                                },

                                {
                                    'p',
                                    // p=+12345678
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "phone",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"
                                        }
                                    }
                                },

                                {
                                    'z',
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "timezones",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"
                                        }
                                    }
                                },

                                {
                                    'r',
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "repeats",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"
                                        }
                                    }
                                },

                                {
                                    't',
                                    // t=0 0
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "timing",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(\\d*) (\\d*)"),
                                            Names = new()
                                            { "start", "stop" },
                                            Types = new()
                                            { 'd', 'd' },
                                            Format =
                                        "%d %d"
                                        }
                                    }
                                },

                                {
                                    'c',
                                    // c=IN IP4 10.47.197.26
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "connection",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^IN IP(\\d) ([^\\\\S/]*)(?:/(\\d*))?"),
                                            Names = new()
                                            { "version", "ip", "ttl" },
                                            Types = new()
                                            { 'd', 's', 'd' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "ttl")
                                                ? "IN IP%d %s/%d"
                                                : "IN IP%d %s";
                                        }
                                        }
                                    }
                                },

                                {
                                    'b',
                                    // b=AS:4000
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                    "",
                                            Push =
                                    "bandwidth",
                                            Reg =
                                    new Regex("^(TIAS|AS|CT|RR|RS):(\\d*)"),
                                            Names = new()
                                            { "type", "limit" },
                                            Types = new()
                                            { 's', 'd' },
                                            Format =
                                    "%s:%d"
                                        }
                                    }
                                },

                                {
                                    'm',
                                    // m=video 51744 RTP/AVP 126 97 98 34 31
                                    new()
                                    {
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(\\w*) (\\d*)(?:/(\\d*))? ([\\w\\/]*)(?: (.*))?"),
                                            Names = new()
                                            {
                                                "type",
                                                "port",
                                                "numPorts",
                                                "protocol",
                                                "payloads"
                                            },
                                            Types = new()
                                            { 's', 'd', 'd', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "numPorts")
                                                ? "%s %d/%d %s %s"
                                                : "%s %d%v %s %s";
                                        }
                                        }
                                    }
                                },

                                {
                                    'a',
                                    new()
                                    {
                                        // a=rtpmap:110 opus/48000/2
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "rtp",
                                            Reg =
                                        new Regex("^rtpmap:(\\d*) ([\\w\\-\\.]*)(?:\\s*\\/(\\d*)(?:\\s*\\/(\\S*))?)?"),
                                            Names = new()
                                            { "payload", "codec", "rate", "encoding" },
                                            Types = new()
                                            { 'd', 's', 'd', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "encoding")
                                                    ? "rtpmap:%d %s/%s/%s"
                                                    : HasValue(o, "rate")
                                                        ? "rtpmap:%d %s/%s"
                                                        : "rtpmap:%d %s";
                                        }
                                        },

                                        // a=fmtp:108 profile-level-id=24;object=23;bitrate=64000
                                        // a=fmtp:111 minptime=10; useinbandfec=1
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "fmtp",
                                            Reg =
                                        new Regex("^fmtp:(\\d*) (.*)"),
                                            Names = new()
                                            { "payload", "config" },
                                            Types = new()
                                            { 'd', 's' },
                                            Format =
                                        "fmtp:%d %s"
                                        },

                                        // a=control:streamid=0
                                        new()
                                        {
                                            Name =
                                        "control",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^control:(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "control:%s"

                                        },

                                        // a=rtcp:65179 IN IP4 193.84.77.194
                                        new()
                                        {
                                            Name =
                                        "rtcp",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^rtcp:(\\d*)(?: (\\S*) IP(\\d) (\\S*))?"),
                                            Names = new()
                                            { "port", "netType", "ipVer", "address" },
                                            Types = new()
                                            { 'd', 's', 'd', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "address")
                                                    ? "rtcp:%d %s IP%d %s"
                                                    : "rtcp:%d";
                                        }
                                        },

                                        // a=rtcp-fb:98 trr-int 100
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "rtcpFbTrrInt",
                                            Reg =
                                        new Regex("^rtcp-fb:(\\*|\\d*) trr-int (\\d*)"),
                                            Names = new()
                                            { "payload", "value" },
                                            Types = new()
                                            { 's', 'd' },
                                            Format =
                                        "rtcp-fb:%s trr-int %d"

                                        },

                                        // a=rtcp-fb:98 nack rpsi
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "rtcpFb",
                                            Reg =
                                        new Regex("^rtcp-fb:(\\*|\\d*) ([\\w\\-_]*)(?: ([\\w\\-_]*))?"),
                                            Names = new()
                                            { "payload", "type", "subtype" },
                                            Types = new()
                                            { 's', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "subtype")
                                                    ? "rtcp-fb:%s %s %s"
                                                    : "rtcp-fb:%s %s";
                                        }
                                        },

                                        // a=extmap:2 urn:ietf:params:rtp-hdrext:toffset
                                        // a=extmap:1/recvonly URI-gps-string
                                        // a=extmap:3 urn:ietf:params:rtp-hdrext:encrypt urn:ietf:params:rtp-hdrext:smpte-tc 25@600/24
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "ext",
                                            Reg =
                                        new Regex("^extmap:(\\d+)(?:\\/(\\w+))?(?: (urn:ietf:params:rtp-hdrext:encrypt))? (\\S*)(?: (\\S*))?"),
                                            Names = new()
                                            { "value", "direction", "encrypt-uri", "uri", "config" },
                                            Types = new()
                                            { 'd', 's', 's', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return new string("extmap:%d") +
                                                    (HasValue(o, "direction") ? "/%s" : "%v") +
                                                    (HasValue(o, "encrypt-uri") ? " %s" : "%v") +
                                                    " %s" +
                                                    (HasValue(o, "config") ? " %s" : "");
                                        }
                                        },

                                        // a=extmap-allow-mixed
                                        new()
                                        {
                                            Name =
                                        "extmapAllowMixed",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(extmap-allow-mixed)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"

                                        },

                                        // a=crypto:1 AES_CM_128_HMAC_SHA1_80 inline:PS1uQCVeeCFCanVmcjkpPywjNWhcYD0mXXtxaVBR|2^20|1:32
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "crypto",
                                            Reg =
                                        new Regex("^crypto:(\\d*) ([\\w_]*) (\\S*)(?: (\\S*))?"),
                                            Names = new()
                                            { "id", "suite", "config", "sessionConfig" },
                                            Types = new()
                                            { 'd', 's', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "sessionConfig")
                                                    ? "crypto:%d %s %s %s"
                                                    : "crypto:%d %s %s";
                                        }
                                        },

                                        // a=setup:actpass
                                        new()
                                        {
                                            Name =
                                        "setup",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^setup:(\\w*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "setup:%s"

                                        },

                                        // a=mid:1
                                        new()
                                        {
                                            Name =
                                        "mid",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^mid:([^\\s]*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "mid:%s"

                                        },

                                        // a=msid:0c8b064d-d807-43b4-b434-f92a889d8587 98178685-d409-46e0-8e16-7ef0db0db64a
                                        new()
                                        {
                                            Name =
                                        "msid",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^msid:(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "msid:%s"

                                        },

                                        // a=ptime:20
                                        new()
                                        {
                                            Name =
                                        "ptime",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^ptime:(\\d*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 'd' },
                                            Format =
                                        "ptime:%d"

                                        },

                                        // a=maxptime:60
                                        new()
                                        {
                                            Name =
                                        "maxptime",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^maxptime:(\\d*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 'd' },
                                            Format =
                                        "maxptime:%d"

                                        },

                                        // a=sendrecv
                                        new()
                                        {
                                            Name =
                                        "direction",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(sendrecv|recvonly|sendonly|inactive)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"

                                        },

                                        // a=ice-lite
                                        new()
                                        {
                                            Name =
                                        "icelite",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(ice-lite)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"

                                        },

                                        // a=ice-ufrag:F7gI
                                        new()
                                        {
                                            Name =
                                        "iceUfrag",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^ice-ufrag:(\\S*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "ice-ufrag:%s"

                                        },

                                        // a=ice-pwd:x9cml/YzichV2+XlhiMu8g
                                        new()
                                        {
                                            Name =
                                        "icePwd",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^ice-pwd:(\\S*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "ice-pwd:%s"

                                        },

                                        // a=fingerprint:SHA-1 00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD:EE:FF:00:11:22:33
                                        new()
                                        {
                                            Name =
                                        "fingerprint",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^fingerprint:(\\S*) (\\S*)"),
                                            Names = new()
                                            { "type", "hash" },
                                            Types = new()
                                            { 's', 's' },
                                            Format =
                                        "fingerprint:%s %s"

                                        },

                                        // a=candidate:0 1 UDP 2113667327 203.0.113.1 54400 typ host
                                        // a=candidate:1162875081 1 udp 2113937151 192.168.34.75 60017 typ host generation 0 network-id 3 network-cost 10
                                        // a=candidate:3289912957 2 udp 1845501695 193.84.77.194 60017 typ srflx raddr 192.168.34.75 rport 60017 generation 0 network-id 3 network-cost 10
                                        // a=candidate:229815620 1 tcp 1518280447 192.168.150.19 60017 typ host tcptype active generation 0 network-id 3 network-cost 10
                                        // a=candidate:3289912957 2 tcp 1845501695 193.84.77.194 60017 typ srflx raddr 192.168.34.75 rport 60017 tcptype passive generation 0 network-id 3 network-cost 10
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "candidates",
                                            Reg =
                                        new Regex("^candidate:(\\S*) (\\d*) (\\S*) (\\d*) (\\S*) (\\d*) typ (\\S*)(?: raddr (\\S*) rport (\\d*))?(?: tcptype (\\S*))?(?: generation (\\d*))?(?: network-id (\\d*))?(?: network-cost (\\d*))?"),
                                            Names = new()
                                            { "foundation", "component", "transport", "priority", "ip", "port", "type", "raddr", "rport", "tcptype", "generation", "network-id", "network-cost" },
                                            Types = new()
                                            { 's', 'd', 's', 'd', 's', 'd', 's', 's', 'd', 's', 'd', 'd', 'd', 'd' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            var str = "candidate:%s %d %s %d %s %d typ %s";

                                            str += HasValue(o, "raddr") ? " raddr %s rport %d" : "%v%v";

                                            // NOTE: candidate has three optional chunks, so %void middles one if it's
                                            // missing.
                                            str += HasValue(o, "tcptype") ? " tcptype %s" : "%v";

                                            if (HasValue(o, "generation"))
                                                str += " generation %d";

                                            str += HasValue(o, "network-id") ? " network-id %d" : "%v";
                                            str += HasValue(o, "network-cost") ? " network-cost %d" : "%v";

                                            return str;
                                        }
                                        },

                                        // a=end-of-candidates
                                        new()
                                        {
                                            Name =
                                        "endOfCandidates",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(end-of-candidates)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"

                                        },

                                        // a=remote-candidates:1 203.0.113.1 54400 2 203.0.113.1 54401
                                        new()
                                        {
                                            Name =
                                        "remoteCandidates",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^remote-candidates:(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "remote-candidates:%s"

                                        },

                                        // a=ice-options:google-ice
                                        new()
                                        {
                                            Name =
                                        "iceOptions",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^ice-options:(\\S*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "ice-options:%s"

                                        },

                                        // a=ssrc:2566107569 cname:t9YU8M1UxTF8Y1A1
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "ssrcs",
                                            Reg =
                                        new Regex("^ssrc:(\\d*) ([\\w_-]*)(?::(.*))?"),
                                            Names = new()
                                            { "id", "attribute", "value" },
                                            Types = new()
                                            { 'd', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            var str = "ssrc:%d";

                                            if (HasValue(o, "attribute"))
                                            {
                                                str += " %s";

                                                if (HasValue(o, "value"))
                                                    str += ":%s";
                                            }

                                            return str;
                                        }
                                        },

                                        // a=ssrc-group:FEC 1 2
                                        // a=ssrc-group:FEC-FR 3004364195 1080772241
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "ssrcGroups",
                                            Reg =
                                        new Regex("^ssrc-group:([\x21\x23\x24\x25\x26\x27\x2A\x2B\x2D\x2E\\w]*) (.*)"),
                                            Names = new()
                                            { "semantics", "ssrcs" },
                                            Types = new()
                                            { 's', 's' },
                                            Format =
                                        "ssrc-group:%s %s"

                                        },

                                        // a=msid-semantic: WMS Jvlam5X3SX1OP6pn20zWogvaKJz5Hjf9OnlV
                                        new()
                                        {
                                            Name =
                                        "msidSemantic",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^msid-semantic:\\s?(\\w*) (\\S*)"),
                                            Names = new()
                                            { "semantic", "token" },
                                            Types = new()
                                            { 's', 's' },
                                            Format =
                                        "msid-semantic: %s %s" // Space after ':' is not accidental.

                                        },

                                        // a=group:BUNDLE audio video
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "groups",
                                            Reg =
                                        new Regex("^group:(\\w*) (.*)"),
                                            Names = new()
                                            { "type", "mids" },
                                            Types = new()
                                            { 's', 's' },
                                            Format =
                                        "group:%s %s"

                                        },

                                        // a=rtcp-mux
                                        new()
                                        {
                                            Name =
                                        "rtcpMux",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(rtcp-mux)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"

                                        },

                                        // a=rtcp-rsize
                                        new()
                                        {
                                            Name =
                                        "rtcpRsize",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^(rtcp-rsize)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"

                                        },

                                        // a=sctpmap:5000 webrtc-datachannel 1024
                                        new()
                                        {
                                            Name =
                                        "sctpmap",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^sctpmap:(\\d+) (\\S*)(?: (\\d*))?"),
                                            Names = new()
                                            { "sctpmapNumber", "app", "maxMessageSize" },
                                            Types = new()
                                            { 'd', 's', 'd' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "maxMessageSize")
                                                  ? "sctpmap:%s %s %s"
                                                : "sctpmap:%s %s";
                                        }
                                        },

                                        // a=x-google-flag:conference
                                        new()
                                        {
                                            Name =
                                        "xGoogleFlag",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("x-google-flag:([^\\s]*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "x-google-flag:%s"

                                        },

                                        // a=rid:1 send max-width=1280;max-height=720;max-fps=30;depend=0
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "rids",
                                            Reg =
                                        new Regex("^rid:([\\d\\w]+) (\\w+)(?: (.*))?"),
                                            Names = new()
                                            { "id", "direction", "params" },
                                            Types = new()
                                            { 's', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return HasValue(o, "params")
                                                    ? "rid:%s %s %s"
                                                    : "rid:%s %s";
                                        }
                                        },

                                        // a=imageattr:97 send [x=800,y=640,sar=1.1,q=0.6] [x=480,y=320] recv [x=330,y=250]
                                        // a=imageattr:* send [x=800,y=640] recv *
                                        // a=imageattr:100 recv [x=320,y=240]
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "imageattrs",
                                            Reg =
                                        new Regex(
                                            // a=imageattr:97
                                            "^imageattr:(\\d+|\\*)" +
                                            // send [x=800,y=640,sar=1.1,q=0.6] [x=480,y=320]
                                            // send *
                                            "[\\s\\t]+(send|recv)[\\s\\t]+(\\*|\\[\\S+\\](?:[\\s\\t]+\\[\\S+\\])*)" +
                                            // recv [x=330,y=250]
                                            // recv *
                                            "(?:[\\s\\t]+(recv|send)[\\s\\t]+(\\*|\\[\\S+\\](?:[\\s\\t]+\\[\\S+\\])*))?"
                                        ),
                                            Names = new()
                                            { "pt", "dir1", "attrs1", "dir2", "attrs2" },
                                            Types = new()
                                            { 's', 's', 's', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return new string("imageattr:%s %s %s") +
                                                    (HasValue(o, "dir2") ? " %s %s" : "");
                                        }
                                        },

                                        // a=simulcast:send 1,2,3;~4,~5 recv 6;~7,~8
                                        // a=simulcast:recv 1;4,5 send 6;7
                                        new()
                                        {
                                            Name =
                                        "simulcast",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex(
                                            // a=simulcast:
                                            "^simulcast:" +
                                            // send 1,2,3;~4,~5
                                            "(send|recv) ([a-zA-Z0-9\\-_~;,]+)" +
                                            // space + recv 6;~7,~8
                                            "(?:\\s?(send|recv) ([a-zA-Z0-9\\-_~;,]+))?" +
                                            // end
                                            "$"
                                        ),
                                            Names = new()
                                            { "dir1", "list1", "dir2", "list2" },
                                            Types = new()
                                            { 's', 's', 's', 's' },
                                            Format =
                                        "",
                                            FormatFunc =
                                        (o) =>
                                        {
                                            return new string("simulcast:%s %s") +
                                                    (HasValue(o, "dir2") ? " %s %s" : "");
                                        }
                                        },

                                        // Old simulcast draft 03 (implemented by Firefox).
                                        //   https://tools.ietf.org/html/draft-ietf-mmusic-sdp-simulcast-03
                                        // a=simulcast: recv pt=97;98 send pt=97
                                        // a=simulcast: send rid=5;6;7 paused=6,7
                                        new()
                                        {
                                            Name =
                                        "simulcast_03",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^simulcast: (.+)$"),
                                            Names = new()
                                            { "value" },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "simulcast: %s"

                                        },

                                        // a=framerate:25
                                        // a=framerate:29.97
                                        new()
                                        {
                                            Name =
                                        "framerate",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^framerate:(\\d+(?:$|\\.\\d+))"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 'f' },
                                            Format =
                                        "framerate:%s"

                                        },

                                        // a=source-filter: incl IN IP4 239.5.2.31 10.1.15.5
                                        new()
                                        {
                                            Name =
                                        "sourceFilter",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^source-filter:[\\s\\t]+(excl|incl) (\\S*) (IP4|IP6|\\*) (\\S*) (.*)"),
                                            Names = new()
                                            { "filterMode", "netType", "addressTypes", "destAddress", "srcList" },
                                            Types = new()
                                            { 's', 's', 's', 's', 's' },
                                            Format =
                                        "source-filter: %s %s %s %s %s"

                                        },

                                        // a=ts-refclk:ptp=IEEE1588-2008:00-50-C2-FF-FE-90-04-37:0
                                        new()
                                        {
                                            Name =
                                        "tsRefclk",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^ts-refclk:(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "ts-refclk:%s"

                                        },

                                        // a=mediaclk:direct=0
                                        new()
                                        {
                                            Name =
                                        "mediaclk",
                                            Push =
                                        "",
                                            Reg =
                                        new Regex("^mediaclk:(.*)"),
                                            Names = new()
                                            { },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "mediaclk:%s"

                                        },

                                        // Any a= that we don't understand is kepts verbatim on media.invalid.
                                        new()
                                        {
                                            Name =
                                        "",
                                            Push =
                                        "invalid",
                                            Reg =
                                        new Regex("(.*)"),
                                            Names = new()
                                            { "value" },
                                            Types = new()
                                            { 's' },
                                            Format =
                                        "%s"

                                        },
                                    }
                                }
                ***/
            };
        }

        //bool HasValue(string o, string key)
        //{
        //    var jsonDocument = JsonDocument.Parse(o);
        //    if (!jsonDocument.RootElement.TryGetProperty(key, out var it))
        //        return false;

        //    if (it.ValueKind == JsonValueKind.String)
        //        return !string.IsNullOrEmpty(it.GetString());
        //    else if (it.ValueKind == JsonValueKind.Number)
        //        return true;
        //    else
        //        return false;
        //}

    }
}
