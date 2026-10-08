FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5101
EXPOSE 7020

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopieren Sie die Projektdateien und Wiederherstellen der Abh�ngigkeiten
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

# Definieren Sie Umgebungsvariablen (http + https, wie in launchSettings.json)
ENV ASPNETCORE_URLS=http://+:5101;https://+:7020
# HTTPS-Zertifikat zur Laufzeit per Volume + Env-Variablen bereitstellen, z.B.:
#   docker run -v "%USERPROFILE%\.aspnet\https:/https/" ^
#     -e ASPNETCORE_Kestrel__Certificates__Default__Password=<password> ^
#     -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx ...
# Zertifikat zuvor lokal exportieren: dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p <password>

ENTRYPOINT ["dotnet", "StrategyService.dll"]
