# CLAUDE.md

.NET 10 weather API (`weather-backend`) wrapping OpenWeatherMap with a DynamoDB
response cache, plus utility endpoints, a CLI, and Terraform infra. Deployed to
AWS ECS via `aws-dotnet-deploy`.

## Layout

| Path                            | What lives there                                    |
| ------------------------------- | --------------------------------------------------- |
| `weather-backend/`              | ASP.NET Core API - controllers, services, hosted services |
| `weather-application/`          | Application services (`EncryptionService`)          |
| `weather-repository/`           | Data access (`CityRepository`)                      |
| `weather-domain/`               | Entities and contracts shared across projects       |
| `Weather.CLI/`                  | Console app - algorithm scratchpad                  |
| `weather-test/`                 | API unit tests                                      |
| `Weather.API.IntegrationTests/` | API integration tests                               |
| `Weather.CLI.UnitTests/`        | CLI unit tests                                      |
| `weather-infra/`                | Terraform - DynamoDB, IAM, Parameter Store          |
| `weather-backend.Deployment/`   | Generated `aws-dotnet-deploy` CDK project           |
| `bruno/`                        | Bruno API collection - the endpoint catalogue       |
| `database/`                     | Local DynamoDB / SQL Server / Postgres containers   |

`weather-backend` → `weather-application` + `weather-repository` →
`weather-domain`. Domain never depends on the API.

## Skills

In `.claude/skills/`. They carry conventions that are not obvious from the code -
prefer them over improvising.

| Skill                   | Use when                                                      |
| ----------------------- | ------------------------------------------------------------- |
| `bruno-collection-sync` | **Any** endpoint is added, renamed, removed, or has its route, verb, params, or body changed |
| `add-api-endpoint`      | Adding a controller or action - the full checklist            |
| `dotnet-conventions`    | Writing or modifying any C# here                              |
| `dotnet-test`           | Running tests or checking coverage                            |
| `openapi-docs`          | Regenerating `api.yaml`                                       |

## Endpoint changes require the Bruno sync skill

`bruno/` holds one request per HTTP endpoint plus negative cases for documented
failure branches. It is committed and expected to stay correct.

**Any change to a route, HTTP verb, query/route parameter, or request body must
invoke `bruno-collection-sync` in the same change.** Do not hand-write `.bru`
files from memory. This covers endpoints registered outside controllers too, such
as `MapHealthChecks("/health")` in `Startup.Configure`.

The same change should regenerate `api.yaml` - the `pre-commit` hook does this
automatically once hooks are installed (`git config core.hooksPath .githooks`).

## Commands

Run the API - `http://localhost:5000` / `https://localhost:5001`, Swagger at
`/swagger` in Development:

```bash
dotnet run --project weather-backend/weather-backend.csproj
```

```bash
dotnet build --configuration Release
```

Tests and coverage: see the `dotnet-test` skill.

## Node is installed via fnm

Node comes from [fnm](https://github.com/Schniz/fnm), which puts it on `PATH`
through a per-shell hook - so `node --version` fails in a non-interactive shell
even though Node is installed. Activate it first (`default` is Node 22):

```bash
eval "$(fnm env --shell bash)" && fnm use default
```

Nothing in the .NET build, test, or deploy path needs Node. It is only used for
tooling such as the Bruno CLI.

## Never hand-edit

- `api.yaml` - generated; regenerate with the `openapi-docs` skill
- `weather-backend/Assets/Generated` - AWS Translate output
- `weather-backend.Deployment/cdk.out`, `out/`, `coverage/`, `bin/`, `obj/`

## Gotchas

- `GET /weather` is **legacy**: uncached, and **sends a real email** on every
  call. Never use it for smoke or load testing.
- `GET /api/prime-number` is gated behind the `primenumber` ConfigCat flag and
  returns `400 "Not enabled"` when off.
- `POST /api/LanguageTranslator/generate` is a deliberate no-op - the AWS
  Translate call is commented out so test runs do not spend credits.
- `GET /api/music/song-title` cancels its own DynamoDB load, so its status code
  is a race by design.
- `KafkaController` is entirely commented out.
