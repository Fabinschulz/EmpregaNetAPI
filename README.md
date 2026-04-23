# EmpregaNet — monorepo (backend, BFF e frontend)

![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-green)
![License](https://img.shields.io/badge/License-MIT-orange)

## Visão geral

Repositório **monolítico**: um único Git com pastas por camada na raiz.

| Pasta | Responsabilidade |
|--------|-------------------|
| **`backend/`** | Stack .NET da API: `EmpregaNet.sln`, `Dockerfile`, `docker-compose.yml`, `.gitignore` e `.dockerignore` do core, mais `src/` (Domain, Application, Infra, Api) e `tests/`. |
| **`bff/`** | BFF em .NET: `EmpregaNet.Bff.sln`, `Dockerfile` e projetos `Bff.*`. |
| **`frontend/`** | Reservado para a aplicação cliente em react. |

### Estrutura do projeto

```
backend/
  EmpregaNet.sln
  Dockerfile
  docker-compose.yml
  .dockerignore
  .gitignore
  .env    
  src/   … Domain, Application, Infra, Api
  tests/
bff/
  EmpregaNet.Bff.sln
  Dockerfile
  .dockerignore
  .env      
  Bff.Core/
  Bff.Infrastructure/
  Bff.WebApi/
frontend/
  README.md
  .env   
.gitignore          
```

### Fluxo de dependência (API)

`Api` e `Infra` referenciam `Application`; `Application` referencia `Domain`. O BFF comunica com a API por HTTP.

---

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL quando correr a API fora do Docker
- Docker e Docker Compose (stack com Postgres, Redis, API e BFF)

---

## Variáveis de ambiente (um `.env` por aplicação)

Cada aplicação mantém o **seu** `.env` na própria pasta (não há `.env` único na raiz do monorepo):

| Aplicação | Ficheiro |
|-----------|----------|
| API + stack Docker (Postgres, Redis, portas da API) | `backend/.env` |
| BFF | `bff/.env` |
| Frontend | `frontend/.env` |

---
### Docker Compose (pasta `backend/`)

A interpolação `${BFF_PORT}` e outras variáveis do BFF vêm do `.env` do BFF. Execute a partir de `backend/` passando **os dois** ficheiros:

```bash
cd backend
docker compose --env-file .env --env-file ../bff/.env up --build -d
```

O serviço **api** carrega também `backend/.env` no contentor (`env_file`). O **bff** carrega `bff/.env` no contentor.

---

## Executar a API e o BFF (desenvolvimento)

| Alvo | Comando |
|--------|---------|
| **Abrir / compilar API** | `dotnet build backend/EmpregaNet.sln` |
| **API** | `dotnet run --project backend/src/EmpregaNet.Api/EmpregaNet.Api.csproj` |
| **BFF** | `dotnet run --project bff/Bff.WebApi/Bff.WebApi.csproj` |
| **BFF (via solução)** | `dotnet build bff/EmpregaNet.Bff.sln` |

---

## Migrações (EF Core)

```bash
cd backend/src/EmpregaNet.Infra
dotnet ef migrations add NomeDaMigracao --context PostgreSqlContext --output-dir Persistence/Migrations
dotnet ef database update
```

---

## Docker Compose (API + BFF + infra)

O ficheiro de orquestração está em **`backend/docker-compose.yml`**. O serviço `api` usa o contexto `backend/`; o `bff` usa `../bff`.

```bash
cd backend
docker compose --env-file .env --env-file ../bff/.env up --build -d
```

Comandos úteis (a partir de `backend/`):

```bash
docker compose ps
docker compose down
```

### Imagem só da API (CI / ECR)

A partir da **raiz** do repositório:

```bash
docker build -f backend/Dockerfile -t empreganet-api ./backend
```

### Testes

A partir da raiz:

```bash
dotnet test backend/tests/tests.csproj
```

---

## Licença

Distribuído sob licença MIT, quando aplicável.
