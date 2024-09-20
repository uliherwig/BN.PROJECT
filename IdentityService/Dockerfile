FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5044
EXPOSE 7076

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY . /src
WORKDIR /src
COPY ["IdentityService.csproj", "/"]
RUN dotnet restore "/IdentityService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "IdentityService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish
	
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definieren Sie Umgebungsvariablen
ENV ASPNETCORE_URLS=http://+:5044

ENTRYPOINT ["dotnet", "IdentityService.dll"]
