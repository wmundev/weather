# Adapted from https://github.com/dotnet/dotnet-docker/blob/master/samples/aspnetapp/Dockerfile
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
#COPY *.sln .
#COPY /*.csproj ./aspnetapp/
#WORKDIR /source/aspnetapp
#RUN dotnet restore

# copy everything else and build app
WORKDIR /source
COPY /. ./aspnetapp/
WORKDIR /source/aspnetapp
RUN dotnet publish -c release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "weather-backend.dll"]