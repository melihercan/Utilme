# Utilme

Utility libraries for .NET, published as independent NuGet packages.

📖 **[Documentation is in the wiki](https://github.com/melihercan/Utilme/wiki)**

| Package | Version | Targets | Dependencies |
|---|---|---|---|
| [Utilme.Result](Result/README.md) — a generic Result library. On success it returns a result object; on failure, an error. | [![NuGet](https://img.shields.io/nuget/v/Utilme.Result.svg)](https://www.nuget.org/packages/Utilme.Result) | `net10.0` | none |
| [Utilme.SdpTransform](SdpTransform/README.md) — SDP parser and writer. Transforms between SDP text, object, and JSON representations. | [![NuGet](https://img.shields.io/nuget/v/Utilme.SdpTransform.svg)](https://www.nuget.org/packages/Utilme.SdpTransform) | `net10.0` | none |

The two libraries are unrelated and share no code.

## Documentation

| Page | What's in it |
|---|---|
| [Getting Started](https://github.com/melihercan/Utilme/wiki/Result-Getting-Started) | Installing and using `Utilme.Result` |
| [API Reference](https://github.com/melihercan/Utilme/wiki/Result-API-Reference) | Every public member |
| [Result vs. ErrorOr](https://github.com/melihercan/Utilme/wiki/Result-vs-ErrorOr) | An honest comparison against the mainstream alternatives — read this before adopting |
| [Design Notes](https://github.com/melihercan/Utilme/wiki/Result-Design-Notes) | Why the library looks the way it does, and what must not change |
| [Refactor Log](https://github.com/melihercan/Utilme/wiki/Result-Refactor-Log) | The .NET 10 migration and the upgrade guide |
| [Building and Testing](https://github.com/melihercan/Utilme/wiki/Building-and-Testing) | Commands and the test setup |
| [SdpTransform](https://github.com/melihercan/Utilme/wiki/SdpTransform) | Overview and known issues (full docs pending a refactor) |

## Building

Requires the **.NET 10 SDK**.

```powershell
dotnet build Utilme.sln
dotnet test
dotnet run --project DemoApp
```

The build is expected to be clean: 0 warnings, 0 errors. Both libraries set
`GeneratePackageOnBuild`, so every build produces a `.nupkg` in `bin/<Config>/`.

See [Building and Testing](https://github.com/melihercan/Utilme/wiki/Building-and-Testing) for the
`dotnet test` setup — the repository uses xUnit v3 on Microsoft.Testing.Platform, which has a couple
of command-line gotchas.

## Licence

[MIT](LICENSE).
