# Utilme.SdpTransform

SDP parser and writer for .NET. Transforms between SDP text, an object graph, and JSON — useful for
VoIP and WebRTC work.

```
dotnet add package Utilme.SdpTransform
```

Requires **.NET 10**, no dependencies.

## Parsing

```csharp
using Utilme.SdpTransform;

Sdp? sdp = sdpText.ToSdp();                  // null if it cannot be parsed
SdpParseResult result = sdpText.ToSdpResult();  // same, plus every problem found
```

`ToSdpResult` never throws. `IsValid` says whether an `Sdp` was produced; `Diagnostics` carries a
`Warning` for each line that was skipped and an `Error` if parsing had to stop, each with the
1-based line number from the original text.

> [!NOTE]
> **Full documentation is pending a refactor.** This page records what is known today, including
> defects found while migrating the library to .NET 10. It will be expanded once the refactor lands,
> the way [Utilme.Result](Result-Getting-Started) already has been.
>
> For usage in the meantime, see [`SdpTransform/README.md`](https://github.com/melihercan/Utilme/blob/master/SdpTransform/README.md)
> and `DemoApp/Program.cs`, which demonstrates the full round trip.

## How it works

Three representations, converted by paired extension methods:

```
SDP text  ⇄  object graph  ⇄  JSON
```

```csharp
using Utilme.SdpTransform;

var sdp     = sdpText.ToSdp();            // text   -> object
var text    = sdp.ToText();               // object -> text
var json    = JsonSerializer.Serialize(sdp, options);
var back    = JsonSerializer.Deserialize<Sdp>(json)!.ToText();
```

Two design points that are not obvious from the file layout:

- **Model classes carry their own grammar.** Each model exposes `const string` tokens next to the
  properties they parse — `Sdp.OriginIndicator = "o="`, `Candidate.Label = "candidate:"`. Parser and
  writer both consume these constants; SDP tokens are never hardcoded in the conversion code.
- **All conversion lives in one file**, `Extensions/ModelExtensions.cs`, as paired
  `ToXxx(this string)` / `ToText(this Xxx)` methods. `ToSdp` and `ToText` are the two entry points.

Enums are double-annotated: `[JsonStringEnumMemberName]` for the JSON wire form, `[Display]` for the
SDP text form, converted through `DisplayName()` / `EnumFromDisplayName<T>()`.

## Known issues

Catalogued during the .NET 10 migration and confirmed by the Phase 0 characterization suite — every
item below is pinned by a test in `SdpTransform.Tests/KnownDefectTests.cs`, so fixing one shows up as
a rewritten test rather than a silent change.

### Fixed in Phase 2 (writer)

All five writer defects are fixed. Their tests were rewritten and moved to `WriterRegressionTests`,
and the round trip is now asserted **byte for byte** rather than line-by-line-trimmed.

| Was | Now |
|---|---|
| `b=AS:1024` written back as `b=ApplicationSpecific 1024` | writes the `Display` name and `:`, so it parses back |
| `a=extmap:1/sendonly` written as `a=extmap:1sendonly` | the `/` separator is preserved |
| `NullReferenceException` writing an attribute-free SDP | optional `Attributes` and `MediaDescriptions` are handled |
| 12 lines with stray trailing whitespace | none |
| `a=rid:hi send ;max-width=1280` | `pt=` and restrictions form one `;`-separated list |

### Fixed in Phase 3 (parser)

| Was | Now |
|---|---|
| enum matching case-sensitive, so RFC-spelled `UDP` threw | matching ignores case; output is still written in canonical casing |
| whitespace-only lines reported as unsupported fields | trimmed before blanks are discarded, so they are ignored silently |
| `string.Replace` stripped indicators anywhere in a value | stripped from the **start only** |
| `k=prompt` threw `IndexOutOfRangeException` | accepted; `Value` is null and it round-trips |
| `ToSeconds` threw for `m` and `s` suffixes | all four of `d`, `h`, `m`, `s` scale correctly |

The anchoring fix is the one change in this phase that alters the result for input that already
parsed: `"s=a s=b".ToSessionName()` returned `"a b"` and now returns `"a s=b"`. The old behaviour
silently corrupted any value containing an indicator-like substring — a session name, a URI query
with `u=` in it, an `fmtp` parameter.

### Fixed in Phase 4 (diagnostics)

`Console.WriteLine` is gone and the swallowed exception is gone. `ToSdpResult` reports both:

```csharp
var result = sdpText.ToSdpResult();

if (!result.IsValid)
    foreach (var d in result.Diagnostics)
        Console.Error.WriteLine(d);        // "Error: IndexOutOfRangeException: ..."
else
    foreach (var d in result.Diagnostics)  // warnings for lines that were skipped
        _log.Warn(d.ToString());           // "Warning (line 6): Unsupported session field: x=bad"
```

| Was | Now |
|---|---|
| unknown fields written to `Console.WriteLine` | reported as `Warning` diagnostics the caller can act on |
| exception caught, message assigned to an unused local, `null` returned | reported as an `Error` diagnostic naming the exception type and message |
| session-level warnings mislabelled "media description attribute" | scope named correctly |
| no way to know which line was at fault | 1-based line numbers into the original text, counting blanks |

`ToSdp` is unchanged — same signature, still `null` on failure — and is now a one-line wrapper over
`ToSdpResult`. Parsing remains all-or-nothing: a recognised indicator whose value does not parse
still aborts the whole document, so nothing that parsed before parses differently now.

### Fixed in Phase 5 (structural)

**Session and media attributes are handled symmetrically.** Both scopes now share one parser
dispatcher and one writer, so they cannot drift apart again. Attributes valid at session level —
`a=setup`, `a=sendonly`, `a=mid`, `a=rtpmap` and the rest — are parsed and written instead of being
dropped with a warning; `a=ice-lite`, previously session-only, is accepted at media level too.

The writer had to change with the parser: capturing more at session level without emitting it would
have silently dropped those lines on the way back out.

**`ModelExtensions.cs` was split** from 1289 lines into five partial-class files —
`.Parsing`, `.Writing`, `.Fields`, `.Attributes`, `.Utility`. No API change; `partial` is a
compile-time construct.

### Outstanding

**Enum names are still duplicated.** Every member carries both a `[JsonStringEnumMemberName]` and a
`[Display]` name, maintained independently and expected to be identical.

The drift *risk* is gone — `EnumNameTests` now asserts that both attributes exist on every member of
every SDP enum, that they agree, that none is blank, that no two are ambiguous under the
case-insensitive matching introduced in Phase 3, and that each round-trips through the library's own
lookup. The duplication itself remains, because collapsing it means deleting `[Display]` from public
enum members, and that is public metadata a consumer could in principle reflect on.

## Backward compatibility

This package is under the same additive-only guarantee as
[Utilme.Result](Result-Design-Notes#the-governing-constraint): no public member is renamed, removed,
or has its type changed. `PublicApiSurfaceTests` enforces it against a checked-in baseline covering
every signature, enum numeric value and `const` grammar token.

One consequence worth stating up front: **`ToSdp` will keep returning `Sdp` and keep returning
`null` on failure.** Better diagnostics will arrive as a new method beside it, not as a changed
return type — and this package stays **dependency-free**, so that method will not return
`Utilme.Result`; any parse-result type is defined here.

## Test coverage

`SdpTransform.Tests` holds 149 tests — see
[Building and Testing](Building-and-Testing#sdptransformtests). The suite pins current behaviour
including all of the defects above; it is the safety net for the refactor, not a specification of
what the library ought to do.
