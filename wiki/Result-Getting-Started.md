# Utilme.Result — Getting Started

```
dotnet add package Utilme.Result
```

Requires **.NET 10**. Version 26.9.7 dropped `netstandard2.0`, `netstandard2.1`, `net5.0` and
`net6.0`; if you need those, stay on 1.0.1.

The package has **no dependencies**.

## Producing results

Use the non-generic `Result` factory — it infers the value type from the argument:

```csharp
using Utilme;

Result<User> FindUser(int id)
{
    if (id <= 0)
        return Result.Fail<User>(ResultStatus.InvalidData, $"id {id} is not positive");

    if (!_users.TryGetValue(id, out var user))
        return Result.Fail<User>(ResultStatus.NotFound, $"no user with id {id}");

    return Result.Ok(user);           // infers Result<User>
}
```

`Fail` is the general-purpose failure factory and the only one that pairs a **specific status with
your own message**. Pass no message and the status name is used:

```csharp
Result.Fail<User>(ResultStatus.Timeout)              // ErrorMessage == "Timeout"
Result.Fail<User>(ResultStatus.Timeout, "took 30s")  // ErrorMessage == "took 30s"
```

`Fail` throws `ArgumentOutOfRangeException` if you pass `ResultStatus.Ok` — that is not a failure.

For operations that succeed without producing a value, use `Result<Unit>`:

```csharp
Result<Unit> Save(User user)
{
    if (!_disk.HasSpace) return Result.Fail<Unit>(ResultStatus.Error, "disk full");
    _disk.Write(user);
    return Result.Ok();               // Result<Unit>
}
```

## Consuming results

```csharp
var result = FindUser(7);

if (result.IsOk)
    Console.WriteLine(result.Value!.Name);
else
    Console.WriteLine($"{result.Status}: {result.ErrorMessage}");
```

> [!WARNING]
> `Value` returns `default` on a failed result — it does **not** throw. Always check `IsOk` (or
> `IsError`) first, or use one of the safe accessors below.

```csharp
if (result.TryGetValue(out var user))
    Console.WriteLine(user.Name);

var name = FindUser(7).GetValueOrDefault(User.Anonymous).Name;
```

`ToString()` is log-friendly: `Ok(42)`, `NotFound`, or `NotFound: no user with id 7`.

## Composing results

The composition operators live in a separate namespace, so importing them is deliberate:

```csharp
using Utilme;
using Utilme.Functional;
```

Every operator **short-circuits**: if the input already failed, your delegate never runs and the
failure propagates with its status and message intact.

```csharp
string message = FindUser(7)
    .Map(u => u.Email)                          // Result<User> -> Result<string>
    .Then(e => Validate(e))                     // chain another result-returning call
    .Tap(e => _log.Info($"sending to {e}"))     // side effect on success only
    .Match(e => $"ok: {e}",                     // collapse both branches to one value
           f => $"failed: {f.ErrorMessage}");
```

| Operator | Purpose |
|---|---|
| `Map` | transform the value, keep it a result |
| `Then` | chain an operation that itself returns a result |
| `Match` | collapse success and failure into one value |
| `Switch` | run one of two side effects |
| `Tap` / `TapError` | observe without changing the result |
| `Else` | unwrap, computing a substitute on failure |

Async variants work over both `Result<T>` and `Task<Result<T>>`, so a whole pipeline stays fluent:

```csharp
var label = await FetchUserAsync(7)                       // Task<Result<User>>
    .MapAsync(u => u.Email)
    .ThenAsync(e => ValidateAsync(e))
    .MatchAsync(e => $"ok: {e}", f => f.Status.ToString());
```

`Match` takes functions and `Switch` takes actions — they are named differently on purpose, because
a lambda whose body is a method call would otherwise be ambiguous between the two.

## A note on the older API

You will find `Result<T>.NotFound`, `Result<T>.NotSupported()` and five siblings in existing code.
They still work and are not deprecated, but they are hidden from IntelliSense because some are
properties and some are methods — see [Design Notes](Result-Design-Notes#the-two-surfaces). New code
should use `Result.NotFound<T>()` or `Fail`.

One trap worth knowing: **`NetworkUp()` is a failure**, not a success. See
[Design Notes](Result-Design-Notes#networkup-is-a-failure).
