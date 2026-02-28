FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EnergyWorker/EnergyWorker.csproj", "EnergyWorker/"]
COPY ["DatabaseInterface/DatabaseInterface.csproj", "DatabaseInterface/"]
RUN dotnet restore "EnergyWorker/EnergyWorker.csproj"
COPY . .
WORKDIR "/src/EnergyWorker"
RUN dotnet build "EnergyWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build -r linux-x64

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EnergyWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true --os linux --arch x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["/app/EnergyWorker"]
