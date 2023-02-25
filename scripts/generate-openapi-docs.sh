#!/bin/bash

(
  cd ../
  # Install swagger tool  
  dotnet tool install -g --version 6.5.0 Swashbuckle.AspNetCore.Cli
    
  # Build app for release
  dotnet publish -c Release -o out weather-backend/weather-backend.csproj
  
  swagger tofile --output api.yaml --yaml out/weather-backend.dll v1
)
