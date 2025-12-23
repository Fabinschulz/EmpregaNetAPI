FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=C.UTF-8
ENV TZ=America/Sao_Paulo

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["EmpregaNet.sln", "./"]
COPY Bff/ ./Bff/
COPY src/ ./src/
COPY tests/ ./tests/

RUN dotnet restore "EmpregaNet.sln"
COPY . .

FROM build AS publish-bff
RUN dotnet publish "Bff/Bff.WebApi/Bff.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish/bff /p:UseAppHost=false

FROM build AS publish-api
RUN dotnet publish "src/EmpregaNet.Api/EmpregaNet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish/api /p:UseAppHost=false

FROM base AS bff
WORKDIR /app
COPY --from=publish-bff /app/publish/bff .
ENTRYPOINT ["dotnet", "Bff.WebApi.dll"]


FROM base AS api
WORKDIR /app
COPY --from=publish-api /app/publish/api .
ENTRYPOINT ["dotnet", "EmpregaNet.Api.dll"]