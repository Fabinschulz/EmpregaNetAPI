---
name: backend-skill
description: >-
  Implementa e evolui a API EmpregaNet em .NET com Clean Architecture, handlers IRequest/IRequestHandler,
  EF Core, validação e padrões já usados no repositório. Use para features, queries, commands,
  infraestrutura e alterações na API ou nos testes de backend.
---

# Backend (.NET — EmpregaNet API)

## Quando aplicar

Qualquer alteração em `backend/src/` (Domain, Application, Infra, Api) ou `backend/tests/`, incluindo contratos HTTP expostos ao BFF ou ao frontend.

## Alinhamento com agentes

Para **desenho de fronteiras e decisões estruturais**, alinhar com `docs/agents/dotnet-architect.md`. Para **código e refactors concretos**, alinhar com `docs/agents/dotnet-implementer.md`.

## Arquitetura do repositório

- **Domain**: regras e modelo; **sem** dependências de EF, ASP.NET ou infraestrutura.
- **Application**: casos de uso, handlers, queries/commands, interfaces para serviços externos; orquestra validação e fluxo.
- **Infra**: EF Core, repositórios, integrações, `DbContext`, migrations.
- **Api**: controladores / endpoints finos que delegam na Application.

O projeto utiliza um **mediator interno** (`IRequest` / `IRequestHandler` em `EmpregaNet.Domain.Libs.Mediator` e pipelines em Infra). **Não** assumir o pacote NuGet MediatR salvo indicação em código — seguir o padrão existente.

## Regras obrigatórias

- **EF Core**: evitar N+1; leituras com projeções (`Select`) quando possível; `AsNoTracking` em caminhos só-leitura; paginação em listagens; não carregar entidades desnecessárias.
- **Handlers**: um handler focado por comando/query; dependências por construtor com DI do ASP.NET Core.
- **Transações**: respeitar `ITransactional` e comportamentos já registados (ex.: pipelines na Infra).
- **Testes**: xUnit + FluentAssertions onde já existir; testar regras de negócio e handlers; integração para semântica de provider e HTTP quando fizer sentido.

## Checklist de entrega (feature API)

1. [ ] Comando/query e handler no lugar certo da Application.
2. [ ] Validação (FluentValidation ou atributos/custom já usados no módulo) coerente com o resto da feature.
3. [ ] Mapeamentos e ViewModels alinhados com o que o BFF/frontend consomem.
4. [ ] Migrações EF quando o modelo (Domain) mudar; sem “hacks” que quebrem rastreabilidade.
5. [ ] Testes para caminhos críticos e falhas esperadas (validação, não encontrado, conflito, autorização quando aplicável).

## Evitar

- Repositório genérico por entidade “por hábito” sem ganho claro.
- Lógica de negócio grossa em controladores.
- `try/catch` genérico que esconde erros ou duplica tratamento global já existente.
- Referenciar tipos de Infra a partir de Domain.

## Idioma

Mensagens ao utilizador em Português (Brasil) quando não indicado o contrário; nomes de código em inglês alinhados ao repositório.
