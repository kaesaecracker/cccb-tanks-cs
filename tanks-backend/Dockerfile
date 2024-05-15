FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-server

RUN apk add clang binutils musl-dev build-base zlib-static cmake openssl-dev openssl-libs-static openssl
RUN apk add rust cargo

WORKDIR /src/tanks-server

COPY . .
RUN dotnet build TanksServer/TanksServer.csproj -c Release -r linux-musl-x64 -o /build
RUN dotnet publish TanksServer/TanksServer.csproj -c Release -r linux-musl-x64 -o /app

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app

COPY --from=build-server /app .

EXPOSE 80
ENTRYPOINT ./TanksServer
