﻿# Adapted from https://github.com/dotnet/dotnet-docker/blob/master/samples/aspnetapp/Dockerfile
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
#COPY *.sln .
COPY /*.csproj ./aspnetapp/
RUN cd aspnetapp && dotnet restore

# copy everything else and build app
COPY /. ./aspnetapp/
WORKDIR /source/aspnetapp
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "weather-backend.dll"]