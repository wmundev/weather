---
name: openapi-docs
description: Regenerates the api.yaml OpenAPI spec from the built weather-backend assembly using the Swashbuckle CLI. Use when api.yaml is stale, after changing a route, parameter, response type, or XML doc comment, or when asked to regenerate, update, or check the OpenAPI/Swagger spec.
---

# Regenerating api.yaml

`api.yaml` at the repo root is **generated**, not hand-edited. It is produced by
publishing `weather-backend` and running the Swashbuckle CLI against the built
DLL, so it always reflects the routing attributes, `[ProducesResponseType]`
declarations, and XML doc comments that are actually in the code.

Never edit `api.yaml` by hand - the next generation will overwrite it, and a
hand-edit that disagrees with the code is worse than a stale file.

## Regenerate

```bash
cd scripts && ./generate-openapi-docs.sh
```

The script restores `Swashbuckle.AspNetCore.Cli` from the local tool manifest
(`.config/dotnet-tools.json`, pinned to the same version as the
`Swashbuckle.AspNetCore` package), publishes the API in Release to `out/`, and
writes `api.yaml` from the `v1` Swagger document. It exits non-zero on any
failure.

Equivalent manual steps if the script fails:

```bash
dotnet tool restore
```

```bash
dotnet publish -c Release -o out weather-backend/weather-backend.csproj
```

```bash
cd out && dotnet tool run swagger tofile --output ../api.yaml --yaml weather-backend.dll v1
```

Run the last step from inside `out/`: the CLI boots the real host, and the
appsettings carrying `ConfigCat:Key` only resolve with `out/` as content root.

When bumping `Swashbuckle.AspNetCore`, bump the CLI in the manifest to match
(`dotnet tool update swashbuckle.aspnetcore.cli --version <x>`).

`out/` is gitignored; `api.yaml` is committed.

## The git hook

`.githooks/pre-commit` already runs the script and stages `api.yaml` on every
commit, and blocks the commit if generation fails. CI also regenerates the spec
and fails if the committed `api.yaml` is stale. Enable it once per clone:

```bash
git config core.hooksPath .githooks
```

If the hook is active you rarely need to run this skill directly - check
`git status` before regenerating manually.

## When the spec looks wrong

The generator only sees what the attributes and XML comments say:

- A missing response code means the action lacks a `[ProducesResponseType]`.
- A parameter in the wrong place (body instead of query) means the binding
  attribute is missing or the `[ApiController]` convention inferred `[FromBody]`
  for a complex type.
- Missing descriptions mean missing `<summary>` / `<param>` XML comments.

Fix the controller, then regenerate. Do not patch the YAML.

## Related

`api.yaml` and the Bruno collection in `bruno/` describe the same surface from
opposite directions - the spec is generated, the collection is curated and
executable. After a route change, regenerate the spec **and** run the
`bruno-collection-sync` skill.
