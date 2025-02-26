FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5044
EXPOSE 7283

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopieren Sie die Projektdateien und Wiederherstellen der Abhängigkeiten
COPY ["IdentityService/IdentityService.csproj", "IdentityService/"]
COPY ["Core/CORE.csproj", "Core/"]
RUN dotnet restore "IdentityService/IdentityService.csproj"

# Kopieren Sie den Rest des Codes und bauen Sie das Projekt
COPY . .
WORKDIR "/src/IdentityService"
RUN dotnet build "IdentityService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definieren Sie Umgebungsvariablen
ENV ASPNETCORE_URLS=http://+:5044;https://+:7283

ENTRYPOINT ["dotnet", "IdentityService.dll"]

