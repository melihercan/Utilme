# Utilme.SdpTransform — Design Notes

Why the library looks the way it does, and the invariants any change must preserve.

## Backward compatibility

Real applications depend on this package, so the refactor has been **additive only**: no public
member renamed, removed, or retyped.

`PublicApiSurfaceTests` enforces this against a checked-in baseline covering every type, member
signature, enum numeric value and `const` literal. The literals matter as much as the signatures —
they are the SDP grammar tokens (`"candidate:"`, `"o="`, `CRLF`) that both the parser and the writer
are built from, so a typo in one would break parsing without changing a signature.

The clearest consequence: **`ToSdp` keeps returning `Sdp` and keeps returning `null` on failure.**
Better diagnostics arrived as `ToSdpResult` beside it, not as a changed return type. For the same
reason the package stays **dependency-free** — `SdpParseResult` is defined here rather than reusing
`Utilme.Result`.

## Model classes carry their own grammar

Each model exposes `const string` tokens next to the properties they parse:

```csharp
public const string OriginIndicator = "o=";     // on Sdp
public const string Label = "candidate:";       // on Candidate
```

Parser and writer both consume these constants. **Never hardcode an SDP token** in the conversion
code.

## One dispatcher, two scopes

`TryParseAttribute` and `WriteAttributes` are shared by the session and media-description branches.
They used to be separate chains, and the session one recognised only five attributes, so anything
else valid at session level was silently dropped. Sharing them keeps the two scopes in step by
construction — and note that parsing more at session level *requires* writing more, or those lines
vanish on the way out.

## Parse leniently, write canonically

`EnumFromDisplayName` matches ignoring case, so `UDP` and `udp` both resolve. `DisplayName` always
emits the canonical spelling, so lenient input cannot leak non-canonical output.

## Anchored prefixes

Indicators and labels are stripped with `StripPrefix`, which removes them **only from the start**.
The converters previously used `string.Replace`, which is global and deleted the token from the
middle of values too — corrupting a session name, or a URI whose query contained `u=`.

## All-or-nothing parsing

A recognised indicator whose value does not parse aborts the whole document. This is long-standing
behaviour and is preserved deliberately: per-line recovery would make `ToSdp` return non-null where
it previously returned `null`, which is a silent behaviour change for existing callers.

## Enum names are declared twice

Every member carries both `[JsonStringEnumMemberName]` (JSON wire form) and `[Display]` (SDP text
form), maintained independently. They drifted once already —
`BandwidthType.ApplicationSpecific` shipped with an empty JSON name against a `Display` of `AS`.

`EnumNameTests` now asserts both exist on every member of every SDP enum, that they agree, that none
is blank, that none are ambiguous under case-insensitive matching, and that each round-trips through
the library's own lookup. The drift risk is gone; the duplication remains, because collapsing it
means deleting `[Display]` from public enum members and that is public metadata.

## Layout

`ModelExtensions` is one static class split across five partial-class files:

| File | Holds |
|---|---|
| `.Parsing` | `ToSdp`, `ToSdpResult`, `TryParseAttribute` |
| `.Writing` | `ToText(Sdp)`, `WriteAttributes` |
| `.Fields` | Session-level field converters |
| `.Attributes` | Attribute converters |
| `.Utility` | `HexadecimalStringToByteArray`, `ToSeconds` |

`partial` is compile-time only, so the split changed no API.
