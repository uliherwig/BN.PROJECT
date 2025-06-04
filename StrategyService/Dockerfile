FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5101
EXPOSE 7091

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopieren Sie die Projektdateien und Wiederherstellen der Abhängigkeiten
COPY ["StrategyService/StrategyService.csproj", "StrategyService/"]
COPY ["Core/CORE.csproj", "Core/"]
RUN dotnet restore "StrategyService/StrategyService.csproj"

# Kopieren Sie den Rest des Codes und bauen Sie das Projekt
COPY . .
WORKDIR "/src/StrategyService"
RUN dotnet build "StrategyService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StrategyService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definieren Sie Umgebungsvariablen
ENV ASPNETCORE_URLS=http://+:5101

ENTRYPOINT ["dotnet", "StrategyService.dll"]
