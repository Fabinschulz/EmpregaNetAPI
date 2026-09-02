---
name: backend-skill
description: Convenções canónicas do backend .NET do EmpregaNet — Clean Architecture (Domain/Application/Infra/Api), mediator interno EmpregaNet.Domain.Libs.Mediator (não MediatR), EF Core, FluentValidation, separação Auth vs dados do utilizador, YAGNI aplicado (quatro custos, custos assimétricos de contrato HTTP e migration destrutiva) e testes xUnit/FluentAssertions/Moq com fixture in-memory. Use ao ler, escrever ou revisar qualquer coisa em backend/src ou backend/tests, ao definir contratos HTTP consumidos pelo Bff/frontend, e ao decidir se uma abstracção, flag ou coluna se constrói agora ou se adia. Não use para trabalho de UI (frontend-skill) nem para especificação de feature antes de código (sdd-orchestrator).
---

# Backend (.NET — EmpregaNet API)

Base de **conhecimento** do backend: fatos do repositório, regras de camada, testes e anti-padrões.
Não é um perfil de comportamento — o comportamento está nos agents `dotnet-architect` e `dotnet-implementer`,
que carregam esta skill como contexto obrigatório.

---

## 1. Quando aplicar

| Situação | Aplicar |
| -------- | ------- |
| Alterações em `backend/src/` (Domain, Application, Infra, Api) | Sim |
| Novos comandos/handlers/queries/repositórios/migrations | Sim |
| Testes em `backend/tests/` (Unit ou Integration) | Sim |
| Contratos HTTP consumidos pelo Bff ou frontend | Sim |
| UI, hooks, estilos | Não — [`frontend-skill`](../frontend-skill/SKILL.md) |
| Especificar feature antes de código | Não — [`sdd-orchestrator`](../sdd-orchestrator/SKILL.md) |

---

## 2. Ligações

| Recurso | Path |
| ------- | ---- |
| Mapa do monorepo e comandos de build | [`docs/README.md`](../../../docs/README.md) |
| Fronteiras e decisões estruturais | [`dotnet-architect`](../../agents/dotnet-architect.md) |
| Código .NET concreto | [`dotnet-implementer`](../../agents/dotnet-implementer.md) |
| Governo SDD (fases e gate) | [`docs/sdd/SDD-ORCHESTRATOR.md`](../../../docs/sdd/SDD-ORCHESTRATOR.md) |
| ADRs transversais | [`docs/sdd/adrs/`](../../../docs/sdd/adrs/) |

Para features com pasta de spec activa (`docs/features/<id>/`): respeitar `prd.md` / `design.md`
antes de divergir; registar desvios pragmáticos nas *deviation notes* do `tasks.md`.

---

## 3. Princípios

| Princípio | Como se traduz aqui |
| --------- | ------------------- |
| **Domínio no centro** | Regras e invariantes no Domain; nomenclatura alinhada à linguagem de negócio. |
| **Menos poder útil** | KISS/YAGNI: um handler por comando/query; não adicionar camadas "para o futuro" (§3.1). |
| **SOLID / coesão** | Tipos pequenos com responsabilidade clara; DRY só quando a duplicação tiver custo real. |
| **Testabilidade** | Handlers com colaboradores mockáveis no Unit; fluxos via Api/providers no Integration. |
| **Fonte única de contratos** | Mudança de contrato HTTP acompanha o consumidor (Bff/front) quando o incremento assim o define. |

### 3.1 YAGNI — o que não se constrói agora

Critério de **Martin Fowler** ([`martinfowler.com/bliki/Yagni.html`](https://martinfowler.com/bliki/Yagni.html)),
aplicado a este backend. **Feature presumida** é capacidade construída hoje para uma necessidade suposta
amanhã — e YAGNI aplica-se só a isso.

Quatro custos, não só o primeiro:

| Custo | O que é | Sintoma aqui |
| ----- | ------- | ------------ |
| **Construir** | Esforço gasto na capacidade presumida | Handler, endpoint ou repositório que ninguém chama |
| **Atraso** | O que ficou por entregar enquanto se construía a presunção | Incremento do `tasks.md` que escorregou de release |
| **Carregar** | A complexidade extra torna **todo o resto** mais caro de mudar | Interface com uma implementação; camada que cada mudança obriga a atravessar |
| **Reparar** | Quando a necessidade chega diferente do presumido, desfazer custa mais do que nunca ter feito | Abstracção que se torce para caber; migration de correcção |

Ambos os desfechos perdem: se não for precisa, paga-se construir + carregar; se for precisa mas diferente,
paga-se construir + carregar + reparar, e a versão errada ainda enviesa a solução certa. O custo de **carregar**
é o que ninguém atribui à decisão que o originou.

**Limite do princípio — YAGNI corta capacidade, nunca qualidade.** Fowler é explícito: cobre capacidade para
feature presumida, **não** o esforço de manter o software fácil de modificar. Não serve para cortar testes de
comportamento, validação de input, RBAC explícito, tratamento de erro, coesão ou nomes claros — isso é o que
baixa o custo de mudar depois. Adiar só é barato onde há teste a proteger o comportamento e fronteira de camada
respeitada; se faltarem, a decisão honesta é criar a rede primeiro, não construir por precaução.

**Custos assimétricos — aqui adiar é mais caro que construir**, e a decisão fica registada:

| Decisão | Porque adiar é caro |
| ------- | ------------------- |
| **Contrato HTTP** já consumido pelo Bff/frontend | Mudar depois é breaking change fora da camada dona (§7) |
| **Migration com `rename`/`drop`** | Release é forward-only; o canário aplica e o rollback não recupera dados (§6) |
| **Captura de dados** (auditoria, analytics, timestamps) | Dado não recolhido não se obtém retroactivamente |
| **Autenticação, autorização e cookies** | Falha aberta em produção não se compensa depois (§7) |

Fora desta lista, presunção vai para o backlog, não para o código. Casos concretos que **não** se constroem
por antecipação: CQRS/Event Sourcing/Saga/Outbox sem requisito (§8); repositório genérico ou interface com uma
única implementação; segundo barramento ao lado do mediator interno; parâmetro, flag ou coluna nullable "para
o futuro"; cache sem número de base e sem política de invalidação.

**Teste de decisão** — para cada peça sem consumidor hoje, uma resposta fraca basta para adiar:

1. Quem consome isto hoje? "Ninguém, mas..." é feature presumida.
2. Quanto custa acrescentar quando a necessidade chegar? Se cai na tabela acima, decidir agora com fundamento.
3. O que esta peça torna mais caro enquanto existir? É o custo de carregar — nomeá-lo.
4. Qual o gatilho concreto que a traz de volta? Sem gatilho nomeável, a necessidade é imaginada.

Registar a recusa numa linha, no PR ou no `tasks.md`:

> **Adiado:** `<capacidade>` — sem consumidor hoje; custo de adicionar depois é local a `<ficheiro/módulo>`;
> gatilho de retorno: `<evento concreto>`.

---

## 4. Camadas (`backend/src/`)

| Camada | Papel | Regras de dependência |
| ------ | ----- | --------------------- |
| **Domain** | Entidades, value objects/enums, invariantes | **Sem** `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.Mvc`, `HttpClient` ou tipos HTTP |
| **Application** | Casos de uso, handlers (`IRequest`/`IRequestHandler`), validators, interfaces de repositório/serviço | Referencia apenas Domain (+ abstracções que o próprio projecto definiu) |
| **Infra** | EF Core, `DbContext`, implementações de repositórios, pipelines do mediator | Implementa interfaces da Application/Domain |
| **Api** | Controllers/endpoints **finos**, auth, filtros | Delega sempre na Application |

**Mediator interno (crítico):** `IRequest` / `IRequestHandler` vivem em **`EmpregaNet.Domain.Libs.Mediator`**;
registos e pipelines na Infra. **Não** introduzir o pacote NuGet **MediatR** nem outro barramento paralelo.

**Excepção registada:** `User`/`Role` herdam do ASP.NET Core Identity dentro do Domain —
ver [ADR 0005](../../../docs/sdd/adrs/0005-identity-no-dominio.md). Não generalizar essa excepção para outras entidades.

---

## 5. Application — práticas

- **Fluxo típico de um comando:** validar entrada → carregar/actualizar agregado via interfaces → persistir dentro dos limites já usados (`ITransactional`, Unit of Work via repositório) → erro de negócio como excepção/resultado controlado conforme a convenção do módulo.
- **Validação:** FluentValidation ou o padrão do módulo (ex.: `CreateCommand<T>` envolto por validator injectado).
- **`DbContext`:** **nunca** injectar directamente nos handlers da Application — usar as interfaces de persistência já definidas.
- **Documentação XML:** `/// <summary>` em português **quando já consistente no módulo**; não inchar com comentários óbvios.
- **Excepções:** registar falhas conforme o logging existente; não engolir excepções nem duplicar o handler global.

Se um módulo histórico tiver estrutura ligeiramente diferente da "ideal": **preservar o padrão local**;
refactor estrutural só com tarefa explícita ou ADR quando for transversal.

### 5.1 Auth ≠ dados do utilizador

Duas responsabilidades separadas de propósito — não voltar a misturá-las ao adicionar endpoints:

| Responsabilidade | Controller | Application |
| ---------------- | ---------- | ----------- |
| Credencial e sessão: login, logout, registo, refresh, Google, confirmação de e-mail, esqueci/redefinir senha | `AuthController` → `/api/auth/*` | `Application/Auth/Commands/` |
| Dados da própria conta: ver/editar perfil, trocar senha (autenticado), encerrar conta | `UsersController` → `/api/users/me*` | `Application/Users/` |
| Gestão de utilizadores por administrador | `AdminController` | `Application/Admin/Users/` |

- **`AuthController` é `[AllowAnonymous]` na classe** (quem chama ainda não tem sessão); **`UsersController` é `[Authorize]` na classe**. Isso torna a autorização segura por omissão: endpoint novo em `/api/users` nasce protegido e abrir uma rota exige `[AllowAnonymous]` explícito. Enquanto os dois grupos coexistiam num controller só, cada método declarava o próprio atributo — esquecer um deixava a rota pública sem nada acusar.
- **Trocar a senha autenticado fica em `Users`**, não em `Auth`: é operação sobre a própria conta com sessão activa, diferente de "esqueci a senha" (recuperação de acesso sem sessão).

---

## 6. EF Core

- Evitar **N+1**: projecções com `Select` para DTOs/view models em leituras; `Include` apenas quando indispensável.
- **`AsNoTracking`** em caminhos só-leitura.
- Paginação em listagens grandes; filtros compiláveis onde fizer sentido.
- Alterações ao modelo (Domain) acompanhadas de **migrations** e revisão humana antes de produção.
- **Release com `rename`/`drop` é forward-only** — o canário aplica as migrações e rollback não recupera dados. Planear migração em duas fases (adicionar → migrar dados → remover num release posterior).

---

## 7. API HTTP

- Serialização **camelCase** (MVC + NewtonsoftJson). `JsonConvert.SerializeObject` directo devolve PascalCase e engana em testes/logs — não usar como referência de contrato.
- Formato de erro estável e único para o cliente (validação vs conflito vs não encontrado vs autorização) — ver [ADR 0008](../../../docs/sdd/adrs/0008-formato-de-erro-da-api.md) e replicar os controllers/handlers próximos.
- Autenticação por **cookie `httpOnly`** (`access_token`); a API emite o cookie — o cliente **não** manipula token em JS. Ver [ADR 0001](../../../docs/sdd/adrs/0001-auth-por-cookies-httponly.md).
- Listagens públicas expostas via **OData** (`/odata`) onde já existir — não inventar `page`/`size` paralelo.
- **Secrets** apenas em variáveis de ambiente / secret stores; nunca no repositório.
- Inputs sempre validados na fronteira; endpoints sensíveis com **RBAC** explícito.

---

## 8. Escopo distribuído (só quando o produto tiver esse desenho)

- Consumers **idempotentes** para reprocessamento/retries.
- Não aplicar Saga / Outbox / Event Sourcing por moda — apenas com requisito explícito e ADR.

---

## 9. Testes (`backend/tests/`)

### Stack real deste repositório

| Componente | Uso |
| ---------- | --- |
| xUnit | Framework de testes |
| FluentAssertions | Asserções legíveis |
| Moq | Duplos de colaboradores em Unit tests |
| `Microsoft.EntityFrameworkCore.InMemory` | Cenários de integração |

### Unit

- Espelho lógico: `EmpregaNet.Tests.Unit...` agrupado por capacidade (`Admin`, `Users`, `Auth`, …).
- Nomeação alinhada ao existente — ex.: `Handle_Cenario_DeveOutcome` (mistura inglês/português aceita quando consistente no mesmo ficheiro).
- **Mocks** para `IRepositories`, validators externos, logger (`NullLogger` quando suficiente).
- Cobrir não só sucesso: caminhos de validação, conflito e não autorizado.

### Integration

| Regra | Motivo |
| ----- | ------ |
| `Collection("Integration")` + `DisableParallelization` | Evita corrida sobre o mesmo fixture / BD in-memory (`IntegrationTestCollection`) |
| `ICollectionFixture<InMemoryIdentityFixture>` (onde já existir) | Arranjo partilhado coerente com Identity + provider |
| Alcance | Cobrir comportamento via handlers ou padrões já presentes; evitar servidor HTTP novo sem necessidade clara |

Novo cenário Integration: espelhar `Integration/Handlers/*IntegrationTests.cs` e reutilizar helpers de `EmpregaNet.Tests.Support`.

**Limitação do InMemory a declarar sempre que for relevante:** não reproduz constraints, semântica de provider real nem migrations. Comportamento dependente do PostgreSQL não é validável aí.

---

## 10. Validação (comandos reais)

```bash
dotnet build backend/EmpregaNet.sln
```

```bash
dotnet test backend/tests/tests.csproj
```

Quando o diff tocar o `Bff/`:

```bash
dotnet build Bff/EmpregaNet.Bff.sln
```

---

## 11. Checklist de entrega (feature API)

1. [ ] Comando/query + handler registados segundo o padrão existente (`Program.cs` / extensões de DI).
2. [ ] Validações coerentes com FluentValidation (ou padrão do módulo).
3. [ ] ViewModels/respostas alinhadas ao que Bff/front consome, em camelCase.
4. [ ] Migrations EF quando o modelo mudar, com plano forward-only se houver `rename`/`drop`.
5. [ ] Unit tests nos handlers críticos; Integration quando tocar pipeline real (Identity, persistência).
6. [ ] Sem referências de Domain a tipos de Infra; sem `DbContext` na Application.
7. [ ] `dotnet build` + `dotnet test` verdes (§10).
8. [ ] Toda abstracção, flag ou coluna introduzida tem **consumidor no mesmo diff**; o que foi adiado está registado com gatilho de retorno (§3.1).

---

## 12. Anti-padrões

| Evitar | Porquê |
| ------ | ------ |
| Repositórios genéricos "por hábito" sem ganho claro | Ruído e acoplamento inútil |
| Lógica de negócio gorda em controllers | Viola a segregação assumida pela solução |
| `try/catch` genérico que mascara stack ou duplica tratamento global | Bugs silenciosos |
| Segundo barramento CQRS paralelo ao mediator interno | Duplica inconsistência |
| `DbContext` injectado num handler da Application | Quebra a fronteira de camada |
| Tratar `SerializeObject` como contrato HTTP | PascalCase falso; a API responde camelCase |
| Interface com uma implementação, flag nunca alternada, coluna "para o futuro" | Custo de carregar sem consumidor (§3.1) |
| "Já que estamos a mexer aqui, deixo preparado" | Presunção disfarçada de eficiência (§3.1) |
| Invocar YAGNI contra teste, validação, RBAC ou tratamento de erro | Fora do âmbito do princípio: corta capacidade, não qualidade (§3.1) |

---

## 13. Idioma

Mensagens de utilizador e logs de negócio: **português (Brasil)**. Identificadores de código: **inglês**.

---

## Histórico

| Versão | Mudança |
| ------ | ------- |
| 3.1.0 | YAGNI deixa de ser uma linha de tabela e passa a critério aplicável (§3.1): quatro custos de Fowler, limite do princípio, custos assimétricos deste backend (contrato HTTP, migration destrutiva, captura de dados, autorização) e teste de decisão, com item de checklist e anti-padrões correspondentes |
| 3.0.0 | Movida para `.claude/skills/` (passa a ser carregável); separada de comportamento (agents); acrescentados camelCase, cookie httpOnly, OData, migrations forward-only, limitação do InMemory e secção de validação com comandos reais |
| 2.0.0 | Estrutura alinhada a skills de referência mantendo mediator interno e camadas |
