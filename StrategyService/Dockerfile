#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 5100
EXPOSE 7167

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY . /src
WORKDIR /src
COPY ["DataService.csproj", "DataService/"]
RUN dotnet restore "./DataService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./DataService.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DataService.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definieren Sie Umgebungsvariablen
ENV ASPNETCORE_URLS=http://+:5100

ENTRYPOINT ["dotnet", "DataService.dll"]
