# Refactor Log

What changed on the way to 26.9.7, and the evidence that it did not break anything.

## Summary

| | Before | 26.9.7 |
|---|---|---|
| Targets | `netstandard2.0` `netstandard2.1` `net5.0` `net6.0` | `net10.0` |
| Build warnings | 11 | 0 |
| Package dependencies (SdpTransform) | 5, one with a known vulnerability | 0 |
| Tests | none | 111 |
| Status + message together | impossible | `Fail(status, message)` |
| Composition operators | none | `Utilme.Functional` |

## .NET 10 migration

Both libraries moved to `net10.0` only. Everything the packages depended on turned out to be in-box:

| Package | Disposition |
|---|---|
| `System.Text.Json` 6.0.2 | dropped — in-box; cleared a known high-severity advisory (NU1903) |
| `System.ComponentModel.Annotations` 5.0.0 | dropped — in-box |
| `System.IO.Pipelines` 6.0.1 | dropped — in-box, and only `System.Buffers` was ever needed |
| `Microsoft.Extensions.DependencyInjection.Abstractions` 6.0.0 | dropped — **never referenced in source** |
| `Macross.Json.Extensions` 2.2.0 | replaced by the built-in `JsonStringEnumConverter<T>` + `[JsonStringEnumMemberName]` |

The `IsExternalInit.cs` shims that made C# 10 `init` accessors compile on netstandard were deleted,
and the explicit `LangVersion 10.0` was removed so the language version follows the SDK.

**Breaking for package consumers**: anyone on .NET Framework or pre-.NET 10 must stay on the previous
release.

One latent bug surfaced during the migration: `BandwidthType.ApplicationSpecific` carried
`[EnumMember(Value = "")]` against a `[Display(Name = "AS")]` — an empty JSON name, which the
built-in converter rejects outright. Corrected to `"AS"`. This changes that member's JSON wire form
from `""` to `"AS"`.

## The four-phase Result refactor

### Phase 0 — safety net

111 tests now, starting from none. The first ones written were **characterization tests**: they pin
the library's existing behaviour, warts included, so that later phases could not change it silently.

Several deliberately assert things that are arguably wrong — that `Value` returns `default` instead
of throwing, that `Ok(null)` is a success, that `NetworkUp()` is a failure. That is the point.

`PublicApiShapeTests` goes further and pins the *shape* of the API by reflection, including a full
public member inventory that fails when anything is added or removed.

### Phase 1 — hygiene, zero API change

- `Nullable` enabled; `Value` annotated `T?`, `ErrorMessage` `string?` — metadata only, binary
  compatible.
- Message-less failures now return cached shared instances.
- XML documentation on every public member, shipped in the package.
- Dead `using` directives removed, file-scoped namespaces, field initializers made explicit.

Exactly one characterization test changed meaning, deliberately: the one asserting that each access
allocated a fresh instance became `Message_less_failures_are_cached_and_shared`.

### Phase 2 — the substantive additions

- **`Fail(status, message)`** — fixes the defect that status and message were mutually exclusive.
  Rejects `ResultStatus.Ok`. With no message it returns the same cached instance as the legacy
  member.
- **Non-generic `Result` factory** — `Result.Ok(42)` infers the type; every status is a method.
- **`Unit`** and `Result.Ok()` for void operations.
- **`IsError`, `TryGetValue`, `GetValueOrDefault`, `ToString()`**.
- **`Utilme.Functional`** — `Match`/`Switch`, `Map`, `Then`, `Tap`/`TapError`, `Else`, plus async
  variants over `Result<T>` and `Task<Result<T>>`.

The API inventory test fired and reported precisely six additions and nothing removed. The inventory
was reviewed and updated rather than the test loosened.

`ToString()` is the only behaviour change in the whole refactor: log output goes from
``Utilme.Result`1[System.Int32]`` to `Ok(42)`.

### Phase 3 — soft deprecation

The seven inconsistently-shaped members got
`[EditorBrowsable(EditorBrowsableState.Never)]` — hidden from IntelliSense, fully functional, and
deliberately not `[Obsolete]` so existing applications compile without a single new warning.

### Phase 4 — not done

Value equality, sealing, struct conversion, removing the legacy members. All breaking; all deferred
to a major version. See [Design Notes](Result-Design-Notes#deferred-to-a-major-version).

## Upgrading to 26.9.7

**Requires .NET 10.** Beyond that, nothing else is required — the API is a strict superset of 1.0.1.

Two things you may notice:

1. **New compiler warnings** if your app has nullable reference types enabled: CS8600/CS8603 on
   unguarded `.Value` reads for reference types. These are correct — `Value` returns `default` on a
   failed result. Fix them with `IsOk`, `TryGetValue` or `GetValueOrDefault`.
2. **The seven legacy per-status members disappear from IntelliSense.** They still compile and still
   behave identically. Replace at your leisure:

| Old | New |
|---|---|
| `Result<T>.NotFound` | `Result.NotFound<T>()` |
| `Result<T>.NotSupported()` | `Result.NotSupported<T>()` |
| `Result<T>.Error("msg")` | unchanged, or `Result.Fail<T>(status, "msg")` for a specific status |

## Version history

| Version | Notes |
|---|---|
| **26.9.7** | .NET 10 only. `Fail`, non-generic factory, `Unit`, safe accessors, `Utilme.Functional`, soft deprecation. |
| 1.0.1 | Added `NetworkUp` and `NetworkDown` status codes. |
| 1.0.0 | Initial release. |

> The project files record `26.09.07`, matching the release date. NuGet and the CLR normalise leading
> zeros, so the published package is **`Utilme.Result.26.9.7`** and `AssemblyVersion` is `26.9.7.0`.
> `FileVersion` keeps the literal `26.09.07`.
