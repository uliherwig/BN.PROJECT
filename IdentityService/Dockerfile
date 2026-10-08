# Use the Alpine-based ASP.NET runtime image for a smaller final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app

# Create a non-root user and group
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY ["IdentityService/IdentityService.csproj", "IdentityService/"]
COPY ["Core/CORE.csproj", "Core/"]
RUN dotnet restore "IdentityService/IdentityService.csproj"

# Copy the rest of the code and build the project
COPY . .
WORKDIR "/src/IdentityService"
RUN dotnet build "IdentityService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set ownership of the /app directory to the non-root user
RUN chown -R appuser:appgroup /app

# Switch to the non-root user
USER appuser

EXPOSE 5044
EXPOSE 7010

# Define environment variables (http + https, wie in launchSettings.json)
ENV ASPNETCORE_URLS=http://+:5044;https://+:7010
# HTTPS-Zertifikat zur Laufzeit per Volume + Env-Variablen bereitstellen, z.B.:
#   docker run -v "%USERPROFILE%\.aspnet\https:/https/" ^
#     -e ASPNETCORE_Kestrel__Certificates__Default__Password=<password> ^
#     -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx ...
# Zertifikat zuvor lokal exportieren: dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p <password>

ENTRYPOINT ["dotnet", "IdentityService.dll"]