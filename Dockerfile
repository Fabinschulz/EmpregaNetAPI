FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=C.UTF-8
ENV TZ=America/Sao_Paulo

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os arquivos de solução e os csproj de todas as camadas.
COPY src/ ./src/
COPY EmpregaNet.sln .

RUN dotnet restore EmpregaNet.sln

COPY src ./src/
RUN dotnet build EmpregaNet.sln -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish-bff
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/EmpregaNet.BFF
# Publica o projeto BFF no diretório /app/bff (ignora o AppHost)
RUN dotnet publish EmpregaNet.BFF.csproj -c $BUILD_CONFIGURATION -o /app/bff /p:UseAppHost=false

FROM build AS publish-api
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/EmpregaNet.Api
RUN dotnet publish EmpregaNet.Api.csproj -c $BUILD_CONFIGURATION -o /app/api /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish-bff /app/bff /app/bff
COPY --from=publish-api /app/api /app/api
