FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-server

RUN apk add clang binutils musl-dev build-base zlib-static cmake openssl-dev openssl-libs-static openssl

WORKDIR /src/tanks-server

# dependencies
COPY ./shared.props .
COPY ./TanksServer.sln .
COPY ./EndiannessSourceGenerator/EndiannessSourceGenerator.csproj EndiannessSourceGenerator/EndiannessSourceGenerator.csproj
COPY ./DisplayCommands/DisplayCommands.csproj DisplayCommands/DisplayCommands.csproj
COPY ./TanksServer/TanksServer.csproj TanksServer/TanksServer.csproj
RUN dotnet restore --runtime linux-musl-x64 TanksServer.sln

#build
COPY . .
RUN dotnet build TanksServer/TanksServer.csproj -c Release -r linux-musl-x64 -o /build
RUN dotnet publish TanksServer/TanksServer.csproj -c Release -r linux-musl-x64 -o /app

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app

COPY --from=build-server /app .

EXPOSE 80
ENTRYPOINT ./TanksServer
