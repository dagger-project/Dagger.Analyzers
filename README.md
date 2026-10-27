# Dagger.Analyzers

Shared analyzers and editorconfig for C# projects. Reference the package once and a project gets the SDK recommended rules, plus Roslynator, Meziantou, threading, async, and disposal analyzers, with one code style. Every analyzer in the package is open source (MIT or Apache-2.0).

The package is a development dependency. It does not ship a library and it does not flow into packages you publish. The analyzer assemblies are packed inside it, so Central Package Management only has to pin this one package.

## Use it

In a project, or in `Directory.Build.props` so every project in the repo picks it up:

```xml
<ItemGroup>
  <PackageReference Include="Dagger.Analyzers" Version="0.1.2" PrivateAssets="all" />
</ItemGroup>
```

With Central Package Management:

```xml
<ItemGroup>
  <PackageVersion Include="Dagger.Analyzers" Version="0.1.2" />
</ItemGroup>
```

`PrivateAssets="all"` still runs the analyzers in that project. It keeps them out of projects that only reference your package.

A repo `.editorconfig` overrides this package. The shared files use analyzer priority 0, so anything you set locally wins.

## What it turns on

Defaults apply only when the project has not set them already:

| Setting | Value |
| --- | --- |
| `AnalysisLevel` | `latest-recommended` |
| `AnalysisMode` | `Recommended` |
| `EnforceCodeStyleInBuild` | `true` |
| `Nullable` | `enable` |
| `ImplicitUsings` | `enable` |
| `LangVersion` | `latest` |

For a new codebase, fail the build on warnings:

```xml
<DaggerAnalyzersTreatWarningsAsErrors>true</DaggerAnalyzersTreatWarningsAsErrors>
```

That stays off by default so bumping the package cannot turn a green build red by surprise.

### Analyzers

These assemblies are bundled in the package. Upstream licenses are in `NOTICE`.

| Package | Why it is here |
| --- | --- |
| SDK .NET analyzers | Recommended correctness and quality rules, including performance rules promoted below |
| [Roslynator.Analyzers](https://github.com/dotnet/roslynator) 5.0.1 | The broad C# correctness and simplification set |
| [Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer) 3.0.299 | Practical API, security, and performance rules |
| [Microsoft.VisualStudio.Threading.Analyzers](https://www.nuget.org/packages/Microsoft.VisualStudio.Threading.Analyzers) 18.12.1 | Async and threading. `VSTHRD200` requires the `Async` suffix on async methods, and rejects it on methods that are not async |
| [AsyncFixer](https://github.com/semihokur/AsyncFixer) 2.1.0 | Forgotten awaits, async void, and other async mistakes |
| [IDisposableAnalyzers.Reloaded](https://www.nuget.org/packages/IDisposableAnalyzers.Reloaded) 5.0.0 | `IDisposable` / `IAsyncDisposable` leaks. Maintained fork of the original package |
| [BannedApiAnalyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.BannedApiAnalyzers) 5.6.0 | Blocks `Thread.Sleep`, `Task.Result`, `Task.Wait`, `GC.Collect`, and `BinaryFormatter` |
| Dagger.Analyzers.CSharp | `DAG0001`: a record property or indexer must not declare `set` |

Pattern matching is a warning: `is` plus a cast, `as` plus a null check, and `not` patterns. Switch expressions are a suggestion, because they are not always the simpler form.

## Records

`DAG0001` warns when a property or indexer declared on a record uses `set`, including `private set`. `init` and get-only members stay valid, so a positional record and a `with` expression keep working. A class nested inside a record can still use `set`.

Performance rules promoted to warning: `CA1826`, `CA1827`, `CA1828`, `CA1829`, `CA1836`, `CA1860`, `CA1861`, `CA1869`, `CA1849`, `CA2016`, `CA2012`, `CA2024`, and Roslynator `RCS1198` (unnecessary boxing).

Complexity: cyclomatic complexity `CA1502` warns above 20. That number is `CA1502: 20` in the packaged `CodeMetricsConfig.txt`. Add your own additional file with that name to use a different threshold. Roslynator `RCS1208` warns when an `if` can be inverted so the nested block sits at a shallower level. Maintainability index and class coupling stay suggestions.

`ConfigureAwait(false)` (`VSTHRD111`) is silent. It is not required for most APIs on modern .NET. Library projects that want it can set `dotnet_diagnostic.VSTHRD111.severity = warning` in their editorconfig.

Joinable Task Factory rules (`VSTHRD001`, `VSTHRD004`, `VSTHRD010`–`VSTHRD012`) are off. They apply to Visual Studio extensions. Turn them back on in a VS SDK project.

XML documentation on every member is off. Say what a public API needs in a doc comment when it helps, and leave the rest alone.

Build enforcement comes from the global config in the package: file-scoped namespaces, braces, pattern matching, naming, and the analyzer severities. Indentation and new lines are in `Dagger.Analyzers.editorconfig`. Copy that file to the repo root when you want `dotnet format` and the editor to use the same layout. A repo `.editorconfig` overrides the package.

## Opt out

```xml
<!-- Keep your own banned-API list. -->
<DaggerAnalyzersBannedSymbols>false</DaggerAnalyzersBannedSymbols>
```

Set `AnalysisLevel`, `Nullable`, `ImplicitUsings`, or any `dotnet_diagnostic.*.severity` in the project or in a repo editorconfig. Those values win.

## Left out on purpose

- **StyleCop.** Ordering and documentation rules overlap this editorconfig and fight the goal of keeping code small.
- **Roslynator.Formatting.Analyzers.** Those rules are off by default and duplicate `dotnet format`.
- **Roslynator.CodeAnalysis.Analyzers.** Useful only in projects that reference Roslyn. Add that package there.
- **ErrorProne.NET.** Still prerelease.
- **SonarAnalyzer.CSharp.** Source-available under the Sonar Source-Available License, which is not an open-source license. Security checks come from Meziantou and the SDK analyzers instead.
- **SecurityCodeScan.** Unmaintained. Meziantou and the SDK security rules cover that ground.

## Check the package

`samples/Clean` is code that should build with `-warnaserror`. `samples/Violations` is code that should report the async suffix, async void, pattern matching, banned APIs, undisposed streams, and complexity.

```sh
eng/verify.sh
```

Pull requests and pushes to `main` run that script on the .NET 11 SDK pinned in `global.json`.

## Publish

Push a tag that matches `<Version>` in the project, such as `v0.1.2`. The release workflow verifies the package and publishes that nupkg to nuget.org. A prerelease version such as `0.2.0-preview.1` is tagged `v0.2.0-preview.1`.

nuget.org needs a Trusted Publishing policy for this repository before the first push:

| Field | Value |
| --- | --- |
| Repository owner | `dagger-project` |
| Repository | `Dagger.Analyzers` |
| Workflow file | `release.yml` |
| Environment | `nuget` |

The repository variable `NUGET_USER` is the nuget.org profile name. The workflow exchanges a GitHub token for a short-lived NuGet API key, so the repository does not store one.
