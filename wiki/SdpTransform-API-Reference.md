# Utilme.SdpTransform — API Reference

Assembly `Utilme.SdpTransform`, version 26.9.7, target `net10.0`, no dependencies.
Everything lives in the **`Utilme.SdpTransform`** namespace.

The whole surface is pinned by an approval test against a checked-in baseline — see
[Design Notes](SdpTransform-Design-Notes#backward-compatibility).

---

## Entry points

`ModelExtensions` holds every converter as an extension method.

| Member | Notes |
|---|---|
| `Sdp ToSdp(this string)` | Parses SDP text. Returns `null` if it cannot be parsed. Discards diagnostics. |
| `SdpParseResult ToSdpResult(this string)` | Parses and reports every problem. Never throws. |
| `string ToText(this Sdp)` | Writes a document. Output is byte-identical to what it parsed from. |

---

## `SdpParseResult`

| Member | Notes |
|---|---|
| `Sdp Sdp` | The parsed document, or `null` if parsing could not complete. |
| `bool IsValid` | Whether an `Sdp` was produced. |
| `IReadOnlyList<SdpDiagnostic> Diagnostics` | Every problem, in document order. |
| `bool HasDiagnostics` | Whether anything was reported. |

A valid result may still carry warnings for lines that were skipped.

## `SdpDiagnostic`

| Member | Notes |
|---|---|
| `SdpDiagnosticSeverity Severity` | `Warning` (line skipped, parsing continued) or `Error` (parsing stopped). |
| `int LineNumber` | 1-based, counting blank lines. `0` when not attributable to one line. |
| `string Line` | The offending line, trimmed. |
| `string Message` | What went wrong. |
| `string ToString()` | `Warning (line 6): Unsupported session field: x=bad` |

---

## The document

### `Sdp`

| Property | SDP | Notes |
|---|---|---|
| `int ProtocolVersion` | `v=` | |
| `Origin Origin` | `o=` | Mandatory. |
| `string SessionName` | `s=` | Mandatory. `Sdp.DefaultSessionName` is `"-"`. |
| `string SessionInformation` | `i=` | Optional. |
| `Uri Uri` | `u=` | Optional. |
| `IList<string> EmailAddresses` | `e=` | Optional, repeatable. |
| `IList<string> PhoneNumbers` | `p=` | Optional, repeatable. |
| `ConnectionData ConnectionData` | `c=` | Here or per media description. |
| `IList<Bandwidth> Bandwidths` | `b=` | Optional, repeatable. |
| `IList<Timing> Timings` | `t=` | Mandatory. Seconds since 1900. |
| `IList<RepeatTime> RepeatTimes` | `r=` | Optional, repeatable. |
| `IList<TimeZone> TimeZones` | `z=` | Optional. Parsed in pairs. |
| `EncryptionKey EncryptionKey` | `k=` | Optional. |
| `Attributes Attributes` | `a=` | Optional. |
| `IList<MediaDescription> MediaDescriptions` | `m=` | Optional. |

`Sdp.CRLF` is `"\r\n"` — the only line separator the parser accepts. Each field also exposes its
indicator as a `const`, e.g. `Sdp.OriginIndicator` is `"o="`.

### `MediaDescription`

`MediaType Media`, `int Port`, `string Proto`, `IList<string> Fmts`, plus the session overrides
`Information`, `ConnectionData`, `Bandwidths`, `EncryptionKey` and `Attributes`.

### `Attributes`

Shared by `Sdp` and `MediaDescription` — both scopes parse and write the same set.

**Binary** (`bool?`, present or absent): `ExtmapAllowMixed`, `IceLite`, `RtcpMux`, `RtcpRsize`,
`SendRecv`, `SendOnly`, `RecvOnly`, `EndOfCandidates`.

**Single-valued**: `Group`, `MsidSemantic`, `Mid`, `Msid`, `IceUfrag`, `IcePwd`, `IceOptions`,
`Fingerprint`, `Rtcp`, `Setup`, `SctpPort`, `MaxMessageSize`, `Simulcast`.

**Repeatable** (`IList<T>`): `Candidates`, `Ssrcs`, `SsrcGroups`, `Rids`, `Rtpmaps`, `Fmtps`,
`RtcpFbs`, `Extmaps`.

Each attribute type carries its own label, e.g. `Candidate.Label` is `"candidate:"`.

---

## Enums

Each member's SDP token is its `[Display]` name; matching on input ignores case, output is always
canonical.

| Enum | Tokens |
|---|---|
| `NetType` | `IN` |
| `AddrType` | `IP4`, `IP6` |
| `MediaType` | `audio`, `video`, `text`, `application`, `message` |
| `BandwidthType` | `AS`, `CT`, `RS`, `RR`, `TIAS` |
| `Direction` | `sendrecv`, `sendonly`, `recvonly`, `inactive` |
| `RidDirection` | `recv`, `send` |
| `SetupRole` | `active`, `passive`, `actpass`, `holdconn` |
| `CandidateType` | `host`, `srflx`, `prlfx`, `relay` |
| `CandidateTransport` | `udp`, `tcp` |
| `GroupSemantics` | `LS`, `FID`, `BUNDLE` |
| `HashFunction` | `sha-1`, `sha-224`, `sha-256`, `sha-384`, `sha-512`, `md2`, `md5` |
| `EncryptionKeyMethod` | `clear`, `base64`, `uri`, `prompt` |
| `SdpDiagnosticSeverity` | *(not an SDP token — `Warning`, `Error`)* |

---

## Per-line converters

Every field and attribute has a `ToXxx(this string)` / `ToText(this Xxx)` pair, so a single line can
be converted on its own.

**Session fields**: `ToProtocolVersion`, `ToOrigin`, `ToSessionName`, `ToInformation`, `ToUri`,
`ToEmailAddresses`, `ToPhoneNumbers`, `ToConnectionData`, `ToBandwidth`, `ToTiming`, `ToRepeatTime`,
`ToTimeZones`, `ToEncryptionKey`, `ToMediaDescription`.

**Attributes**: `ToGroup`, `ToMsidSemantic`, `ToMid`, `ToMsid`, `ToIceUfrag`, `ToIcePwd`,
`ToIceOptions`, `ToFingerprint`, `ToRtcp`, `ToSetup`, `ToSctpPort`, `ToMaxMessageSize`, `ToCandidate`,
`ToSsrc`, `ToSsrcGroup`, `ToRid`, `ToSimulcast`, `ToRtpmap`, `ToFmtp`, `ToRtcpFb`, `ToExtmap`.

Each has a matching `ToText`. A few write-side helpers are named differently where the input type is
`string` or `int` and would otherwise collide: `ToProtocolVersionText`, `ToSessionNameText`,
`ToInformationText`, `ToEmailAddressesText`, `ToPhoneNumbersText`.

**Helpers**: `ToFmtp(this Dictionary<string, object>, int payloadType)` and `ToDictionary(this Fmtp)`
convert format parameters to and from a dictionary. `HexadecimalStringToByteArray` and
`ToSeconds(this string)` — which accepts `d`, `h`, `m` and `s` suffixes — are public utilities.
