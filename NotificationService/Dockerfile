FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5254
EXPOSE 7220

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

# Definieren Sie Umgebungsvariablen
ENV ASPNETCORE_URLS=http://+:5254

ENTRYPOINT ["dotnet", "NotificationService.dll"]
