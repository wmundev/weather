# Build from the repository root - weather-backend references weather-application, weather-repository and
# weather-domain, and package versions live in the root Directory.Packages.props:
#   docker build -f weather-backend/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Restore as its own layer. Only the files restore reads are copied first, so a source edit reuses the
# cached packages instead of downloading them again.
COPY Directory.Build.props Directory.Packages.props ./
COPY weather-domain/weather-domain.csproj weather-domain/
COPY weather-application/weather-application.csproj weather-application/
COPY weather-repository/weather-repository.csproj weather-repository/
COPY weather-backend/weather-backend.csproj weather-backend/
RUN dotnet restore weather-backend/weather-backend.csproj

COPY weather-domain/ weather-domain/
COPY weather-application/ weather-application/
COPY weather-repository/ weather-repository/
COPY weather-backend/ weather-backend/
RUN dotnet publish weather-backend/weather-backend.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
ENTRYPOINT ["dotnet", "weather-backend.dll"]
