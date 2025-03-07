FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj and restore dependencies
COPY ./EVCSMBackend.csproj ./
RUN dotnet restore EVCSMBackend.csproj

# Copy everything else
COPY . ./
RUN dotnet publish EVCSMBackend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EVCSMBackend.dll"]
