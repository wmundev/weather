---
name: dotnet-conventions
description: Code conventions for the weather solution - layering, dependency injection, model binding, routing quirks, the weather cache pattern, shared JsonSerializerOptions, test naming, secrets, and formatting. Use when writing or modifying any C# in weather-backend, weather-application, weather-repository, weather-domain, or the test projects.
---

# C# conventions in this solution

These are the patterns the existing code follows. Match them rather than
importing habits from elsewhere.

## Layering

`weather-backend` → `weather-application` + `weather-repository` → `weather-domain`.

Domain never depends on the API. Keep that direction intact.

**Controllers coordinate; they do not compute.** Logic belongs in a service under
`weather-backend/Services/` behind an interface in `Services/Interfaces/`,
registered in `Startup.ConfigureServices`. `weather-application` and
`weather-repository` register their own services through their
`DependencyInjection.cs`.

## Dependency injection

Constructor-inject and null-guard:

```csharp
_client = client ?? throw new ArgumentNullException(nameof(client));
```

Newer files use primary constructors (`CityController(CityList cityList)`) and
still guard. Either is fine; guard regardless.

## Model binding

Be explicit. Use `[FromQuery]`, `[FromRoute]`, `[FromBody]`, and `[Required]`
rather than relying on `[ApiController]` inference - inference for complex types
is the thing that surprises people (`EncryptionController.EncryptMessage` binds
from the query only because `EncryptMessageRequest.Message` carries
`[FromQuery]`).

Give parameters real defaults where the API has one (`units = WeatherUnit.Metric`,
`countryCode = "us"`) instead of special-casing null downstream.

Request DTOs live in `weather-backend/Dto/` or `weather-backend/RequestEntities/`;
types shared across projects live in `weather-domain/`. Use `record` with
`required` init-only properties for request bodies.

## Declaring outcomes

Every action declares all of its outcomes with `[ProducesResponseType]`, failures
included, and carries `<summary>` / `<param>` / `<returns>` / `<response>` XML doc
comments.

These are not decoration - Swashbuckle turns both into `api.yaml`, and the
declared failure codes tell `bruno-collection-sync` which negative cases deserve
their own request.

## Routing

Routing in this repo is deliberately mixed, so choose rather than copy:

- Literal class-level prefix - `[Route("city")]`, `[Route("api/email")]`,
  `[Route("phone-number")]`.
- `[Route("[controller]")]` derives the prefix from the class name minus the
  `Controller` suffix. `LanguageTranslatorController` → `/api/LanguageTranslator`,
  and that casing is what clients must send.
- **A method-level route with a leading `/` is absolute** and discards the class
  prefix entirely. That is why `WeatherForecastController` serves `/weather/*`
  rather than `/WeatherForecast/*`.

Prefer an explicit literal class-level route with relative method routes for new
work. Reserve absolute method routes for holding an existing public URL stable.

## Weather lookups go through the cache

Follow `WeatherForecastController.GetOrFetchAsync`:

1. Generate a cache key from the request DTO via `IWeatherCacheService.GenerateCacheKey`.
2. Return the cached value on a hit.
3. Fetch, then cache, on a miss.
4. Map `HttpRequestException` → `404`, everything else → `400`.

TTL is one hour and the key covers the whole query, units and language included -
so changing any parameter is a different cache entry.

## Shared JsonSerializerOptions

Use `Constants.DefaultJsonOptions` and `Constants.CamelCaseJsonOptions`. Do not
`new` up options per call: `System.Text.Json` caches serialisation metadata per
options object, so a fresh instance throws that cache away. They are `static
readonly` fields rather than properties for exactly this reason.

Integration tests deserialise with `Constants.CamelCaseJsonOptions` to match what
the API emits.

## Tests

xunit + NSubstitute across all three test projects. Name tests
`Method_WhenCondition_ShouldOutcome`.

- Service-level unit tests → `weather-test/Services/`.
- Controller behaviour → `Weather.API.IntegrationTests/Controllers/`, one test
  class per controller. New controllers are expected to keep that one-to-one.

Cover the failure branches you declared with `[ProducesResponseType]`, not just
the happy path.

See the `dotnet-test` skill for running them.

## Secrets

Secrets come from AWS Parameter Store through `ISecretService` / `AllSecrets`.
Never commit credentials. `weather-infra/secret-variables.tfvars` is gitignored
and must stay that way.

## Formatting

`.editorconfig` at the repo root is authoritative. CI runs
`dotnet format --verify-no-changes` as a non-blocking step - run it before
pushing.
