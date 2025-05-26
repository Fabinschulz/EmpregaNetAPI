FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY EmpregaNet.sln ./
COPY src/EmpregaNet.Api/EmpregaNet.Api.csproj ./src/EmpregaNet.Api/
COPY src/EmpregaNet.Application/EmpregaNet.Application.csproj ./src/EmpregaNet.Application/
COPY src/EmpregaNet.Domain/EmpregaNet.Domain.csproj ./src/EmpregaNet.Domain/
COPY src/EmpregaNet.Infra/EmpregaNet.Infra.csproj ./src/EmpregaNet.Infra/

RUN dotnet restore EmpregaNet.sln

COPY src ./src

WORKDIR /src/src/EmpregaNet.Api
RUN dotnet build EmpregaNet.Api.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish EmpregaNet.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmpregaNet.Api.dll"]
