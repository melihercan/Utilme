# Building and Testing

Requires the **.NET 10 SDK**.

```powershell
dotnet build Utilme.sln
dotnet test
dotnet run --project DemoApp
dotnet pack Result/Result.csproj -c Release
```

The build is expected to be **clean: 0 warnings, 0 errors**. Both libraries set
`GeneratePackageOnBuild`, so every build drops a `.nupkg` into `bin/<Config>/`.

## `dotnet test` runs on Microsoft.Testing.Platform

`Result.Tests` uses **xUnit v3**, whose test projects are executables. The .NET 10 SDK refuses to run
those through the legacy VSTest path, so the repository opts into MTP mode via `global.json`:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

Deleting that file breaks the suite with *"Testing with VSTest target is no longer supported"*.

Two consequences worth knowing before you fight the CLI:

- **Do not pass `--nologo`** or other VSTest-era flags. MTP forwards unrecognised arguments to the
  test executable, which rejects them with exit code 5 and *"Zero tests ran"* — a failure that looks
  like broken tests but is really a bad command line.
- Target one project with **`--project <path>`**, not a bare path argument:

```powershell
dotnet test --project Result.Tests/Result.Tests.csproj
```

`dotnet run --project Result.Tests` also works and runs the tests directly.

### FluentAssertions is pinned to 7.x

Version 8 moved to a licence that requires payment for commercial use; 7.x is the last Apache-2.0
release. The pin is deliberate — do not let a tool bump it.

## Characterization tests

`Result.Tests` contains two kinds of test, and the distinction matters.

**Characterization tests** pin the library's existing observable behaviour so that refactoring cannot
change it silently. Several deliberately assert things that are arguably wrong. A red one means
behaviour changed — acceptable only if that was intended, in which case update the test in the same
commit and say so.

| File | Pins |
|---|---|
| `ResultCharacterizationTests` | Runtime behaviour: `Value` returns `default` on failure, `Ok(null)` is a success, `Error(string)` always collapses to `ResultStatus.Error`, enum failures use the status name as their message. |
| `ResultStatusCharacterizationTests` | The enum's numeric values, names, count and underlying type. New statuses may only be **appended**. |
| `PublicApiShapeTests` | The public surface by reflection, including a full member inventory and the property-vs-method shape of each legacy member. |

Two carry explicit warnings in their comments:

- `NetworkUp_is_a_FAILURE_despite_its_name` — do not "fix" the polarity.
- `Message_less_failures_are_cached_and_shared` — flipped deliberately in Phase 1.

**Specification tests** cover the newer API normally: `ResultFailTests`, `ResultAccessorTests`,
`ResultExtensionsTests`, `SoftDeprecationTests`.

### When the inventory test fails

`No_public_members_have_been_added_or_removed_unnoticed` fails on **any** change to the public
surface. That is the point — it forces a deliberate review. Update the list in the same commit;
do not loosen the assertion.

## SdpTransform.Tests

149 tests over the SDP parser and writer, same policy as above — they pin current
behaviour, defects included.

| File | Covers |
|---|---|
| `SdpSamples` | The 55-line WebRTC offer, **generated from `DemoApp/Program.cs`** so the fixture cannot drift from the sample the library was always exercised against. |
| `SdpRoundTripTests` | End-to-end: text → object → text line by line, writer idempotence, and the JSON round trip. The broadest net in the suite. |
| `FieldConversionTests` | Each session-level `ToXxx` / `ToText` pair in isolation. |
| `AttributeConversionTests` | Each attribute converter, with the expected SDP written out in full. |
| ~~`KnownDefectTests`~~ | Gone — every defect it pinned has been fixed and its tests rewritten into the regression files above. These assert behaviour that is **wrong**, and are meant to be rewritten — not deleted — as each defect is fixed. |
| `WriterRegressionTests` | The five writer defects fixed in Phase 2, rewritten from their original defect pins so the change is visible in the diff. |
| `ParserRegressionTests` | The five parser defects fixed in Phase 3, likewise rewritten rather than deleted. |
| `DiagnosticsTests` | Phase 4's `ToSdpResult`, plus the compatibility tests proving `ToSdp` still behaves identically. |
| `AttributeSymmetryTests` | Phase 5's shared attribute dispatcher and writer. |
| `EnumNameTests` | That every SDP enum member's two names exist, agree, and are unambiguous. |
| `PublicApiSurfaceTests` | The whole public surface against `PublicApi.approved.txt` — every signature, enum value and `const` literal. Copy `PublicApi.received.txt` over the baseline when a change is intentional. |

`SdpSamples` joins lines with `Sdp.CRLF` explicitly rather than using a verbatim string. `ToSdp`
splits on CRLF only, so a source file saved with LF endings would hand the parser one giant token and
every test would fail for the wrong reason.

`DiagnosticsTests` and `ParserRegressionTests` redirect `Console.Out` to prove the library writes
nothing there any more. That is safe because xUnit runs tests within one class sequentially — but
keep console-capturing tests confined to as few classes as possible.

`DemoApp` remains useful as a manual check: it round-trips the same offer and prints each stage.
Its output is now byte-identical to its input, and since Phase 3 it prints no parser warnings at
all.
