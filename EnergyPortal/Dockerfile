FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-arm64v8 AS base

USER root
# Create the /var/dpkeys directory
RUN mkdir -p /var/dpkeys
# Set permissions for the /var/dpkeys directory
RUN chown -R $APP_UID /var/dpkeys

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0-noble-arm64v8 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EnergyPortal/EnergyPortal.csproj", "EnergyPortal/"]
COPY ["DatabaseInterface/DatabaseInterface.csproj", "DatabaseInterface/"]
RUN dotnet restore "EnergyPortal/EnergyPortal.csproj"
COPY . .
WORKDIR "/src/EnergyPortal"

# Install Node.js and npm
RUN apt-get update && apt-get install curl
RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash
RUN source ~/.bashrc
RUN nvm install 22
RUN nvm use 22

# Run npm install and npm run build
RUN npm install
RUN npm run build

RUN dotnet build "EnergyPortal.csproj" -c $BUILD_CONFIGURATION -o /app/build -r linux-arm64

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EnergyPortal.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true --os linux --arch arm64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#ENTRYPOINT ["tail", "-f", "/dev/null"]
ENTRYPOINT ["/app/EnergyPortal"]
