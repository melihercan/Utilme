# Utilme

Utility libraries for .NET, published to NuGet as independent packages.

| Package | Version | Targets | Dependencies | Docs |
|---|---|---|---|---|
| [Utilme.Result](https://www.nuget.org/packages/Utilme.Result) | 26.9.7 | `net10.0` | none | [Getting Started](Result-Getting-Started) |
| [Utilme.SdpTransform](https://www.nuget.org/packages/Utilme.SdpTransform) | 1.0.0 | `net10.0` | none | [SdpTransform](SdpTransform) |

The two libraries are unrelated and share no code.

## Utilme.Result

A small result type for operations that can fail: on success it carries a value, on failure a
[`ResultStatus`](Result-API-Reference#resultstatus) and a message.

```csharp
using Utilme;

Result<User> FindUser(int id) =>
    _users.TryGetValue(id, out var user)
        ? Result.Ok(user)
        : Result.Fail<User>(ResultStatus.NotFound, $"no user with id {id}");
```

→ **[Getting Started](Result-Getting-Started)** · **[API Reference](Result-API-Reference)**

**Read this before adopting it:** [Result vs. ErrorOr](Result-vs-ErrorOr) is an honest assessment of
where this library stands against the mainstream alternatives. If you are starting a new project
with no constraints, that page most likely points you at ErrorOr instead.

## Utilme.SdpTransform

SDP parser and writer — converts between SDP text, an object graph, and JSON. Useful for VoIP and
WebRTC work.

→ **[SdpTransform](SdpTransform)** (documentation pending a refactor; known issues are listed there)

## Repository

- [Design Notes](Result-Design-Notes) — the constraints that shape `Utilme.Result` and the
  invariants any change must preserve.
- [Refactor Log](Result-Refactor-Log) — the .NET 10 migration and the four-phase `Result` refactor,
  including what changed in 26.9.7 and why nothing broke.
- [Building and Testing](Building-and-Testing) — commands, and the `dotnet test` setup gotchas.

## Licence

MIT.
