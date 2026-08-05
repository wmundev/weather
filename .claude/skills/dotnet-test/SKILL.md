---
name: dotnet-test
description: Runs the weather solution's test suites the same way CI does - API unit tests, CLI unit tests, and integration tests - with optional Cobertura coverage collection. Use when asked to run the tests, run unit or integration tests, check coverage, or verify a change builds and passes before committing.
---

# Running the tests

The solution has three separate test projects and CI runs each with its own
`dotnet test` invocation from its own working directory. Running `dotnet test`
at the solution root works but hides which suite failed and mixes the coverage
output, so mirror the CI layout.

## Project map

| Project                        | Kind              | Needs external services |
| ------------------------------ | ----------------- | ----------------------- |
| `weather-test`                 | API unit tests    | no                      |
| `Weather.CLI.UnitTests`        | CLI unit tests    | no                      |
| `Weather.API.IntegrationTests` | API integration   | **yes** - AWS + ConfigCat |

All three are xunit + NSubstitute on .NET 10, with `coverlet.collector` for
coverage.

## Fast loop

Build once, then run the two dependency-free suites:

```bash
dotnet build --configuration Release
```

```bash
dotnet test weather-test --no-build --configuration Release
```

```bash
dotnet test Weather.CLI.UnitTests --no-build --configuration Release
```

Use `--no-build` only when the build above succeeded and nothing changed since;
drop it if in doubt.

To run a single test or class:

```bash
dotnet test weather-test --filter "FullyQualifiedName~WeatherCacheServiceTest"
```

## Integration tests

These spin up the real host through `CustomWebApplicationFactory<Startup>`, so
they need AWS credentials (Parameter Store / DynamoDB, `us-east-1`) and the
ConfigCat SDK key. CI supplies them via `ci-scripts/setup_env.sh` and repository
secrets.

```bash
dotnet test Weather.API.IntegrationTests --configuration Release
```

If they fail with credential or secret-loading errors rather than assertion
failures, that is a missing local environment, not a regression - say so instead
of chasing the assertion.

## With coverage

This is what CI runs:

```bash
dotnet test weather-test --no-build --no-restore --configuration Release --collect:"XPlat Code Coverage" --results-directory ./coverage
```

Each project writes `coverage/<guid>/coverage.cobertura.xml` under its own
directory. Coverage is uploaded to Codecov (`wmundev/weather`) from the
`coverage` job, which merges the unit and integration artifacts - so a local
number from one project alone will read lower than the badge.

The root `/coverage` directory is gitignored.

## Formatting gate

CI also runs, as a non-blocking step:

```bash
dotnet format --verify-no-changes --verbosity diagnostic
```

Run it before pushing. Formatting rules come from `.editorconfig` at the repo
root.

## Reporting results

State plainly which suites ran, which passed, and which were skipped for missing
credentials. Do not describe a change as verified if only the unit tests ran and
the change touched a controller - controllers are covered by
`Weather.API.IntegrationTests`.
