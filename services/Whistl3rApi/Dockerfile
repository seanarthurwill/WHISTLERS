# Stage 1: Base runtime image (smallest, production-ready)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build environment (has SDK for compiling)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies (this layer gets cached!)
COPY ["Whistl3rApi.csproj", "./"]
RUN dotnet restore "Whistl3rApi.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "Whistl3rApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish (create deployment-ready files)
FROM build AS publish
RUN dotnet publish "Whistl3rApi.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 4: Final runtime image (copy only what's needed to run)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Tell ASP.NET Core which port to use
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Whistl3rApi.dll"]