FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build

COPY ./src/Mockaco/Mockaco.csproj /src/Mockaco/
COPY ./src/Mockaco.AspNetCore/Mockaco.AspNetCore.csproj /src/Mockaco.AspNetCore/

WORKDIR /src/Mockaco
RUN dotnet restore

WORKDIR /repo
COPY ./ ./

WORKDIR /repo/src/Mockaco
RUN dotnet build "Mockaco.csproj" -c Release -o /app/build
RUN find -iname gitversion.json -exec cat {} \;
RUN dotnet dev-certs https

FROM build AS publish
RUN dotnet publish "Mockaco.csproj" -c Release -o /app/publish

FROM base AS final
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_ENVIRONMENT=Docker
WORKDIR /app

COPY --from=publish /app/publish .
COPY ./src/Mockaco/Mocks/hello.json ./Mocks/
COPY ./src/Mockaco/Settings ./Settings

COPY --from=build /root/.dotnet/corefx/cryptography/x509stores/my /root/.dotnet/corefx/cryptography/x509stores/my

VOLUME /app/Mocks
VOLUME /app/Settings
VOLUME /root/.dotnet/corefx/cryptography/x509stores/my

ENTRYPOINT ["dotnet", "Mockaco.dll"]