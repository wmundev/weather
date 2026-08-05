---
name: add-api-endpoint
description: End-to-end checklist for adding or changing an HTTP endpoint in weather-backend - controller action, XML docs, unit and integration tests, Bruno collection entry, and regenerated OpenAPI spec. Use when adding a new controller or action method, changing a route or verb, adding query/route/body parameters, or when asked to expose something over HTTP in this API.
---

# Adding an API endpoint

An endpoint is not finished when the action method compiles. In this repo it is
finished when the controller, the tests, the Bruno collection, and `api.yaml`
all agree.

Work through the steps in order. Do not stop once the action compiles.

## 1. Write the action

**Read the `dotnet-conventions` skill first** - it carries the routing rules
(including the leading-slash-is-absolute trap), DI null-guards, explicit binding,
`[ProducesResponseType]` and XML doc requirements, DTO placement, and the
`IWeatherCacheService` pattern for anything that calls OpenWeatherMap.

Endpoint-specific reminders on top of that:

- Controllers live in `weather-backend/Controllers/`, inherit `ControllerBase`,
  and carry `[ApiController]`.
- Return `ActionResult<T>` (or `Task<ActionResult<T>>`) so both the payload and
  the status code are expressible.
- The `[ProducesResponseType]` failure codes you declare here are what
  `bruno-collection-sync` uses to decide which negative cases get their own
  request. Declaring them is not optional.

## 2. Tests

Both layers are expected, and CI runs them separately.

**Unit tests** - `weather-test/Services/`. Test the service, not the controller.

**Integration tests** - `Weather.API.IntegrationTests/Controllers/`. There is one
test class per controller and new controllers are expected to keep that up.

```csharp
public sealed class ThingControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
{
    private readonly HttpClient _client;

    public ThingControllerTests(CustomWebApplicationFactory<Startup> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetThing_WhenCalledWithValidInput_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/thing?id=1");

        response.EnsureSuccessStatusCode();
    }
}
```

Deserialize with `Constants.CamelCaseJsonOptions`. Cover the failure branches you
declared with `[ProducesResponseType]`, not just the happy path.

## 3. Update the Bruno collection - required

**Invoke the `bruno-collection-sync` skill.** Every endpoint in
`weather-backend/Controllers/` has a corresponding request in `bruno/`, and that
invariant is the whole point of keeping the collection in the repo.

Do not hand-write the `.bru` file from memory; the skill carries the conventions
(`{{baseUrl}}`, `auth: inherit`, path params as `:name`, when to assert a status,
required `docs` blocks).

## 4. Regenerate the OpenAPI spec

`api.yaml` is generated from the built assembly. The `pre-commit` hook in
`.githooks/` regenerates and stages it, so if hooks are installed
(`git config core.hooksPath .githooks`) this happens for you.

Otherwise run the `openapi-docs` skill, or:

```bash
cd scripts && ./generate-openapi-docs.sh
```

## 5. Verify

```bash
dotnet build --configuration Release
```

Then run the tests - see the `dotnet-test` skill.

## Definition of done

- [ ] Action method with explicit binding attributes and `[ProducesResponseType]`
- [ ] XML doc comments on the action
- [ ] Service logic behind an interface, registered in `Startup`
- [ ] Unit tests in `weather-test/`
- [ ] Integration test in `Weather.API.IntegrationTests/Controllers/`
- [ ] Bruno request added via `bruno-collection-sync`
- [ ] `api.yaml` regenerated
- [ ] `dotnet build` and tests pass
