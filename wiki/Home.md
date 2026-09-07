# Utilme

Utility libraries for .NET, published to NuGet as independent packages.

| Package | Version | Targets | Dependencies | Docs |
|---|---|---|---|---|
| [Utilme.Result](https://www.nuget.org/packages/Utilme.Result) | 26.9.7 | `net10.0` | none | [Getting Started](Result-Getting-Started) |
| [Utilme.SdpTransform](https://www.nuget.org/packages/Utilme.SdpTransform) | 26.9.7 | `net10.0` | none | [Getting Started](SdpTransform-Getting-Started) |

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

```csharp
using Utilme.SdpTransform;

Sdp? sdp = sdpText.ToSdp();          // null if it cannot be parsed
string text = sdp!.ToText();         // byte-identical to what it parsed from
```

→ **[Getting Started](SdpTransform-Getting-Started)** · **[API Reference](SdpTransform-API-Reference)**

## Repository

- Design Notes — the constraints that shape each library and the invariants any change must
  preserve: [Result](Result-Design-Notes), [SdpTransform](SdpTransform-Design-Notes).
- Refactor Logs — the .NET 10 migration and what changed in 26.9.7:
  [Result](Result-Refactor-Log), [SdpTransform](SdpTransform-Refactor-Log).
- [Building and Testing](Building-and-Testing) — commands, and the `dotnet test` setup gotchas.

Both packages are released by GitHub Actions on a `result-v*` / `sdptransform-v*` tag.

## Licence

MIT.
