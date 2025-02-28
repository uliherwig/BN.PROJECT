# Unter https://aka.ms/customizecontainer erfahren Sie, wie Sie Ihren Debugcontainer anpassen und wie Visual Studio dieses Dockerfile verwendet, um Ihre Images für ein schnelleres Debuggen zu erstellen.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

# Create a non-root user and group
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser
ENV ASPNETCORE_ENVIRONMENT=Development
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TestService/TestService.csproj", "TestService/"]
RUN dotnet restore "./TestService/TestService.csproj"
COPY . .
WORKDIR "/src/TestService"
RUN dotnet build "./TestService.csproj"  -c Release -o /app/build

# Diese Stufe wird verwendet, um das Dienstprojekt zu veröffentlichen, das in die letzte Phase kopiert werden soll.
FROM build AS publish
RUN dotnet publish "./TestService.csproj" -c Release -o /app/publish

# Diese Stufe wird in der Produktion oder bei Ausführung von VS im regulären Modus verwendet (Standard, wenn die Debugkonfiguration nicht verwendet wird).
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Set ownership of the /app directory to the non-root user
RUN chown -R appuser:appgroup /app

# Switch to the non-root user
USER appuser

EXPOSE 5193
ENV ASPNETCORE_URLS=http://+:5193

ENTRYPOINT ["dotnet", "TestService.dll"]