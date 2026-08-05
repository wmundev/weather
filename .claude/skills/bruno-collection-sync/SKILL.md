---
name: bruno-collection-sync
description: Keeps the Bruno API collection in bruno/ in lockstep with the controllers in weather-backend/Controllers/. Use whenever an HTTP endpoint is added, renamed, removed, or has its route, verb, parameters, or request body changed - and whenever asked to audit, update, refresh, or check the Bruno collection or API request collection.
---

# Bruno collection sync

The Bruno collection at `bruno/` is the hand-maintained, executable catalogue of
every HTTP endpoint this API exposes. It has to be updated in the same change
that touches an endpoint, otherwise it silently rots.

This skill audits the collection against the controllers and fixes the drift.

## When this runs

Run it whenever any of these happen:

- A new action method or controller is added.
- A `[Route]`, `[HttpGet]`/`[HttpPost]`/etc. attribute changes.
- A query parameter, route parameter, or request-body DTO is added, removed, or
  renamed.
- A response shape changes in a way the collection asserts on.
- Someone asks to audit or refresh the collection.

## Step 1 - Build the endpoint inventory from source

Enumerate every routable action. Do not trust the existing collection as the
starting point; derive the truth from code each time.

```bash
rg -n "Http(Get|Post|Put|Patch|Delete)|\[Route\(" weather-backend/Controllers --glob '!*.cs.bak'
```

For each controller, resolve the full path by combining:

- The class-level `[Route(...)]`. `[Route("[controller]")]` expands to the class
  name minus the `Controller` suffix (`LanguageTranslatorController` →
  `LanguageTranslator`), and Bruno must use that exact casing.
- The method-level `[Route(...)]`. **A leading `/` makes it absolute** and
  discards the class-level prefix - `WeatherForecastController` uses this for
  every `/weather/*` route, which is why none of them sit under
  `/WeatherForecast`.

Also capture, per action:

- HTTP verb.
- `[FromQuery]` parameters, their defaults, and whether `[Required]`.
- `[FromRoute]` parameters.
- `[FromBody]` DTO shape (find the record/class in `RequestEntities/` or `Dto/`).
- Declared `[ProducesResponseType]` status codes, which tell you which negative
  cases are worth a request of their own.

Skip anything fully commented out (`KafkaController` currently is) - but leave a
note in the folder docs if a whole controller is disabled.

Endpoints registered outside controllers count too. `Startup.Configure` maps
`/health` via `endpoints.MapHealthChecks("/health")`; check for new
`Map*` calls there as well.

## Step 2 - Diff against the collection

```bash
rg -n "^\s*(get|post|put|patch|delete|head|options):|url:" bruno --glob '*.bru'
```

Produce three lists:

1. **Missing** - in the controllers, absent from `bruno/`.
2. **Stale** - in `bruno/`, no longer in the controllers (route renamed or action
   deleted).
3. **Drifted** - present in both, but the verb, path, params, or body no longer
   match.

Report all three before changing anything.

## Step 3 - Apply the changes

### Adding a request

Place it in the folder that matches its controller. If no folder fits, create
one with a `folder.bru` whose `seq` is one higher than the current maximum.

Filenames are kebab-case and describe the scenario, not the route
(`validate-phone-number-invalid.bru`, not `phone-2.bru`).

Template - keep the block order (`meta`, verb, `params:*`, `headers`, `body:*`,
`assert`, `vars:*`, `tests`, `docs`):

```
meta {
  name: Human Readable Name
  type: http
  seq: <next free seq in this folder>
}

get {
  url: {{baseUrl}}/some/path/:pathParam?queryParam=value
  body: none
  auth: inherit
}

params:query {
  queryParam: value
}

params:path {
  pathParam: value
}

assert {
  res.status: eq 200
}

docs {
  `GET /some/path/{pathParam}`

  What it does, in one or two sentences.

  | Param        | Required | Default | Notes |
  | ------------ | -------- | ------- | ----- |
  | `queryParam` | yes      | -       | ...   |
}
```

For a request with a JSON body:

```
post {
  url: {{baseUrl}}/api/thing
  body: json
  auth: inherit
}

headers {
  Content-Type: application/json
}

body:json {
  {
    "field": "value"
  }
}
```

### Removing a request

Delete the `.bru` file. If the folder is now empty, delete the folder and
renumber the remaining `folder.bru` `seq` values so they stay contiguous.

### Updating a drifted request

Change the `url`, `params:*`, and `body:*` blocks together - Bruno keeps the
query string in the `url` and the `params:query` block in sync, so an edit to
one without the other shows up as a phantom diff the next time anyone opens the
GUI.

## Conventions this collection follows

Match these; do not invent new ones.

- **`{{baseUrl}}` only.** Never hardcode a host. Environments live in
  `bruno/environments/` (`Local`, `Local HTTPS`, `Docker`).
- **`auth: inherit`** on every request; the collection sets `auth { mode: none }`.
- **Path params use `:name`** in the URL plus a `params:path` block, not an
  inlined literal.
- **Assert only what is deterministic.** Add `assert { res.status: eq 200 }` when
  the endpoint reliably returns 200 with no external dependency. Omit the assert
  and say why in `docs` when the result depends on a feature flag, a race, an
  unseeded data store, or the caller's IP - see `Geolocation/get-location.bru`,
  `Prime Number/check-prime-number.bru`, and `Data Store/get-song-title.bru`.
- **Cover documented failure modes.** If the action has a
  `[ProducesResponseType(400)]` branch that is easy to trigger, add a sibling
  request asserting it, named `... - Invalid (400)` or `... - Over Limit (400)`.
- **Chain with variables rather than copy-paste.** Capture with
  `vars:post-response { name: res.body.field }` and consume as `{{name}}`. See
  the `Encryption` and `Text Utilities` folders.
- **Every request and folder gets a `docs` block.** Include the raw route, a
  parameter table, and any prerequisite (external service, ordering, feature
  flag). Call out side effects explicitly - `Legacy Endpoint` sends a real email.

## Step 4 - Verify

There is no offline linter, so check by hand:

- Every `.bru` file has a `meta` block with a `seq` that is unique in its folder.
- Multiline blocks (`docs`, `tests`, `body:json`, `script:*`) never contain a
  line that starts with `}` at column 0 - that terminates the block early. Indent
  inner closing braces by at least two spaces.
- Query params appear in both the `url` and `params:query`.

Then confirm the count lines up:

```bash
find bruno -name '*.bru' ! -name 'folder.bru' ! -name 'collection.bru' -not -path '*environments*' | wc -l
```

At the time of writing that is **51 requests across 17 folders**, covering all
29 routable endpoints (some endpoints have a second request for a negative case).

A run against a live API is the real check. Node is managed by fnm, so activate
it first or `npx` will not be found (see CLAUDE.md):

```bash
eval "$(fnm env --shell bash)" && fnm use default && npx --yes @usebruno/cli run bruno --env Local
```

The CLI parses every `.bru` file before issuing any request, so even with the
API stopped a clean parse confirms the syntax - parse errors look different from
connection errors.

Expect failures for folders whose backing services (DynamoDB, Redis, ConfigCat,
OpenWeatherMap) are not configured locally - that is not collection drift.

## Related

- `add-api-endpoint` invokes this skill as its final step.
- `api.yaml` at the repo root is the generated OpenAPI spec and is a useful
  cross-check on route shapes - regenerate it with the `openapi-docs` skill.
