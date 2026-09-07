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

## Documentation

| Page | What's in it |
|---|---|
| [Getting Started](SdpTransform-Getting-Started) | Parsing, writing, JSON, diagnostics, gotchas |
| [API Reference](SdpTransform-API-Reference) | Every public type and member |
| [Design Notes](SdpTransform-Design-Notes) | Why it looks like this; invariants to preserve |
| [Refactor Log](SdpTransform-Refactor-Log) | What changed on the way to 26.9.7, and what is left |

## Backward compatibility

Additive-only, the same guarantee as [Utilme.Result](Result-Design-Notes#the-governing-constraint),
enforced by an approval test over the whole public surface. See
[Design Notes](SdpTransform-Design-Notes#backward-compatibility).

## Test coverage

149 tests — see [Building and Testing](Building-and-Testing#sdptransformtests). They began as a
characterization suite pinning existing behaviour, defects included, which is what made the refactor
safe.
