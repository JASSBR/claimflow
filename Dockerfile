# syntax=docker/dockerfile:1
# API image. Build: docker build -t claimflow-api .   (add --platform linux/amd64 on Apple Silicon for cloud targets)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, from project files only: this layer stays cached until a dependency changes.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ClaimFlow.Api/ClaimFlow.Api.csproj src/ClaimFlow.Api/
COPY src/ClaimFlow.ServiceDefaults/ClaimFlow.ServiceDefaults.csproj src/ClaimFlow.ServiceDefaults/
COPY src/ClaimFlow.SharedKernel/ClaimFlow.SharedKernel.csproj src/ClaimFlow.SharedKernel/
COPY src/ClaimFlow.BuildingBlocks/ClaimFlow.BuildingBlocks.csproj src/ClaimFlow.BuildingBlocks/
COPY src/Modules/Claims/ClaimFlow.Claims.Domain/ClaimFlow.Claims.Domain.csproj src/Modules/Claims/ClaimFlow.Claims.Domain/
COPY src/Modules/Claims/ClaimFlow.Claims/ClaimFlow.Claims.csproj src/Modules/Claims/ClaimFlow.Claims/
COPY src/Modules/Documents/ClaimFlow.Documents.Domain/ClaimFlow.Documents.Domain.csproj src/Modules/Documents/ClaimFlow.Documents.Domain/
COPY src/Modules/Documents/ClaimFlow.Documents/ClaimFlow.Documents.csproj src/Modules/Documents/ClaimFlow.Documents/
RUN dotnet restore src/ClaimFlow.Api/ClaimFlow.Api.csproj

COPY src/ src/
RUN dotnet publish src/ClaimFlow.Api/ClaimFlow.Api.csproj -c Release -o /app --no-restore

# Chiseled runtime: no shell, no package manager, non-root by default — a minimal attack surface.
# The "-extra" variant adds ICU and time zones: the app formats French amounts and dates (fr-FR culture),
# which the plain chiseled image (globalization-invariant mode) cannot do.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClaimFlow.Api.dll"]
