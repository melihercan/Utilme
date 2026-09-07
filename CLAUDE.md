# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

Two independent NuGet libraries plus a console demo, tied together by `Utilme.sln`:

- `Result/` → package `Utilme.Result`, namespaces `Utilme` and `Utilme.Functional`
- `SdpTransform/` → package `Utilme.SdpTransform`, public namespace `Utilme.SdpTransform` (internal helpers live in `UtilmeSdpTransform`)
- `DemoApp/` → console app referencing `SdpTransform`
- `Result.Tests/` → xUnit v3 characterization tests for `Utilme.Result`
- `SdpTransform.Tests/` → xUnit v3 characterization tests for `Utilme.SdpTransform`

The libraries do not reference each other.

## Commands

```powershell
dotnet build Utilme.sln
dotnet test
dotnet test --project Result.Tests/Result.Tests.csproj
dotnet run --project DemoApp
dotnet pack SdpTransform/SdpTransform.csproj -c Release
```

**`dotnet test` runs in Microsoft.Testing.Platform (MTP) mode**, opted in via the `test.runner`
section of `global.json`. xUnit v3 test projects are executables and the .NET 10 SDK refuses to
run them through the legacy VSTest path, so that file is required — deleting it breaks the suite
with "Testing with VSTest target is no longer supported".

Two consequences worth knowing before you fight the CLI:

- **Do not pass `--nologo`** (or other VSTest-era flags). MTP mode forwards unrecognized
  arguments to the test executable, which rejects them with exit code 5 / "Zero tests ran" —
  a failure that looks like broken tests but is a bad command line.
- Target a single project with **`--project <path>`**, not a bare path argument.

`dotnet run --project Result.Tests` also works and runs the tests directly.

Both libraries now have characterization suites (`Result.Tests`, `SdpTransform.Tests`). `DemoApp`
remains a useful manual check: it round-trips a WebRTC SDP through text → object → JSON → object → text and prints each stage, so changes to `SdpTransform` are validated by running it and diffing the two printed SDP blocks (currently 55 identical lines each).

Running it also prints one `==== SDP unsupported media description field:` warning with an empty token. That is a known pre-existing parser bug, not a regression: `ToSdp` splits on `Sdp.CRLF` with `RemoveEmptyEntries` *before* trimming, so the whitespace-only final line of the demo's raw string survives as an empty token.

`GeneratePackageOnBuild` is `True` on both libraries, so every build drops a `.nupkg` into `bin/<Config>/`. Package version and release notes are properties in the `.csproj` (`Version`, `AssemblyVersion`, `FileVersion`, `PackageReleaseNotes`) — bump them there when publishing.

**Packaging is done by GitHub Actions**, in `.github/workflows/`:

- `ci.yml` — build and test on every push and PR to master. Builds with `-warnaserror`, so the
  repository's zero-warning bar is enforced there, and a new NuGet advisory fails the build.
- `publish-result.yml` / `publish-sdptransform.yml` — one per package. Triggered by a tag
  (`result-v<version>` / `sdptransform-v<version>`), or manually, where the default is a dry run that
  packs and uploads the `.nupkg` as a build artifact without publishing. Both verify the tag matches
  the csproj `<Version>` before packing, run the tests, and push with `--skip-duplicate`. They need
  the `NUGET_API_KEY` repository secret.

`GeneratePackageOnBuild` has been **removed** from both libraries now that CI owns packaging — local
builds no longer emit a `.nupkg`. Use `dotnet pack` explicitly when you want one.

Both packages set `PackageReadmeFile`, `PackageLicenseExpression` and a date-based `<Version>`
(`26.09.07`, which NuGet normalises to `26.9.7`). `SdpTransform` deliberately does **not** set
`GenerateDocumentationFile`: its public members are not XML-documented, so turning it on would emit
hundreds of CS1591 warnings and break the zero-warning bar.

## Targeting

All three projects target **`net10.0` only** (single-target). The repo was migrated from `netstandard2.0/2.1 + net5.0/net6.0`; that history explains some of the code shape but is no longer a constraint.

Consequences of the migration, so they are not accidentally reintroduced:

- `SdpTransform` has **zero `PackageReference`s**. `System.Text.Json`, `System.ComponentModel.Annotations`, and `System.IO.Pipelines` are all in-box on net10.0, and `Microsoft.Extensions.DependencyInjection.Abstractions` was never used in source.
- The `IsExternalInit.cs` shims are gone; `init` accessors are native now.
- No explicit `LangVersion` — it follows the SDK default (C# 14).
- `Nullable` is enabled in **`Result` and `Result.Tests` only**. `SdpTransform` and `DemoApp` do not have it yet; turning it on there will surface many warnings across the models, so do it deliberately rather than as a side effect. `ImplicitUsings` is off everywhere except `Result.Tests`.

The build is clean: **0 warnings, 0 errors**. Keep it that way.

## Result.Tests — characterization suite

These are **characterization tests**, not specifications. They pin the library's current
observable behaviour — warts included — so the planned refactor cannot silently change what
consuming apps see. Several deliberately assert things that are arguably wrong.

A red test here means behaviour changed. That is only acceptable if the change was intended, in
which case update the test in the same commit.

The Phase 0 characterization files — do not relax these:

- `ResultCharacterizationTests` — runtime behaviour. Pins that `Value` returns `default` (never
  throws) on failure, that `Ok(null)` is a success, that `Error(string)` always collapses to
  `ResultStatus.Error`, and that enum failures set `ErrorMessage` to the status name.
- `ResultStatusCharacterizationTests` — the enum's numeric values, names, count and underlying
  type. Values are frozen because apps may have persisted them as ints, so **new statuses may
  only be appended at the end**.
- `PublicApiShapeTests` — reflection over the public surface, including a full member-name
  inventory that fails when anything is added or removed.

Two tests carry explicit warnings in their comments:

- `NetworkUp_is_a_FAILURE_despite_its_name` — `NetworkUp()` reports `IsOk == false`. Do not
  "fix" this; flipping the polarity would silently invert branches in every consuming app.
- `Message_less_failures_are_cached_and_shared` — flipped deliberately in Phase 1 (it previously
  asserted `NotBeSameAs`). Any future change to instance identity must update it consciously.

Phase 2 added `ResultFailTests`, `ResultAccessorTests` and `ResultExtensionsTests`; Phase 3 added
`SoftDeprecationTests`. These are ordinary specification tests for the new API and the deprecation
policy, rather than characterization tests.

`PublicApiShapeTests` encodes the constraint that drives the whole refactor: `NotFound`/`Timeout`/
`Cancelled` are **properties** while `NotSupported`/`InvalidData`/`NetworkUp`/`NetworkDown` are
**methods**. C# forbids one name being both, so this cannot be unified in place — a consistent
API has to be added *beside* the existing members, never in place of them.

## SdpTransform.Tests — characterization suite

Same policy as `Result.Tests`: these pin **current** behaviour, defects included, as the safety net
for the refactor. They are not a specification of what the library ought to do.

- `SdpSamples` — the 55-line WebRTC offer, **generated from `DemoApp/Program.cs`** by a script so the
  fixture cannot drift from the sample the library was always exercised against. It joins lines with
  `Sdp.CRLF` explicitly: `ToSdp` splits on CRLF only, so a fixture written as a verbatim string in an
  LF-saved file would hand the parser one giant token and every test would fail for the wrong reason.
  **Write new fixtures the same way.**
- `SdpRoundTripTests` — text → object → text line by line, writer idempotence, JSON round trip. The
  broadest net; run these first when changing anything.
- `FieldConversionTests` / `AttributeConversionTests` — each `ToXxx` / `ToText` pair in isolation.
- `KnownDefectTests` — asserts behaviour that is **wrong**, one test per outstanding defect. When a
  defect is fixed, rewrite its test rather than deleting it, so the fix is visible in the diff.
- `WriterRegressionTests` — the five writer defects fixed in Phase 2, rewritten from their original
  defect pins.
- `ParserRegressionTests` — the five parser defects fixed in Phase 3, likewise rewritten.
- `DiagnosticsTests` — Phase 4's `ToSdpResult`, plus the tests proving `ToSdp` is unchanged.
- `AttributeSymmetryTests` — Phase 5's shared dispatcher and writer.
- `EnumNameTests` — that each SDP enum member's two names exist, agree and are unambiguous.
- `PublicApiSurfaceTests` — the whole public surface against `PublicApi.approved.txt`.

### Backward compatibility

`Utilme.SdpTransform` is under the **same additive-only guarantee as `Utilme.Result`**: real apps
depend on it, so no public member may be renamed, removed, or have its type changed. New capability
is added *beside* the old surface, never in place of it.

`PublicApiSurfaceTests` enforces this mechanically — it captures every public type, member signature,
enum numeric value and `const` literal. The literals matter as much as the signatures: they are the
SDP grammar tokens (`"candidate:"`, `"o="`, …) the parser and writer are both built from. When a
change is intentional, review the diff and copy `PublicApi.received.txt` from the test output
directory over `PublicApi.approved.txt`. Never weaken the assertion.

This rules out the obvious shape for the diagnostics work: **`ToSdp` cannot change its return type**
to `Result<Sdp>` or anything else. It keeps returning `Sdp` and keeps returning `null` on failure; a
diagnostics-carrying alternative has to be a new method alongside it.

Two decisions settled for that work:

- **`Utilme.SdpTransform` stays dependency-free.** It must not take `Utilme.Result` as a dependency;
  any parse-result type is defined inside this package.
- **Console output goes.** `Console.WriteLine` diagnostics are to be removed rather than preserved.
  The two tests pinning them are expected to be rewritten when that happens.

**Phase 2 fixed every writer defect**, so `ToText` output is now byte-identical to its input and
`SdpRoundTripTests` asserts exact equality. Do not weaken that assertion.

**Phase 3 fixed the parser-robustness defects**: enum matching now ignores case (while `DisplayName`
still writes canonical casing — parse leniently, write canonically), whitespace-only lines are
ignored silently, field indicators are stripped from the **start only** via `StripPrefix` rather than
`string.Replace`, `k=prompt` is accepted, and `ToSeconds` handles all of `d`/`h`/`m`/`s`. DemoApp now
runs with no parser warnings at all.

**Phase 4 replaced the diagnostics.** `Console.WriteLine` is gone from the library entirely, and the
swallowed exception is reported. `ToSdpResult` returns `SdpParseResult` (`Sdp`, `IsValid`,
`Diagnostics`), where each `SdpDiagnostic` carries a severity, a 1-based line number into the
original text, the offending line and a message. `ToSdp` keeps its signature and its null-on-failure
behaviour and is now a one-line wrapper over it. Parsing is still all-or-nothing: a recognised
indicator whose value does not parse aborts the document, so nothing that parsed before parses
differently. `SdpParseResult` is local to this package — **do not** replace it with `Utilme.Result`.

**Phase 5 made the two attribute scopes symmetric.** `TryParseAttribute` and `WriteAttributes` are
shared by the session and media-description branches — change one and both scopes follow, which is
the point. Parsing more at session level required changing the writer at the same time, or those
lines would be dropped on the way out. `KnownDefectTests` is gone: every defect it pinned is fixed.

`ModelExtensions.cs` was split into five partial-class files (`.Parsing`, `.Writing`, `.Fields`,
`.Attributes`, `.Utility`). `partial` is compile-time only, so the API is unchanged.

Still duplicated: every enum member carries both `[JsonStringEnumMemberName]` and `[Display]`.
`EnumNameTests` removes the drift *risk*; collapsing the duplication would mean deleting `[Display]`
from public enum members, which is public metadata and therefore a compatibility decision.

Note when writing tests that `z=` is the time-zone indicator, not a free letter — use `q`, `x` or `y`
for "unrecognised line" fixtures.

Two tests redirect `Console.Out`. That is safe only because xUnit runs tests within a single class
sequentially and no other class touches Console — keep console-capturing tests in that one class.

## SdpTransform architecture

The library converts between three representations: SDP text ⇄ object graph ⇄ JSON (`System.Text.Json`).

**Model classes carry their own grammar.** Each model exposes `const string` tokens next to the properties they parse — `Sdp.OriginIndicator = "o="`, `Candidate.Label = "candidate:"`, `Candidate.Typ = "typ"`. The parser and the writer both consume these constants; never hardcode an SDP token in `ModelExtensions.cs`.

**All conversion logic lives in `Extensions/ModelExtensions.cs`** (~1300 lines) as paired extension methods: `ToXxx(this string)` parses one line, `ToText(this Xxx)` writes it back including the trailing `Sdp.CRLF`. `ToSdp(this string)` / `ToText(this Sdp)` are the two entry points.

`ToSdp` splits on `Sdp.CRLF` only, walks session-level lines until it hits `m=`, then walks media-description lines. Both loops are long `if/else if` chains dispatching on `StartsWith(<const>)`. Note the asymmetry: the **session** branch recognizes only `extmap-allow-mixed`, `ice-lite`, `group`, `msid-semantic`, `fingerprint`, while the **media-description** branch handles the full attribute set. Unknown lines print a `==== SDP unsupported ...` warning rather than throwing (the `throw` variants are commented out beside them), and `ToSdp` wraps everything in a try/catch that returns `null` on failure.

`ToText` emits fields in a fixed hand-written order; the sequence of `if` blocks in that method *is* the output line order.

**Adding an SDP attribute** means touching, in order: a model class under `Models/Attributes/` with its `Label` const → a property on `Models/Attributes.cs` (`Attributes` is shared by both `Sdp` and `MediaDescription`) → a `To<Attr>` / `ToText` pair in `ModelExtensions.cs` → the dispatch chain in `ToSdp` (session and/or media branch) → the emit sequence in `ToText`.

**Enums are double-annotated** (`Enums/`): `[JsonConverter(typeof(JsonStringEnumConverter<TEnum>))]` on the type plus `[JsonStringEnumMemberName("...")]` per member for the JSON wire form, and `[Display(Name = "...")]` per member for the SDP text form. SDP text conversion goes through `DisplayName()` / `EnumFromDisplayName<T>()` in `Extensions/UtilityExtensions.cs` — use those, not `ToString()`.

The two names are maintained independently and are expected to be identical; they drifted once already (`BandwidthType.ApplicationSpecific` had an empty JSON name against a `Display` of `AS`). Collapsing them onto a single attribute is an open cleanup.

JSON round-tripping expects `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`; optional SDP elements are modelled as nullable properties/lists that stay `null` when absent.

## Result library

Four files, and the split matters:

- `Result.cs` — `Result<T>` itself.
- `ResultFactory.cs` — the non-generic `static class Result`. Legal beside `Result<T>` because the
  arity differs, exactly like `Task`/`Task<T>`.
- `Unit.cs` — payload for results that succeed without producing a value.
- `Functional/ResultExtensions.cs` — composition operators, in namespace `Utilme.Functional`.

### The two surfaces

`Result<T>`'s per-status members are **deliberately inconsistent**: `NotFound`, `Timeout`,
`Cancelled` are properties; `NotSupported()`, `InvalidData()`, `NetworkUp()`, `NetworkDown()` are
methods. C# forbids one name being both, so this cannot be unified in place without a breaking
removal — see `PublicApiShapeTests`, which pins each member's shape in both directions. Leave them
exactly as they are.

The consistent surface is the **non-generic `Result` factory**, where every status is a method and
the value type is inferred (`Result.Ok(42)`, `Result.NotFound<Foo>()`). Prefer it in new code.

Those seven legacy members carry `[EditorBrowsable(EditorBrowsableState.Never)]`: they are hidden
from IntelliSense so new code does not reach for them, while remaining fully functional. They are
deliberately **not** `[Obsolete]` — that would spam warnings through apps already using them.
`SoftDeprecationTests` pins both halves of that policy, including that nothing is marked obsolete.

Note the attribute is honoured by the IDE only for members arriving from a referenced *assembly*.
Projects inside this solution — `Result.Tests` included — still see the members in completion, which
is why the tests can keep exercising them.

`Result<T>.Fail(status, message)` is the canonical failure factory and the only one that pairs a
specific status with a caller-supplied message. `Error(string)` still collapses to
`ResultStatus.Error` and still stores a null message verbatim — both pinned by tests.

### Invariants to preserve

- The project is `Nullable`-enabled and emits an XML documentation file (shipped in the package),
  so **every public member must carry XML docs** or the build fails its own zero-warning bar via
  CS1591.
- `Value` is `T?` and `ErrorMessage` is `string?`. Metadata-only annotations — IL signatures are
  unchanged and the change is binary compatible, pinned by
  `Nullable_annotations_did_not_change_the_runtime_signatures`. Consumer apps with nullable enabled
  will get CS8600/CS8603 on unguarded `.Value` reads, which is intended: it surfaces the fact that
  `Value` returns `default` on failure rather than throwing.
- Failures carrying no caller-supplied message return **cached instances**, one per closed generic
  type; `Ok`, `Error` and `Fail(status, message)` allocate because they carry data. Anything added
  to the type must stay immutable or the caching becomes unsafe.
- `Cached(status)` **falls back to allocating** for a status it does not know, so a status appended
  to `ResultStatus` keeps working without being registered there.
- `FromFailure(status, message)` is `internal` and exists so combinators can carry a failure across
  a change of value type with status and message preserved *exactly*, null messages included.
- New statuses must be **appended** to `ResultStatus`; the numeric values are pinned.

### Utilme.Functional

`Match`/`Switch`, `Map`, `Then`, `Tap`/`TapError`, `Else`, plus async variants over both
`Result<T>` and `Task<Result<T>>`. Every operator short-circuits: on failure the delegate is not
invoked and the failure propagates untouched.

These sit in their own namespace **on purpose**. They are extension methods on `Result<T>`, so had
they been in `Utilme` they could have become ambiguous with same-named extensions a consuming app
already defines — an ambiguity that is a compile error, not a warning. `using Utilme.Functional;`
makes importing them a deliberate act.

`Match` (takes `Func`) and `Switch` (takes `Action`) are named differently rather than overloaded,
because a lambda whose body is a method call would otherwise be ambiguous between them.
