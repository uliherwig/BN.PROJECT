FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5130
EXPOSE 7095

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopieren Sie die Projektdateien und Wiederherstellen der Abhängigkeiten
COPY ["AlpacaService/AlpacaService.csproj", "AlpacaService/"]
COPY ["Core/CORE.csproj", "Core/"]
RUN dotnet restore "AlpacaService/AlpacaService.csproj"

# Kopieren Sie den Rest des Codes und bauen Sie das Projekt
COPY . .
WORKDIR "/src/AlpacaService"
RUN dotnet build "AlpacaService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlpacaService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definieren Sie Umgebungsvariablen
ENV ASPNETCORE_URLS=http://+:5130

ENTRYPOINT ["dotnet", "AlpacaService.dll"]
