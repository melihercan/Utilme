# Utilme.Result

[![NuGet](https://img.shields.io/nuget/v/Utilme.Result.svg)](https://www.nuget.org/packages/Utilme.Result)

A generic Result library. On success it returns a result object; on failure, a status and an error
message.

```
dotnet add package Utilme.Result
```

Requires **.NET 10**. No dependencies.

📖 **[Full documentation is in the wiki](https://github.com/melihercan/Utilme/wiki/Result-Getting-Started)**

## At a glance

```csharp
using Utilme;

Result<User> FindUser(int id)
{
    if (id <= 0)
        return Result.Fail<User>(ResultStatus.InvalidData, $"id {id} is not positive");

    if (!_users.TryGetValue(id, out var user))
        return Result.Fail<User>(ResultStatus.NotFound, $"no user with id {id}");

    return Result.Ok(user);
}

var result = FindUser(7);

if (result.TryGetValue(out var user))
    Console.WriteLine(user.Name);
else
    Console.WriteLine($"{result.Status}: {result.ErrorMessage}");
```

Composition operators are opt-in, via `using Utilme.Functional;`:

```csharp
string message = FindUser(7)
    .Map(u => u.Email)
    .Then(e => Validate(e))
    .Match(e => $"ok: {e}", f => $"failed: {f.ErrorMessage}");
```

> [!WARNING]
> `Value` returns `default` on a failed result — it does **not** throw. Check `IsOk`, or use
> `TryGetValue` / `GetValueOrDefault`.

## Documentation

| Page | What's in it |
|---|---|
| [Getting Started](https://github.com/melihercan/Utilme/wiki/Result-Getting-Started) | Producing, consuming and composing results |
| [API Reference](https://github.com/melihercan/Utilme/wiki/Result-API-Reference) | Every public member, and the `ResultStatus` contract |
| [Result vs. ErrorOr](https://github.com/melihercan/Utilme/wiki/Result-vs-ErrorOr) | Honest comparison against the mainstream alternatives |
| [Design Notes](https://github.com/melihercan/Utilme/wiki/Result-Design-Notes) | Why it looks like this; invariants to preserve |
| [Refactor Log](https://github.com/melihercan/Utilme/wiki/Result-Refactor-Log) | Upgrade guide and version history |

## Should you use this?

If you are starting a new project with no constraints, **[ErrorOr](https://github.com/amantinband/error-or)
is probably the better choice** — it does everything this library does and more.
[Result vs. ErrorOr](https://github.com/melihercan/Utilme/wiki/Result-vs-ErrorOr) sets out the
comparison in full, including the known design flaws that are preserved here for backward
compatibility and the one area where this library has a genuine edge: a closed, domain-specific
status vocabulary for transport and networking code.

## Upgrading to 26.9.7

The API is a strict superset of 1.0.1, but the package now requires **.NET 10** —
`netstandard2.0`/`2.1` and `net5.0`/`net6.0` are no longer supported. See the
[Refactor Log](https://github.com/melihercan/Utilme/wiki/Result-Refactor-Log#upgrading-to-2697).

## Licence

[MIT](../LICENSE).
