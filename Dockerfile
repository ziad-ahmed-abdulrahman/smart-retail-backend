ARG DOTNET_VERSION=10.0

# 1. Base Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files separately for optimal NuGet layer caching
COPY ["SmartRetail.Domain/SmartRetail.Domain.csproj", "SmartRetail.Domain/"]
COPY ["SmartRetail.Application/SmartRetail.Application.csproj", "SmartRetail.Application/"]
COPY ["SmartRetail.Infrastructure/SmartRetail.Infrastructure.csproj", "SmartRetail.Infrastructure/"]
COPY ["SmartRetail.Api/SmartRetail.Api.csproj", "SmartRetail.Api/"]

RUN dotnet restore "SmartRetail.Api/SmartRetail.Api.csproj"

# Copy the rest of the source files
COPY . .

WORKDIR "/src/SmartRetail.Api"
RUN dotnet build "SmartRetail.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Development stage with hot reload for bind-mounted source files.
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS development
WORKDIR /src
COPY ["SmartRetail.Domain/SmartRetail.Domain.csproj", "SmartRetail.Domain/"]
COPY ["SmartRetail.Application/SmartRetail.Application.csproj", "SmartRetail.Application/"]
COPY ["SmartRetail.Infrastructure/SmartRetail.Infrastructure.csproj", "SmartRetail.Infrastructure/"]
COPY ["SmartRetail.Api/SmartRetail.Api.csproj", "SmartRetail.Api/"]
RUN dotnet restore "SmartRetail.Api/SmartRetail.Api.csproj"
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
EXPOSE 8080
ENTRYPOINT ["dotnet", "watch", "--project", "SmartRetail.Api/SmartRetail.Api.csproj", "run", "--no-launch-profile", "--urls", "http://+:8080"]

# 3. Publish Stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SmartRetail.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. Final Production Runtime Stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartRetail.Api.dll"]
