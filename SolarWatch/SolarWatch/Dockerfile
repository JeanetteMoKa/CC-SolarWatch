# Stage 1: Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5106

# Stage 2: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY SolarWatch/SolarWatch.csproj ./
RUN dotnet restore "SolarWatch.csproj"

COPY SolarWatch/. .
RUN dotnet build "SolarWatch.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SolarWatch.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

COPY ["SolarWatch/Resources", "/app/Resources"]

# Stage 4: Migrations
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS ef-migrations
WORKDIR /src
COPY SolarWatch/. .
COPY --from=publish /app/publish /app

# Set the connection string environment variable
ENV ConnectionStrings__Default="Server=db,1433;Database=SolarWatch;User Id=sa;Password=Ant34teR;Encrypt=False;"

# Install EF tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Run migrations
RUN dotnet ef database update --context UsersContext
RUN dotnet ef database update --context SolarWatchContext


# Stage 5: Final image for runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/Resources ./Resources

ENTRYPOINT ["dotnet", "SolarWatch.dll"]
