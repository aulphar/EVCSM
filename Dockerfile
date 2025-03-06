FROM node:18 AS frontend-build
WORKDIR /src
COPY FRONTEND/package*.json ./Frontend/
WORKDIR /src/FRONTEND
RUN npm install
COPY FRONTEND/ .
RUN npm run build




FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5274
EXPOSE 3000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EVCSMBackend/EVCSMBackend.csproj", "EVCSMBackend/"]
RUN dotnet restore "EVCSMBackend/EVCSMBackend.csproj"
COPY EVCSMBackend .
WORKDIR "/src/EVCSMBackend"
RUN dotnet build "EVCSMBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EVCSMBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apt-get update && apt-get install -y nodejs npm
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=frontend-build /src/FRONTEND/build .
ENTRYPOINT ["dotnet", "EVCSMBackend.dll"]