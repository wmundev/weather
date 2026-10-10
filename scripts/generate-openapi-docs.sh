#!/bin/sh
# Regenerates api.yaml from the published weather-backend assembly. Exits non-zero on any failure so
# the pre-commit hook cannot record a stale spec as though it were current.
set -e

cd "$(dirname "$0")/.."

# The CLI is pinned in .config/dotnet-tools.json to the same version as the Swashbuckle.AspNetCore
# package, so the generator and the runtime agree on the document shape.
dotnet tool restore

dotnet publish -c Release -o out weather-backend/weather-backend.csproj

# Run from inside out/ so the host's content root is the publish directory. The CLI boots the real
# Startup, which throws when ConfigCat:Key is missing - and the appsettings files that carry it only
# resolve when the content root is where they were published to, not the repo root.
cd out
dotnet tool run swagger tofile --output ../api.yaml --yaml weather-backend.dll v1
