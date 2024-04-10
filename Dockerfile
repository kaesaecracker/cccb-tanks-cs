FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-server

RUN apk add clang binutils musl-dev build-base zlib-static cmake openssl-dev openssl-libs-static openssl

WORKDIR /src/TanksServer

# dependencies
COPY TanksServer/TanksServer.csproj .
RUN dotnet restore --runtime linux-musl-x64 TanksServer.csproj

#build
COPY TanksServer .
RUN dotnet build TanksServer.csproj -c Release -o /app/build
RUN dotnet publish TanksServer.csproj -r linux-musl-x64 -c Release -o /app/publish


FROM node:21-alpine AS build-client
WORKDIR /app

# dependencies
COPY tank-frontend/package.json .
COPY tank-frontend/package-lock.json .
RUN npm i

# build
env CONTAINERMODE 1
COPY tank-frontend .
RUN npm run build


FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app

COPY --from=build-server /app/publish .
COPY --from=build-client /app/dist ./client

EXPOSE 80
ENTRYPOINT ./TanksServer
