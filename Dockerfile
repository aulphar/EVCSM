FROM node:18 AS frontend-build
WORKDIR /src
COPY FRONTEND/package*.json ./FRONTEND/
WORKDIR /src/FRONTEND
RUN npm install
COPY FRONTEND/ .
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY EVCSMBackend/EVCSMBackend.csproj EVCSMBackend/
RUN dotnet restore EVCSMBackend/EVCSMBackend.csproj
COPY EVCSMBackend/ .
WORKDIR /src/EVCSMBackend
RUN dotnet publish EVCSMBackend.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /src/Frontend/build ./wwwroot
EXPOSE 5274
EXPOSE 3000
ENTRYPOINT ["dotnet", "EVCSMBackend.dll"]
