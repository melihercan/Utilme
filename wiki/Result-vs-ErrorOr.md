# Result vs. ErrorOr

An honest assessment of where `Utilme.Result` stands against the mainstream alternatives —
[ErrorOr](https://github.com/amantinband/error-or) in particular, but the same reasoning applies to
FluentResults, OneOf and CSharpFunctionalExtensions.

This page exists because the question deserves a straight answer rather than a sales pitch.

## The short version

**If you are starting fresh with no constraints, use ErrorOr.** There is no capability
`Utilme.Result` has that ErrorOr lacks, and ErrorOr is better on nearly every axis that matters.

`Utilme.Result` continues to exist and be maintained because real applications already depend on it,
and because its closed status vocabulary happens to fit the transport and networking code it grew
out of. Those are the honest reasons — not technical superiority.

## Where ErrorOr is simply better

| | Utilme.Result | ErrorOr |
|---|---|---|
| Implicit conversions | none — every return is an explicit factory call | `return value;` and `return Error.NotFound();` |
| Multiple errors | one message | `List<Error>` |
| Error richness | a `string` plus a closed enum | code, description, type, metadata dictionary |
| Extensible error types | no — adding a status means editing and republishing the package | yes |
| Allocation | class; every result allocates (except cached failures) | `readonly record struct` |
| Void results | `Result<Unit>` | `ErrorOr<Success>` |
| Ecosystem | one repository, one consumer | widely used, documented, battle-tested |

Since 26.9.7 the composition gap is much narrower — `Utilme.Result` now has `Match`, `Map`, `Then`,
`Tap`, `Else` and async variants in [`Utilme.Functional`](Result-API-Reference#utilmefunctionalresultextensions).
Before that release it had no combinators at all, and callers wrote `if (result.IsOk)` ladders by
hand.

## Design flaws you should know about

These are real, and they are preserved deliberately because changing them would break existing
applications. Each is pinned by a characterization test so it cannot drift silently.

### Status and message were mutually exclusive

The original API could express "NotFound" **or** "here is what went wrong", never both:
`Error(string)` always forced `ResultStatus.Error`.

**Fixed in 26.9.7** by [`Fail(status, message)`](Result-API-Reference#static-factories).
`Error(string)` keeps its old behaviour for compatibility.

### `Value` is readable on failure

It returns `default` rather than throwing, and nothing guards it. A caller who forgets to check
`IsOk` silently gets `null` or `0` instead of an exception at the point of the mistake.

Not changed — code may already rely on it. 26.9.7 added `TryGetValue` and `GetValueOrDefault`
alongside, and nullable annotations now make the compiler warn on unguarded reads.

### `NetworkUp` is a failure

`Result<T>.NetworkUp()` reads like a success but reports `IsOk == false`.

**This will not be changed.** Flipping the polarity would silently invert branches in every
consuming application — the most dangerous change available in this library. It is documented on the
member, in the enum, and pinned by a test named `NetworkUp_is_a_FAILURE_despite_its_name`.

### The per-status members are inconsistently shaped

`NotFound` is a property; `NotSupported()` is a method. C# forbids one name being both, so this
cannot be unified in place without a source- and binary-breaking removal.

**Worked around in 26.9.7** by adding a consistent surface beside it — the non-generic `Result`
factory — and hiding the seven legacy members from IntelliSense. See
[Design Notes](Result-Design-Notes#the-two-surfaces).

### A "successful" result can wrap null

`Result.Ok<string>(null)` is a success carrying `null`. Not changed: callers may legitimately do
this.

### No value equality

Two structurally identical results are not equal. Results are therefore safe as reference-semantics
dictionary keys, but `Assert.Equal(expected, actual)` will not do what you expect. Changing this is
deferred to a major version.

## The one genuine advantage

A **closed, domain-specific status vocabulary**.

`ResultStatus` encodes a fixed set — `Timeout`, `Cancelled`, `NetworkUp`, `NetworkDown`,
`InvalidData` — that came out of VoIP and WebRTC work. Because it is a sealed enum you get
compile-time exhaustiveness when you `switch` over it, with no magic numbers:

```csharp
var retry = result.Status switch
{
    ResultStatus.Timeout or ResultStatus.NetworkDown => true,
    ResultStatus.NotFound or ResultStatus.InvalidData => false,
    _ => false,
};
```

ErrorOr's built-in `ErrorType` covers `Failure`, `Unexpected`, `Validation`, `Conflict`,
`NotFound`, `Unauthorized` and `Forbidden` — a web-API vocabulary. Anything else goes through
`Error.Custom(int type, ...)`, where the type is an `int` you define and manage yourself.

That is a mild advantage, and it is the only one. **"Zero dependencies" is not a differentiator** —
ErrorOr is dependency-free too.

## Which should you use?

**New project, free choice** → ErrorOr.

**Already on `Utilme.Result`** → staying is reasonable. 26.9.7 closed the composition gap and the
status/message defect, and the library is tested and documented. The cost of migrating is real and
the benefit is incremental.

**You want the closed status vocabulary specifically** → `Utilme.Result` gives it to you directly;
with ErrorOr you would build it on top of `Error.Custom`.

## If you do migrate

There is no interop shim, but the mapping is mechanical:

```csharp
public static ErrorOr<T> ToErrorOr<T>(this Result<T> result) =>
    result.IsOk
        ? result.Value!
        : result.Status switch
        {
            ResultStatus.NotFound => Error.NotFound(description: result.ErrorMessage!),
            ResultStatus.InvalidData => Error.Validation(description: result.ErrorMessage!),
            ResultStatus.Cancelled or ResultStatus.Timeout
                => Error.Failure(description: result.ErrorMessage!),
            _ => Error.Unexpected(description: result.ErrorMessage!),
        };
```

Note what is lost in that direction: `Timeout`, `Cancelled`, `NetworkUp` and `NetworkDown` have no
distinct ErrorOr counterpart and collapse into `Failure` unless you define custom error types to
carry them.
