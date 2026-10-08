FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5254
EXPOSE 7030

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopieren Sie die Projektdateien und Wiederherstellen der Abhängigkeiten
COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["Core/CORE.csproj", "Core/"]
RUN dotnet restore "NotificationService/NotificationService.csproj"

# Kopieren Sie den Rest des Codes und bauen Sie das Projekt
COPY . .
WORKDIR "/src/NotificationService"
RUN dotnet build "NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definieren Sie Umgebungsvariablen (http + https, wie in launchSettings.json)
ENV ASPNETCORE_URLS=http://+:5254;https://+:7030
# HTTPS-Zertifikat zur Laufzeit per Volume + Env-Variablen bereitstellen, z.B.:
#   docker run -v "%USERPROFILE%\.aspnet\https:/https/" ^
#     -e ASPNETCORE_Kestrel__Certificates__Default__Password=<password> ^
#     -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx ...
# Zertifikat zuvor lokal exportieren: dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p <password>

ENTRYPOINT ["dotnet", "NotificationService.dll"]
