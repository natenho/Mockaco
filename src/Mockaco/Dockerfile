FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ./Mockaco.csproj .
RUN dotnet restore
COPY . .
RUN dotnet build "Mockaco.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Mockaco.csproj" -c Release -o /app

FROM base AS final
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
WORKDIR /app
COPY --from=publish /app .
COPY Mocks /app/Mocks
COPY Settings /app/Settings
VOLUME /app/Mocks
VOLUME /app/Settings
ENTRYPOINT ["dotnet", "Mockaco.dll"]