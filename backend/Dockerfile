# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

RUN apk add --no-cache icu-libs tzdata curl

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=C.UTF-8 \
    TZ=America/Sao_Paulo \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=1

WORKDIR /app

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=25s --retries=3 \
    CMD curl -fsS http://127.0.0.1:8080/health/live || exit 1

USER $APP_UID
ENTRYPOINT ["dotnet", "EmpregaNet.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY EmpregaNet.sln ./
COPY src/EmpregaNet.AI/*.csproj          src/EmpregaNet.AI/
COPY src/EmpregaNet.Api/*.csproj         src/EmpregaNet.Api/
COPY src/EmpregaNet.Application/*.csproj src/EmpregaNet.Application/
COPY src/EmpregaNet.Domain/*.csproj      src/EmpregaNet.Domain/
COPY src/EmpregaNet.Infra/*.csproj       src/EmpregaNet.Infra/
COPY tests/*.csproj                      tests/
RUN dotnet restore "EmpregaNet.sln"
COPY src/ src/
RUN dotnet publish "src/EmpregaNet.Api/EmpregaNet.Api.csproj" \
      -c $BUILD_CONFIGURATION \
      -o /publish \
      /p:UseAppHost=false


FROM runtime AS local
COPY --from=build --chown=app:app /publish ./

FROM runtime AS prebuilt
COPY --chown=app:app publish/ ./
