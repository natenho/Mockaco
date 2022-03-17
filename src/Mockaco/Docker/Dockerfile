FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
COPY ./src/Mockaco/Mockaco.csproj /src/Mockaco/
WORKDIR /src/Mockaco
RUN dotnet restore
WORKDIR /repo
COPY ./ ./
WORKDIR /repo/src/Mockaco
RUN dotnet build "Mockaco.csproj" -c Release -o /app/build
RUN find -iname gitversion.json -exec cat {} \;

FROM build AS publish
RUN dotnet publish "Mockaco.csproj" -c Release -o /app/publish

FROM base AS final
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_ENVIRONMENT=Docker
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./src/Mockaco/Mocks/hello.json /app/Mocks/
COPY ./src/Mockaco/Settings /app/Settings
VOLUME /app/Mocks
VOLUME /app/Settings
ENTRYPOINT ["dotnet", "Mockaco.dll"]