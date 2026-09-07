# Utilme.Result — Design Notes

Why the library looks the way it does, and the invariants any change must preserve. If you are
about to modify `Utilme.Result`, read this first.

## The governing constraint

Real applications depend on this package. Every change since 1.0.1 has been **additive**: nothing
removed, nothing renamed, no behaviour altered. The
[characterization test suite](Building-and-Testing#characterization-tests) exists to enforce that
mechanically rather than by good intentions.

## The two surfaces

`Result<T>`'s per-status members are inconsistently shaped:

```csharp
Result<T>.NotFound          // property
Result<T>.NotSupported()    // method
```

This cannot be fixed in place. C# forbids a member name being both a property and a method, so
promoting the methods or demoting the properties requires **deleting** the other form — source- and
binary-breaking for every caller.

The resolution was to add a clean surface **beside** the old one:

- `Result<T>.Fail(status, message)` — the general-purpose failure factory
- the non-generic `Result` class — every status a method, value type inferred

and then hide the seven legacy members with
`[EditorBrowsable(EditorBrowsableState.Never)]`. They keep working; they simply stop appearing in
completion lists so new code does not reach for them.

They are deliberately **not** `[Obsolete]`. That would spam warnings through applications already
using them, for no benefit. `SoftDeprecationTests` pins both halves of this policy — that the seven
are hidden, and that nothing anywhere is marked obsolete.

> `EditorBrowsable` is honoured by the IDE only for members arriving from a referenced *assembly*.
> Projects inside this solution still see them in completion, which is why the test project can keep
> exercising them.

## `NetworkUp` is a failure

`Result<T>.NetworkUp()` reports `IsOk == false`.

Semantically backwards, and permanent. Flipping it would silently invert `if (result.IsOk)` branches
in every consuming application — a change with no compile error to catch it and no runtime signal
that anything moved. The test `NetworkUp_is_a_FAILURE_despite_its_name` carries a do-not-fix comment.

## Instance caching

Failures carrying no caller-supplied message return a **shared instance**, one per closed generic
type:

```csharp
ReferenceEquals(Result<int>.NotFound, Result<int>.NotFound)      // true
ReferenceEquals(Result<int>.NotFound, Result<string>.NotFound)   // false — different closed types
```

`Ok`, `Error` and `Fail(status, message)` still allocate, because they carry data.

This is only safe while `Result<T>` is immutable. **Anything added to the type must stay immutable.**

`Cached(status)` falls back to allocating for a status it does not recognise, so appending a member
to `ResultStatus` cannot throw at runtime if someone forgets to register it there.

## Nullability

`Value` is `T?` and `ErrorMessage` is `string?`.

These are metadata-only annotations — the IL signatures are unchanged, so the change is binary
compatible. In particular `T?` on an unconstrained type parameter does **not** become `Nullable<T>`;
`Nullable_annotations_did_not_change_the_runtime_signatures` pins that.

Source-wise, consumers with nullable enabled now get CS8600/CS8603 on unguarded `.Value` reads for
reference types. That is intended: it surfaces the pre-existing hazard that `Value` returns `default`
on failure rather than throwing.

## `ResultStatus` is append-only

The numeric values are part of the contract — callers may have persisted or transmitted them as
integers. New members go at the **end**, never inserted or reordered.
`ResultStatusCharacterizationTests` freezes the values, names, count and underlying type; adding one
requires deliberately updating the count.

## XML documentation is mandatory

The project sets `GenerateDocumentationFile`, so **every public member must carry XML docs** or the
build fails its own zero-warning bar via CS1591. The docs ship inside the package, so they are what
consumers see in IntelliSense — the primary discovery mechanism for the newer API.

## Why the combinators live in `Utilme.Functional`

They are extension methods on `Result<T>`. Had they been in `Utilme`, they would have been in scope
for every existing consumer automatically — and if an application already defined its own `.Match()`
or `.Map()` extension on `Result<T>`, having both in scope is an **ambiguity compile error**, not a
warning.

Putting them in their own namespace makes importing them a deliberate act and keeps the upgrade
safe. `Match` (functions) and `Switch` (actions) are separate names rather than overloads, because a
lambda whose body is a method call would otherwise be ambiguous between them.

## Deferred to a major version

Not done, and each breaking:

- **Value equality** — would change `HashSet`/dictionary behaviour for anyone using results as keys.
- **`sealed`** — technically breaking for a derived type, however unlikely one is.
- **Converting to a struct** — breaks `null` checks, `default`, and boxing assumptions.
- **Removing the legacy members** — the reason the whole two-surface design exists.
- **An ErrorOr interop shim** — sketched in [Result vs. ErrorOr](Result-vs-ErrorOr#if-you-do-migrate).
