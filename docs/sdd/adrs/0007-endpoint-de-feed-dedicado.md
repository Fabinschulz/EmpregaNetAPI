# ADR 0007: Endpoint de feed dedicado, separado do CRUD genérico e do estado por utilizador

## Status
Aceite

## Contexto

A leitura pública de vagas usava `GET /api/jobs`, servida pelo contrato genérico
`GetAllQuery<TResponse>(Page, Size, OrderBy, IsDeleted, IsActive, Search)` - o mesmo record usado
por `Company`, `User` e demais entidades através de `MainController<,,>`.

O feed de vagas precisa de treze filtros (busca, cidade, estado, modalidade, vínculo, senioridade,
área, tecnologias, benefícios, empresa, faixa salarial, janela de publicação, ordenação), vários
deles multi-valor. Duas pressões apareceram ao mesmo tempo:

1. **Onde colocar os filtros.** Estender `GetAllQuery<T>` levaria treze parâmetros específicos de
   vaga para o contrato compartilhado por todas as entidades. Toda listagem do sistema passaria a
   carregar `Technologies` e `Seniority` na assinatura, e `MainController.GetAll` teria de os
   repassar. O tipo genérico deixaria de ser genérico.

2. **Onde colocar o estado por utilizador.** O cartão precisa indicar "já me candidatei". Essa
   informação é por sessão, mas `GET /api/jobs` é `[AllowAnonymous]` e está sob a política de output
   cache `PublicCatalog`. Incluir o estado pessoal na mesma resposta obrigaria a variar o cache por
   `userId` - multiplicando entradas por utilizador e anulando o cache justamente na rota mais
   acessada do produto. Sem variar, seria pior: a resposta de um utilizador serviria a outro.

## Decisão

- **Query e endpoint dedicados para o feed.** `GetJobsFeedQuery` com o seu próprio handler,
  validator e view model, exposto em `GET /api/jobs/feed`. `GetAllQuery<T>` fica intacto e continua
  a servir o CRUD genérico, incluindo `GET /api/jobs` para a tela de recrutamento.
- **View model próprio.** `JobFeedItemViewModel` projeta só o que o cartão usa - sem `Description`
  completa - e **não** herda de `BaseViewModel`, porque precisa de `publishedAt` como instante
  ISO-8601 e não como a string pt-BR já formatada que `BaseViewModel` impõe.
- **Enums serializados por nome** (`"workModel": "Hybrid"`), como propriedade `string` do view
  model. Local ao contrato do feed, sem alterar o serializador global.
- **Estado por utilizador em endpoint separado:** `GET /api/jobs/feed/interactions?ids=…`,
  `[Authorize]`, resolvido em lote com uma consulta. O feed permanece anónimo e cacheável.
- **Read model no Domain.** O repositório devolve `JobFeedProjection` (record do Domain), não
  entidades. Permite join com `Company` e contagem de candidaturas por subquery numa consulta só,
  sem levar EF para o Domain nem gerar N+1.

## Consequências

**Positivas:**
- `GetAllQuery<T>` continua a ser um contrato genérico de verdade. Nenhuma outra entidade paga pela
  complexidade do feed de vagas.
- O feed mantém uma entrada de cache por combinação de filtros (a política `PublicCatalog` já usa
  `QueryKeys = "*"`) e a invalidação por tag de entidade existente continua a valer.
- O estado pessoal custa um round-trip em lote, não uma consulta por cartão nem um cache por
  utilizador.
- A projeção enxuta reduz o tráfego por cartão - o texto completo da vaga só é carregado no detalhe.

**Negativas / obrigações futuras:**
- Existem agora **dois caminhos de leitura de vagas** (`/api/jobs` e `/api/jobs/feed`). Mudança no
  domínio pode exigir tocar os dois. É o preço de não contaminar o contrato genérico, e a separação
  é legítima: um serve gestão, outro serve descoberta.
- O contrato do feed diverge do resto da API em dois pontos (enums por nome, data em ISO-8601). A
  divergência é deliberada e está documentada, mas é divergência - quem criar endpoints novos deve
  seguir o padrão da API, não o do feed, salvo se tiver a mesma necessidade.
- O frontend faz duas requisições no primeiro render autenticado (feed + interações). Aceitável:
  a segunda é pequena, dispara só com sessão e não bloqueia a renderização dos cartões.

## Referências

- `backend/src/EmpregaNet.Application/Jobs/Queries/GetJobsFeedHandler.cs`
- `backend/src/EmpregaNet.Api/Controllers/Jobs/JobsController.cs`
- `backend/src/EmpregaNet.Api/Configuration/OutputCache/OutputCachePolicyBase.cs`
- [`docs/features/emp-feed-vagas/design.md`](../../features/emp-feed-vagas/design.md)
- [ADR 0006](0006-agregado-job-enriquecido.md) - campos que este endpoint expõe
