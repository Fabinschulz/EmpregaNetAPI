---
name: backend-skill
description: >-
  Skill completa EmpregaNet: .NET Clean Architecture, mediator interno, Domain/Application/Infra/Api,
  EF Core, validação e testes (xUnit, FluentAssertions, Moq, integração com fixture in-memory).
  Use para qualquer trabalho em backend/src ou backend/tests, alinhado a SDD e agentes dotnet-*.
author: EmpregaNet
version: 2.0.0
date: 2026-05-07
status: Approved
---

# Backend (.NET — EmpregaNet API)

Skill **única** para humanos e agentes de IA: princípios, estrutura, regras de implementação,
testes e anti-padrões adaptados ao repositório. Funde convenções já usadas aqui com boas práticas
tipo “skills” de referência (DDD pragmático, qualidade por camadas, foco em testabilidade —
sem presumir microsserviços ou tooling que não exista neste projeto).

---

## 1. Quando aplicar

| Situação | Usar esta skill |
| -------- | --------------- |
| Alterações em `backend/src/` (Domain, Application, Infra, Api) | Sim |
| Novos comandos/handlers/queries/repositórios/migrations | Sim |
| Testes em `backend/tests/` (Unit ou Integration) | Sim |
| Contratos HTTP para BFF ou frontend | Sim |

---

## 2. Ligações obrigatórias

| Recurso | Path |
| ------- | ---- |
| Mapa `docs/` e comandos de build (`backend/`, `Bff/`) | [`docs/README.md`](../../README.md) |
| Agente para fronteiras e decisões estruturais | [`docs/agents/dotnet-architect.md`](../../agents/dotnet-architect.md) |
| Agente para código .NET concreto | [`docs/agents/dotnet-implementer.md`](../../agents/dotnet-implementer.md) |
| Fluxo especificação antes de código | [`docs/sdd/SDD-ORCHESTRATOR.md`](../../sdd/SDD-ORCHESTRATOR.md) |
| Skill orquestrador SDD | [`docs/skills/sdd-orchestrator/SKILL.md`](../sdd-orchestrator/SKILL.md) |

Para features com pasta de spec ativa (`docs/features/<id>/`): respeitar `prd.md` / `design.md`
antes de divergir; registar decisões pragmáticas nas *deviation notes* do `tasks.md` quando necessário.

---

## 3. Princípios (fusão disciplina × simplicidade)

| Princípio | Como se traduz aqui |
| --------- | ------------------- |
| **Domínio no centro** | Regras e invariantes no Domain; nomenclatura alinhada à linguagem de negócio. |
| **Menos poder útil** | KISS/YAGNI: um handler por comando/query; não adicionar camadas só “por futuro”. |
| **SOLID / coesão** | Tipos pequenos com responsabilidade clara; DRY apenas quando copiar mesmo teria custo real. |
| **Testabilidade** | Handlers com collaborators mockáveis na Unit; Fluxos através de Api/Providers na Integration. |
| **Fonte única para contratos** | Mudanças de contrato HTTP acompanhadas de consumidor (BFF/front) quando o incremento assim o definir |

---

## 4. Arquitectura de camadas (`backend/src/`)

| Camada | Papel | Regras de dependência |
| ------ | ----- | ----------------------- |
| **Domain** | Entidades, value objects / enums, invariantes | **Sem** EF, ASP.NET, HttpClient ou detalhes de infra |
| **Application** | Casos de uso, handlers (`IRequest` / `IRequestHandler`), validators, interfaces de repositório/serviço | Referencia apenas Domain (+ abstrações que o próprio projeto definiu) |
| **Infra** | EF Core, `DbContext`, implementações de repositórios, pipelines do mediator registados aqui | Implementa interfaces da Application ou Domain conforme já existente |
| **Api** | Controllers / endpoints **finos**, auth, filtros | Delega sempre na Application |

**Mediator interno (crítico):** o projeto usa `IRequest` / `IRequestHandler` em **`EmpregaNet.Domain.Libs.Mediator`** (e registos/pipelines típicos na Infra).  
**Não** assumir o pacote NuGet **MediatR** nem outros barramentos paralelos sem existirem na solução.

---

## 5. Application — práticas (inspiradas em guidelines de referência)

- **Fluxo típico de um comando:** validar entrada → carregar/atualizar agregação via interfaces → persistir dentro dos limites já usados (`ITransactional`, Unit of Work implicitamente via repositório,etc.) → erro de negócio como excepção/controlado conforme convenção actual.
- **Validação:** seguir FluentValidation ou padrão do módulo (ex.: `CreateCommand<T>` envolto por validator injectado).
- **`DbContext`:** **nunca** injetar directamente nos handlers da Application neste projeto — usar interfaces de persistência já definidas.
- **Documentação XML:** usar `/// <summary>` em português **quando estiver consistente no módulo**; não inchar com comentários óbvios.
- **Excepções:** registar falhas conforme logging existente; **não** engolir excepções nem duplicar handler global já presente sem necessidade.

Se um módulo histórico tiver estrutura de pastas ligeiramente diferente à “ideal”: **preserve o padrão local** primeiro; refactor estrutural só com tarefa explícita ou ADR quando for transversal.

---

## 6. EF Core

- Evitar **N+1**: projeções com `Select` para DTOs/view models em leituras; `Include` apenas quando indispensable.
- **AsNoTracking** em caminhos só-leitura.
- Paginação em listagens grandes; filtros compiláveis onde fizer sentido.
- Alterações ao modelo (**Domain**) acompanhadas de **migrations** e revisão humana antes de aplicar em produção.

---

## 7. API HTTP

- Mapeamento de erros estável para o cliente (validação vs conflito vs não encontrado vs autorização) — replica padrões existentes nos controllers/handlers próximos.
- **Secrets** apenas em variáveis de ambiente / secret stores; nunca commit.
- Inputs sempre validados; endpoints sensíveis com autorização (**RBAC**) explícita.

---

## 8. Escopo distribuído (apenas quando o produto tiver esse desenho)

Se no futuro existirem mensagens assíncronas ou outros serviços:

- Consumers **idempotentes** para re-processamento / retries documentados pelo broker.
- Não aplicar Saga / Outbox / Event Sourcing só por moda — apenas com requisito explícito e ADR.

---

## 9. Testes (`backend/tests/`)

### Stack actual (facto deste repo)

| Componente | Uso |
| ---------- | ----- |
| xUnit | Framework de testes |
| FluentAssertions | Asserções legíveis |
| Moq | Duplos de collaborators em Unit tests |
| `Microsoft.EntityFrameworkCore.InMemory` | Cenários de integração onde aplicável |

### Testes unitários

- Mirror lógico: `EmpregaNet.Tests.Unit...` agrupados por capacidade (`Admin`, `Users`, `Auth`, etc.).
- Nomeação alinhada ao existente — por exemplo **`Handle_Cenario_DeveOutcome`** (mistura inglês/português aceite quando consistente dentro do mesmo ficheiro).
- **Mocks** para `IRepositories`, validators externos, logger abstract (`NullLogger` quando suficiente).
- Verificações não só “sucesso” mas caminhos de validação/conflito (excepções esperadas ou códigos de erro de domínio).

### Testes de integração

| Regra EmpregaNet | Motivo |
| ---------------- | ------ |
| Atributos `Collection("Integration")` e `DisableParallelization` | Evita corrida sobre o mesmo fixture / BD in-memory (`IntegrationTestCollection`) |
| `ICollectionFixture<InMemoryIdentityFixture>` (onde já existir) | Arranjo partilhado coerente com Identity + provider |
| Alcance recomendado | Cobrir comportamento via handlers ou padrões já presentes nos testes; evitar servidor HTTP novo sem necessidade clara |

Ao adicionar um novo cenário Integration: espelhar a estrutura em `Integration/Handlers/*IntegrationTests.cs` e reutilizar *helpers* já em `EmpregaNet.Tests.Support`.

---

## 10. Checklist de entrega (feature API)

1. Comando/query + handler registados segundo o padrão existente (`Program.cs`/Extensões de DI).
2. Validações coerentes com FluentValidation (ou padrão equivalente já usado no módulo).
3. ViewModels/respostas alinhadas ao que BFF/front consome.
4. Migrações EF quando o modelo mudar **e** regressão dos testes.
5. Unit tests nos handlers críticos; Integration quando tocar pipeline real (Identity, persistência, etc.).
6. Sem referências de **Domain** a tipos de **Infra**; sem `DbContext` na Application.

---

## 11. Anti-patterns (evitar sistematicamente)

| Evitar | Porquê |
| ------ | ------ |
| Repositories genéricos “por hábito” sem ganho claro | Ruído e acoplamento inútil |
| Lógica de negócio gorda em controllers | Viola segregação já assumida pela solução |
| `try/catch` genérico que mascara stack ou duplica tratamento global | Bugs silenciosos |
| Inventar segundo barramento CQRS paralelo ao mediator interno | Duplica inconsistência |

---

## 12. Idioma

- Mensagens utilizador/logs orientados ao negócio: **português (Brasil)** quando o produto estiver assim.
- Identificadores de código: **inglês** coerentes com histórico do repo.

---

## Histórico de versão skill

| Versão | Mudança |
| ------ | ------- |
| 2.0.0 | Estrutura alinhada a skills de referência (YAML, capítulos, testes formais EmpregaNet) mantendo mediator interno e camadas |
