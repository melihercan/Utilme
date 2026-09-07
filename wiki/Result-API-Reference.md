# Utilme.Result — API Reference

Assembly `Utilme.Result`, version 26.9.7, target `net10.0`, no dependencies.

Two namespaces:

- **`Utilme`** — `Result<T>`, `Result`, `ResultStatus`, `Unit`
- **`Utilme.Functional`** — `ResultExtensions` (composition operators, opt-in)

---

## `Result<T>`

A class (not a struct), immutable, reference-equality.

### Instance members

| Member | Notes |
|---|---|
| `T? Value` | The value on success. Returns `default` on failure — **does not throw**. |
| `bool IsOk` | Whether the operation succeeded. |
| `bool IsError` | The inverse of `IsOk`. |
| `ResultStatus Status` | `Ok` on success, otherwise the failure status. |
| `string? ErrorMessage` | `null` on success. The status name when no message was supplied. |
| `bool TryGetValue(out T value)` | `true` on success, with `value` set; otherwise `false`. |
| `T GetValueOrDefault(T fallback)` | The value on success, `fallback` on failure. |
| `string ToString()` | `Ok(42)` · `NotFound` · `NotFound: no user with id 7` |
| `Result(T value)` | Public constructor for a success. Equivalent to `Ok(value)`. |

### Static factories

| Member | Notes |
|---|---|
| `Ok(T value)` | A successful result. `value` may be `null`. |
| `Error(string message)` | A failure with `Status == ResultStatus.Error`. Stores `message` verbatim, `null` included. |
| `Fail(ResultStatus status, string? message = null)` | The general-purpose failure factory. Throws `ArgumentOutOfRangeException` for `ResultStatus.Ok`. |

### Legacy per-status members

Hidden from IntelliSense, fully supported, **not** `[Obsolete]`.

| Member | Shape |
|---|---|
| `NotFound` | property |
| `Timeout` | property |
| `Cancelled` | property |
| `NotSupported()` | method |
| `InvalidData()` | method |
| `NetworkUp()` | method — **reports failure** despite the name |
| `NetworkDown()` | method |

The inconsistent shapes cannot be unified without a breaking removal; see
[Design Notes](Result-Design-Notes#the-two-surfaces).

---

## `Result` (static)

Factories that infer the value type and give every status the same shape. This is the surface new
code should use.

| Member | Returns |
|---|---|
| `Ok<T>(T value)` | `Result<T>` — type inferred from the argument |
| `Ok()` | `Result<Unit>` — success with no value |
| `Fail<T>(ResultStatus status, string? message = null)` | `Result<T>` |
| `Error<T>(string message)` | `Result<T>` |
| `NotFound<T>()` · `Timeout<T>()` · `Cancelled<T>()` | `Result<T>` |
| `NotSupported<T>()` · `InvalidData<T>()` | `Result<T>` |
| `NetworkUp<T>()` · `NetworkDown<T>()` | `Result<T>` |

Legal alongside `Result<T>` because the arity differs — the same pattern as `Task` and `Task<T>`.

---

## `ResultStatus`

An `int`-backed enum. **The numeric values are part of the contract**: callers may have persisted or
transmitted them, so new members are only ever appended.

| Name | Value | Meaning |
|---|---|---|
| `Ok` | 0 | Succeeded. |
| `Error` | 1 | Failed with a caller-supplied message. |
| `NotFound` | 2 | The requested item does not exist. |
| `Timeout` | 3 | Did not complete within its time budget. |
| `Cancelled` | 4 | Cancelled before completing. |
| `NotSupported` | 5 | The operation is not supported. |
| `InvalidData` | 6 | The supplied data was malformed. |
| `NetworkUp` | 7 | The network came up. Results with this status report `IsOk == false`. |
| `NetworkDown` | 8 | The network went down. |

---

## `Unit`

A `readonly struct` with exactly one value, used as the payload for results that succeed without
producing anything. `Unit.Value`, all instances equal, `ToString()` returns `()`.

Create one with `Result.Ok()`.

---

## `Utilme.Functional.ResultExtensions`

Extension methods on `Result<T>`. Requires `using Utilme.Functional;`.

Every operator short-circuits on failure: the delegate is not invoked and the failure propagates
with `Status` and `ErrorMessage` preserved exactly, including a `null` message. All of them throw
`ArgumentNullException` for a null delegate.

### Synchronous

| Signature | Purpose |
|---|---|
| `TOut Match<T, TOut>(Func<T, TOut> onOk, Func<Result<T>, TOut> onError)` | Collapse both outcomes into one value. |
| `void Switch<T>(Action<T> onOk, Action<Result<T>> onError)` | Run one of two side effects. |
| `Result<TOut> Map<T, TOut>(Func<T, TOut> map)` | Transform the value. |
| `Result<TOut> Then<T, TOut>(Func<T, Result<TOut>> next)` | Chain a result-returning operation. |
| `Result<T> Tap<T>(Action<T> action)` | Observe success, return the original. |
| `Result<T> TapError<T>(Action<Result<T>> action)` | Observe failure, return the original. |
| `T Else<T>(Func<Result<T>, T> fallback)` | Unwrap, computing a substitute on failure. |

### Asynchronous

On `Result<T>`:

| Signature |
|---|
| `Task<Result<TOut>> MapAsync<T, TOut>(Func<T, Task<TOut>> map)` |
| `Task<Result<TOut>> ThenAsync<T, TOut>(Func<T, Task<Result<TOut>>> next)` |

On `Task<Result<T>>`, so pipelines stay fluent without intermediate `await`s:

| Signature |
|---|
| `Task<Result<TOut>> MapAsync<T, TOut>(Func<T, TOut> map)` |
| `Task<Result<TOut>> ThenAsync<T, TOut>(Func<T, Task<Result<TOut>>> next)` |
| `Task<TOut> MatchAsync<T, TOut>(Func<T, TOut> onOk, Func<Result<T>, TOut> onError)` |
| `Task<Result<T>> TapAsync<T>(Action<T> action)` |

All awaits use `ConfigureAwait(false)`.
