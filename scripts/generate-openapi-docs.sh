#!/bin/sh

(
  cd ../
  # Install swagger tool  
  dotnet tool install -g --version 6.5.0 Swashbuckle.AspNetCore.Cli
    
  # Build app for release
  dotnet publish -c Release -o out weather-backend/weather-backend.csproj
  
  # Run from inside out/ so the host's content root is the publish directory. The CLI boots the real
  # Startup, which throws when ConfigCat:Key is missing - and the appsettings files that carry it only
  # resolve when the content root is where they were published to, not the repo root.
  cd out
  swagger tofile --output ../api.yaml --yaml weather-backend.dll v1
)
