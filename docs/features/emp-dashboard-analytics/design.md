---
version: 1.3.0
date: 2026-08-23
---

# Design técnico — Dashboard de Métricas e Analytics (`emp-dashboard-analytics`)

Painel de analytics do recrutamento em `/dashboard`, com cinco endpoints no backend — um por seção da
tela — e uma tela composta por seções independentes no frontend.

Este documento é a fonte de verdade sobre **de onde vem cada número** e **o que o domínio atual não
sustenta**. A segunda parte não é rodapé: é o que impede a tela de exibir zero onde a resposta
correta é "não medimos isso".

---

## 1. Análise do domínio existente

Entidades e campos realmente disponíveis (`backend/src/EmpregaNet.Domain/Entities/`):

| Entidade | Campos usados pelo painel | Observações |
| -------- | ------------------------- | ----------- |
| `Job` | `CompanyId`, `Title`, `Area`, `Location` (City/State/Country), `IsActive`, `PublishedAt`, `CreatedAt`/`UpdatedAt`/`IsDeleted` | Sem data de expiração e sem contador de exibições |
| `JobApplication` | `JobId`, `UserId`, `Status`, `AppliedAt`, `IsDeleted` | Guarda apenas o **status atual**, sem histórico de transições |
| `Company` | `CompanyName`, `CreatedAt`, `IsDeleted` | **Não tem campo de ativação** |
| `User` (Identity) | `UserType`, `Address` (opcional), `EmailConfirmed`, `CreatedAt`, `IsDeleted`, `EmployerCompanyId` | Sem registo de último acesso; sem área/profissão |
| `UserRefreshToken` | — | `CreatedAt` existe, mas rotaciona e é revogado: proxy inconfiável de atividade, **não usado** |

Relacionamentos relevantes:

- `JobApplication.UserId` → `User` (FK, `User.Applications`).
- `JobApplication.JobId` → **coluna com índice, sem FK nem navegação** (`IX_JobApplications_JobId`).
  Toda leitura que precise da vaga faz join explícito.
- `Job.CompanyId` → `Companies` (índice `IX_Jobs_CompanyId`).
- `User.EmployerCompanyId` → `Companies` (`ON DELETE SET NULL`). É o vínculo que define o escopo.

Exclusão lógica: **não há filtro global de `IsDeleted`** no `PostgreSqlContext`. Toda consulta do
painel filtra explicitamente.

Fatos do domínio que mudaram decisões:

1. **A candidatura nasce em `Processing`**, não em `Pending` (`JobApplication(jobId, userId)`).
   `Pending` só é alcançável por transição de retorno.
2. **Não existe status de contratação.** O fluxo termina em `Finished` ("Concluída"), que não
   distingue contratado de encerrado sem contratação.
3. **Encerrar vaga não grava data própria** — `Close()` apenas zera `IsActive`, e `UpdatedAt` é
   movido por qualquer edição.
4. **`IsActive` não tem histórico**: "vagas ativas" é sempre leitura do presente.

---

## 2. Catálogo de métricas

### 2.1 Disponíveis (leitura direta)

| Métrica | Origem |
| ------- | ------ |
| Vagas ativas | `COUNT(Jobs WHERE IsActive AND NOT IsDeleted)` |
| Vagas publicadas no período | `PublishedAt` na janela |
| Candidaturas recebidas no período | `AppliedAt` na janela |
| Candidaturas por status | `GROUP BY Status` |
| Vagas por área / por UF | `GROUP BY Area` / `GROUP BY Location.State` |
| Empresas cadastradas / novas | `Companies.CreatedAt` |
| Usuários / novos usuários / por perfil | `Users.CreatedAt`, `UserType` |
| Candidaturas por vaga (ranking) | subconsulta correlacionada por `JobId` |

### 2.2 Disponíveis com cálculo

| Métrica | Regra |
| ------- | ----- |
| Novos candidatos | `MIN(AppliedAt)` por `UserId` cai na janela — entrada no funil, não cadastro |
| Total de candidatos | `COUNT(DISTINCT UserId)` sobre candidaturas não excluídas |
| Empresas ativas | empresas com ≥ 1 vaga ativa (não há campo de ativação) |
| Taxa de aprovação | `(Approved + Finished) ÷ candidaturas do período` |
| Chegaram à aprovação | `Approved + Finished` — soma necessária porque o status não tem histórico |
| Média de candidaturas por vaga | candidaturas acumuladas ÷ vagas existentes no fim da janela |
| Desempenho vs. média | desvio percentual sobre a média, com faixa neutra de ±10% |
| Vagas estagnadas | ativas, publicadas há > 30 dias e sem candidatura nesse prazo |
| Vagas sem nenhuma candidatura | ativas com zero candidaturas até o fim da janela — conjunto distinto do anterior, inclui as recém-publicadas |
| Vagas encerradas no período | inativas com `UpdatedAt` na janela (aproximação declarada) |
| Variação vs. período anterior | janela imediatamente anterior, **mesma duração** |
| Candidatos por UF | `User.Address.State`; endereço é opcional, e a diferença é declarada |

### 2.3 Depende de nova informação

| Métrica | O que falta |
| ------- | ----------- |
| Visualizações de vaga | Entidade/contador de exibição. Sem isso o funil começa na candidatura |
| Contratações e taxa de contratação | Status (ou entidade) de contratação distinto de `Finished` |
| Vagas próximas do vencimento | `Job.ExpiresAt` |
| Candidatos por área de atuação | Área/profissão no cadastro do candidato |
| Usuários inativos (de facto) | `LastLoginAt` — hoje aproximado por e-mail não confirmado |
| Vagas encerradas (data exata) | `Job.ClosedAt` — hoje aproximado por `UpdatedAt` |
| Evolução da candidatura pelo funil ao longo do tempo | Histórico de transições de status |

Cada item de 2.3 viaja na resposta em `meta.unavailable`
(`Application/Dashboard/UseCase/DashboardDomainGaps.cs`). **Desde a curadoria de 2026-08-23 já não é
exibido na tela:** limitação de domínio é linguagem de implementação, e este documento é o lugar dela.
Onde a limitação muda a leitura de um número concreto, ela aparece no tooltip daquele painel — o funil
diz que começa na candidatura, o ranking de vagas diz que a vaga não expira.

### 2.4 Não é possível calcular

Nada foi classificado aqui: todos os pedidos do produto caem em 2.1–2.3. O que não é calculável hoje
é calculável assim que o dado de 2.3 existir.

### 2.5 Regras de exibição de um número

Três estados diferentes que a tela **não** pode confundir, e onde cada um é decidido:

| Estado | Contrato | Tela |
| ------ | -------- | ---- |
| Existe valor e existe base de comparação | `value`, `previousValue`, `changePercent` | número + variação assinada + "vs. período anterior" |
| Existe valor, base anterior é zero | `value`, `previousValue = 0`, `changePercent = null` | número, **sem linha de variação**; o tooltip explica |
| Indicador acumulado (foto, não fluxo) | `previousValue = null`, `isPeriodScoped = false` | número, sem linha de variação; o tooltip diz que é acumulado |
| **Não existe valor** | `value = null` | "—" + "ainda sem dados no período" |
| Métrica não medida pelo domínio | entrada em `meta.unavailable` | **nada na tela** — vive neste documento e nos tooltips |

As duas últimas colunas mudaram na curadoria de 2026-08-23 (§7): "sem base de comparação" e "métrica
não disponível neste domínio" são vocabulário de quem implementou. O contrato continua a distinguir
os estados — o que mudou foi a tela deixar de os anunciar em texto, passando a explicá-los no tooltip
de quem for procurar.

O quarto caso é o que exigiu tornar `DashboardKpiViewModel.Value` anulável: taxa de aprovação num
período sem nenhuma candidatura é uma divisão sem denominador, e o `?? 0m` anterior fazia o cartão
afirmar "0%" — que se lê como "ninguém foi aprovado", não como "não houve processo".

A regra da percentagem vive num lugar só (`DashboardKpiFactory.ChangePercent`): base nula ou zero
devolve `null`. Espalhada pelos cartões, o primeiro `if` esquecido exibiria "+100%" onde não há base.

---

## 3. Arquitetura

```
Domain
  Common/Dashboard/DashboardFilter.cs        escopo, intervalo semiaberto UTC, deslocamento local
  Common/Dashboard/DashboardProjections.cs   registos de saída (só agregados)
  Interfaces/IDashboardAnalyticsRepository   contrato de leitura agregada
  Enums/DashboardPeriod.cs, DashboardGranularity.cs

Infra
  Persistence/Repositories/Dashboard/DashboardAnalyticsRepository.cs

Application/Dashboard
  DashboardFilterInput.cs                    recortes como chegam da requisição
  UseCase/DashboardScopeAccess.cs            RBAC → escopo
  UseCase/DashboardPeriod.cs                 período e granularidade
  UseCase/DashboardQueryContext.cs           contexto + meta da resposta
  UseCase/DashboardBreakdownFactory.cs       distribuições, cauda longa, percentagens
  UseCase/DashboardSeriesBuilder.cs          dobra dia → semana/mês, preenche zeros
  UseCase/DashboardKpiFactory.cs             comparação e tendência
  UseCase/DashboardInsightsBuilder.cs        leituras derivadas (função pura)
  UseCase/DashboardDomainGaps.cs             lacunas declaradas
  Queries/*, ViewModel/*, Queries/Validator.cs

Api
  Controllers/Dashboard/DashboardController.cs
  Controllers/Dashboard/DashboardRequest.cs
  Configuration/OutputCache/… política DashboardRead
```

### 3.1 Por que cinco endpoints, e não um

O corte é **por seção da tela**, não por métrica. Numa resposta única, a seção mais lenta atrasaria
todo o painel e uma consulta com falha derrubaria a tela inteira. Separadas, os cartões do topo
chegam primeiro e cada seção falha, recarrega e é cacheada por conta própria.

| Endpoint | Seção | Consultas agregadas |
| -------- | ----- | ------------------- |
| `GET /api/dashboard/overview` | Indicadores + funil | 3 |
| `GET /api/dashboard/trends` | Séries temporais | 3 |
| `GET /api/dashboard/distribution` | Status + concentração por área | 3 |
| `GET /api/dashboard/jobs` | Ranking de vagas | 3 |
| `GET /api/dashboard/insights` | Leituras derivadas | 9 |

`insights` é o mais caro porque cruza fontes diferentes (saúde da carteira, contadores comparados,
concentração por área, vaga líder e média) para decidir o que vale dizer. É o custo de emitir só
afirmações sustentadas em vez de três frases fixas.


`jobs` e `insights` cresceram na v1.1: o primeiro passou a devolver o bloco de saúde
(`GetJobHealthAsync`, 4 `COUNT`), o segundo passou a ler os contadores comparados para poder falar de
crescimento e de queda. Em ambos os casos o alternativo era um endpoint novo por bloco — mais idas ao
servidor para desenhar a mesma seção da tela.

### 3.2 Contratos

Parâmetros comuns (`DashboardRequest`), em todos os cinco:

| Parâmetro | Tipo | Nota |
| --------- | ---- | ---- |
| `period` | `Today \| Last7Days \| Last30Days \| Last90Days \| ThisYear \| Custom` | padrão `Last30Days` |
| `from`, `to` | `yyyy-MM-dd` | obrigatórios em `Custom`; dia civil no fuso de Brasília; teto de 366 dias |
| `companyId` | `long?` | pedido, não instrução — ver §5 |
| `state` | `UF[]` (chave repetida) | máx. 20 |
| `area` | `JobAreaEnum[]` (chave repetida) | máx. 20 |
| `status` | `ApplicationStatusEnum?` | **só recorta números de candidatura** |

Específicos: `granularity` (`trends`), `topCompanies` (`distribution`), `ranking`/`limit`/`onlyActive`
(`jobs`).

Toda resposta traz `meta`: período pedido e resolvido (local e UTC), período de comparação,
`generatedAt`, escopo, `appliedFilters` e `unavailable`.

**Indicadores como lista de descritores**, e não campos fixos: o conjunto muda com o escopo
(utilizadores e empresas só existem na visão de plataforma). Com campos fixos, a tela precisaria de
um `if` por indicador para decidir se um nulo significa "zero" ou "não se aplica ao seu perfil".

---

## 4. Estratégia de queries

**Tudo agrega no banco.** Nenhum método do repositório devolve entidade; o que sai são escalares e
linhas de `GROUP BY` — no máximo uma por dia da janela, por membro de enum, por empresa cadastrada
ou pelo limite do ranking.

Decisões que valem registo:

1. **Candidatura sempre passa pela vaga.** Todo agregado de candidatura junta `JobApplications` a
   `Jobs`, mesmo sem filtro de empresa/UF/área, para excluir candidaturas de vaga apagada. Sem isso,
   o total não fecharia com a distribuição por área nem com a soma do ranking.
2. **Duas janelas numa varredura.** `GetComparedCountersAsync` conta período atual e anterior por
   agregação condicional, com o predicado externo abrindo no início do período anterior para o índice
   continuar a recortar. Metade das idas ao banco.
3. **Baldes de dia em hora local, no banco.** `instant.UtcDateTime + offset` antes de extrair
   ano/mês/dia. O `UtcDateTime` torna a extração independente do `TimeZone` da sessão do Postgres;
   agrupar por dia UTC deslocaria três horas de cada dia para o balde vizinho.
   *Assunção registada:* um único deslocamento por janela — válido para `America/Sao_Paulo`, sem
   horário de verão desde 2019. Semana e mês são dobrados na Application, com o fuso completo.
4. **Ordenar antes de projetar.** O ranking projeta escalares num tipo anónimo, ordena pela coluna de
   contagem e só depois monta o registo. `OrderBy` sobre uma propriedade do registo projetado **não é
   traduzível** — falha em runtime, e foi encontrada assim na verificação pelo browser.
5. **Top de empresas em duas agregações + junção em memória**, em vez de subconsulta correlacionada
   por empresa: cada lado devolve no máximo uma linha por empresa cadastrada (dezenas a centenas),
   e a alternativa é frágil de traduzir sem ganho nessa escala.
6. **Média por divisão de dois `COUNT`**, não por média de subconsulta correlacionada: mesmo número,
   tradução robusta.
7. **`ToSingleRowAsync` em vez de `FirstOrDefaultAsync`** nas agregações condicionais: evita cinco
   avisos `RowLimitingOperationWithoutOrderByWarning` por carga, que afogariam um aviso real.

Índices já existentes que sustentam o painel: `IX_Jobs_Feed (IsDeleted, IsActive, PublishedAt DESC)`,
`IX_Jobs_CompanyId`, `IX_Jobs_Location (State, City)`, `IX_JobApplications_JobId`,
`IX_Users_EmployerCompanyId`. **Nenhuma migration foi necessária.**

Índices a avaliar quando o volume crescer (não aplicados agora — só com medição):

- `JobApplications (AppliedAt)` — as janelas de candidatura hoje varrem por `JobId`.
- `Jobs (Area)` e `Users (CreatedAt)` para as distribuições e a série de utilizadores.

### 4.1 Cache

Política própria `DashboardRead`: TTL de **300 s** (`OutputCache:DashboardExpirationSeconds`), vary
por utilizador e por query. Invalidação **por tempo, não por tag**: qualquer candidatura ou vaga nova
altera algum número, então invalidar por evento esvaziaria o cache continuamente. O cabeçalho da tela
exibe `generatedAt`, que é a idade real do dado servido.

Medição local (primeira carga, cinco endpoints em paralelo, build Debug com log sensível ligado):
314–360 ms por endpoint.

---

## 5. Segurança

O escopo é decidido no servidor, em `DashboardScopeAccess`, e em nenhum outro lugar:

| Papel | Escopo | `companyId` na query |
| ----- | ------ | -------------------- |
| `Admin` | plataforma inteira, ou a empresa pedida | aceito, com validação de existência |
| `Recruiter` / `Manager` | sempre `User.EmployerCompanyId` | **recusado** se diferente da própria |
| `Candidate` | sem acesso | rota bloqueada pela política + recusa no caso de uso |

- A classe do controller exige `Constants.AuthPolicies.Recrutamento`.
- `RecruitmentAccess.EnsureRecruitmentStaff` repete a verificação na Application (defesa em
  profundidade).
- Pedir empresa de terceiros é **recusado**, não ignorado: ignorar devolveria dados válidos para a
  pergunta errada.
- Empresa inexistente falha em vez de devolver zeros — indistinguíveis de empresa sem movimento.
- No escopo de empresa, os números de plataforma (utilizadores, empresas) **não são calculados nem
  devolvidos**.
- `ControllerAuthorizationTests` obriga a classificar todo controller novo; `DashboardController`
  está registado como superfície privilegiada.

---

## 6. Frontend

```
features/dashboard/
  index.tsx                       roteia por papel (analytics vs. candidato)
  candidate/                       painel do candidato (própria atividade)
  service/                         api + schemas Zod + keys + queries
  analytics/
    index.tsx                      composição das seções, na ordem da narrativa (§6.1)
    header/                        título, recorte numa linha, última atualização, atualizar
    filters/                       barra de filtros (RHF + Zod)
    kpis/                          faixa de indicadores (5 ou 6, plano único)
    sections/                      trends, distribution (2 painéis), jobs
    funnel/                        funil de recrutamento
    insights/                      insights agrupados por categoria
    jobs/                          lista de performance das vagas
    charts/                        primitivas Recharts + tooltip + tema + a11y
    shared/                        painel com estados, skeletons, formatação, segmented control
```

### 6.1 Hierarquia e narrativa da tela

Quatro perguntas, na ordem em que se fazem:

| Nível | Seção | Pergunta |
| ----- | ----- | -------- |
| 1 | Indicadores da operação (5–6 cartões) | Como está a operação? |
| 2 | Evolução no período (largura total) | O que mudou? |
| 3 | Funil + status + concentração por área | Onde está o gargalo? |
| 4 | Performance das vagas (largura total) | O que performa, e o que precisa de intervenção? |
| 5 | O que exige atenção (insights, largura total) | Onde tenho de olhar? |

Decisões sobre hierarquia:

- **Plano único de indicadores.** A v1.1 tinha dois planos (`tier` no contrato, secundários num
  `<details>`) porque a visão de plataforma emitia treze cartões. A curadoria de 2026-08-23 cortou os
  administrativos, e com cinco ou seis cartões o mecanismo de esconder deixou de ter função: `tier`
  saiu do contrato e o `<details>` saiu da tela. Um plano de destaque só se justifica quando há o que
  não destacar.
- **Texto técnico vai para o tooltip.** Origem, regra de cálculo e a diferença entre acumulado e
  período saem do corpo do cartão. O cartão fica com rótulo, número e variação.
- **As lacunas do domínio saíram do cabeçalho** para o fim da página (`notes/`). É informação que se
  procura ao estranhar um número, não ao abrir a tela — e no topo soava a defeito do painel em vez de
  limite do domínio. Continua na tela, e não só neste documento: exibir zero para "contratações"
  faria o utilizador concluir que ninguém foi contratado.
- **Quatro painéis de distribuição, uma consulta, três lugares na página.** Cada painel chama o mesmo
  hook com a mesma chave, e o React Query serve todos de um pedido só — é o que permite espalhá-los
  pela narrativa sem multiplicar idas ao servidor.
- **A marca segue a densidade dos dados.** A evolução é área acima de quatro baldes e barra abaixo
  disso: com um balde só, a área não desenha nada e sobrava uma moldura vazia de 260 px. A altura
  acompanha (300 px como protagonista, 200 px quando há pouco para mostrar).
- **Insights agrupados por natureza** (atenção, crescimento, destaque, comportamento) e ordenados por
  severidade. Numa lista corrida, um alerta de vaga parada e uma nota de concentração por área têm o
  mesmo peso, e o utilizador deixa de distinguir decisão de contexto.
- **Cor por significado, aplicada com parcimónia.** No bloco de saúde só recebem cor as contagens
  maiores que zero que pedem ação; nos insights, só o título do grupo e uma barra lateral por
  severidade. Quatro contagens amarelas equivalem a nenhuma.
- **Ranking por UF e por empresa passaram a Recharts** (v1.2). A geografia usa a mesma
  `BreakdownBarChart` da concentração por área — mesma geometria para a mesma pergunta dispensa
  reaprender o gráfico a cada painel. As empresas ganharam gráfico próprio porque a forma dos dados é
  outra: só as candidaturas entram no desenho (é a dimensão que ordena), e vagas ativas e média por
  vaga ficam no tooltip. Desenhar as três como barras agrupadas mostraria menos com mais tinta — as
  escalas são incomensuráveis, e a barra de "vagas ativas" ficaria invisível ao lado de centenas de
  candidaturas. A **tabela continua disponível na alternância do painel**, porque é o único lugar onde
  as três métricas aparecem juntas.
- **Grade tracejada perpendicular às barras.** Em barras deitadas o que se compara é comprimento, e
  as linhas que ajudam a ler são as verticais — não as horizontais, que apenas separam as categorias
  que o eixo já rotula.
- **Semântica de status num lugar só** (`statusTone` em `chart-theme.ts`). O donut e o selo do feed
  leem daí, então "Rejeitado" não pode ser vermelho num sítio e neutro noutro. Mapeia o nome do
  membro do enum, não o rótulo em pt-BR: inferir tom por palavra-chave no texto traduzido falha em
  "Vaga cancelada pela empresa" e quebra na primeira mudança de redação.

Decisões:

- **Uma consulta por seção**, com `keepPreviousData`: ao trocar o período o painel esmaece os números
  anteriores em vez de piscar para skeleton — num dashboard, é a diferença entre comparar períodos e
  perder o contexto a cada clique.
- **Recharts onde há eixo e escala** (evolução, distribuição por categoria, composição por status).
  Ranking geográfico, funil e tabela de empresas são HTML/CSS: têm melhor semântica, texto real e não
  precisam de eixo.
- **Um único gráfico circular** (status), onde a pergunta é de composição. Nas outras distribuições a
  pergunta é de ordenação, e a barra ganha.
- **Paleta em tokens CSS** (`--chart-1..6`, semânticos e cromo), definida nos dois temas em
  `globals.scss`. O Recharts escreve os tokens como atributos SVG, então o gráfico acompanha a troca
  de tema sem re-render.
- **Cauda longa agregada em "Outras"** acima de 8 categorias: 16 áreas e 27 UFs num gráfico só ficam
  ilegíveis.
- Toda seção tem `loading` (skeleton com a forma do conteúdo), `error` com retry, `empty` explicativo
  e `refreshing`. `app/(main)/dashboard/loading.tsx` mantém o shell montado na troca de rota.
- Datas trafegam como `yyyy-MM-dd` — dia civil. Mandar ISO com hora faria o navegador aplicar o fuso
  da máquina e o período pedido mudaria conforme quem abre a tela.
- Gráficos têm alternativa textual (`ChartA11yTable`), dentro de um `div.sr-only` — `width: 1px` num
  `display: table` é largura mínima, e a tabela esticava o `scrollWidth` do documento, fazendo a
  página rolar de lado no telefone.

---

## 7. Curadoria de informação (2026-08-23)

Auditoria de todos os elementos exibidos, classificados por valor para decisão. O critério não foi
"o dado existe?" — todos existiam — mas **"que decisão muda por causa deste elemento?"**. Onde a
resposta era "nenhuma", o elemento saiu.

### 7.1 Removido

| Elemento | Classificação | Motivo |
| -------- | ------------- | ------ |
| KPI "Candidatos" (total) | Irrelevante | Acumulado sem comparação: cresce sempre, não distingue um mês bom de um mau |
| KPI "Empresas cadastradas" (total) | Irrelevante | Idem |
| KPI "Usuários" (total) | Irrelevante | Idem, e é administração de sistema |
| KPI "Novos usuários" | Redundante | Crescimento da plataforma, não da operação; a maioria são candidatos, já em "novos candidatos" |
| KPI "Novas empresas" | Secundário → removido | Aquisição comercial. Vive na tela de empresas |
| KPI "Contas sem e-mail confirmado" | Irrelevante | Administração de sistema, não métrica de negócio |
| KPI "Vagas encerradas" | Irrelevante | Aproximação por `UpdatedAt` (qualquer edição a move) e ninguém age sobre ela |
| Série "Novos usuários" na evolução | Redundante | Quarta linha no mesmo eixo a repetir, noutra régua, o que "novos candidatos" já diz |
| Etapa "Vagas publicadas" no funil | Redundante | Unidade diferente, não produz conversão — a própria tela tinha de a rotular "contexto". O número continua no cartão |
| Bloco "Saúde das vagas" (4 contagens) | Redundante | "Ativas" repetia o cartão; "encerradas" é a aproximação acima; "sem movimento" e "sem candidatura" já são insights, com severidade e proporção |
| Painel "Distribuição geográfica" | Irrelevante para ação | "SP tem 42% das vagas" não gera decisão. O **filtro por UF** é a forma acionável da mesma informação |
| Painel "Empresas por volume" | Secundário → removido | Ranking de gestão; pertence à tela de empresas (§9 do briefing: o painel não substitui telas de gestão) |
| Painel "Base de usuários por perfil" | Irrelevante | Composição administrativa, sem ação associada |
| Painel "Atividade recente" | Secundário → removido | Feed de candidaturas individuais é a tela de candidaturas. Um painel executivo não lista registos |
| Bloco "Notas sobre os dados" | Informação técnica | Limitação de domínio é linguagem de implementação: vive neste documento e nos tooltips |
| Chips de filtros aplicados no cabeçalho | Redundante | Repetiam a barra de filtros imediatamente abaixo |
| "Comparando com \<intervalo\>" no cabeçalho | Redundante | Passou ao tooltip do cartão, junto à variação que explica |
| Texto "Sem base de comparação" no cartão | Informação técnica | Sem comparação, o cartão simplesmente não mostra a linha |
| Texto "Valor acumulado, não depende do período" | Informação técnica | Passou ao tooltip |
| Alternância Gráfico/Tabela nas empresas | Redundante | Duas vistas do mesmo dado; o painel saiu inteiro |
| Plano secundário de KPIs (`tier` + `<details>`) | Mecanismo sem função | Existia para domar treze cartões; com seis, não há o que esconder |

### 7.2 Mantido, e a pergunta que responde

| Elemento | Pergunta de negócio |
| -------- | ------------------- |
| 5–6 KPIs (vagas ativas, candidaturas, novos candidatos, vagas publicadas, taxa de aprovação, empresas ativas) | Como está a operação? |
| Evolução (3 séries, granularidade alternável) | O que mudou, e em que ritmo? |
| Funil (candidaturas → aprovação → conclusão) | Onde o processo perde gente? |
| Status das candidaturas (donut) | Em que etapa está o volume agora? |
| Concentração por área (barras, procura/oferta) | Que áreas puxam a operação? |
| Performance das vagas (lista, 3 ordenações) | Que vagas funcionam, e quais precisam de intervenção? |
| Insights (4 categorias, por severidade) | Onde tenho de olhar primeiro? |
| Filtros (período, empresa, UF, área, status) | Todos os recortes acima, sobre qualquer corte |

### 7.3 Efeito colateral: consultas poupadas

A curadoria não foi só visual. Por carregamento do painel:

| Endpoint | Antes | Depois |
| -------- | ----- | ------ |
| `overview` | 11 consultas | **6** |
| `distribution` | 7–8 agregações | 3 |
| `trends` | 3–4 séries | 3 |
| `jobs` | 7 agregações | 3 |
| `activity` | 1 | endpoint removido |

O ganho em `overview` veio da limpeza que seguiu a curadoria (§7.4): `GetTotalsAsync` calculava seis
contagens e a tela lia duas; `GetComparedCountersAsync` calculava seis e a tela lia três — e o
predicado de vagas carregava um `OR` sobre `UpdatedAt` que existia só para contar encerramentos.

### 7.4 Limpeza de código morto

Depois da curadoria, o código que alimentava o que saiu ficou sem chamador. Foi removido, não deixado
para trás — um método público sem consumidor é dívida, não capacidade.

**Backend** — cinco métodos do repositório (`GetJobsByStateAsync`, `GetCandidatesByStateAsync`,
`GetTopCompaniesAsync`, `GetUsersByTypeAsync`, `GetNewUsersDailySeriesAsync`), o endpoint `activity`
inteiro (action, request, handler, query, view model, validador, `GetRecentApplicationsAsync`), três
projeções (`DashboardCompanyVolume`, `DashboardEnumDistribution`, `DashboardCategoryCount`,
`DashboardRecentApplication`), seis campos de `DashboardTotals`, três de `DashboardPeriodCounters` e
duas lacunas de domínio que explicavam cartões já removidos (`UserActivity`, `ExactJobClosing`).

**Frontend** — `statusTone` (criado para o selo do feed de atividade, órfão quando o feed saiu),
`RankingSkeleton` e as suas classes SCSS, `hasActiveDashboardFilters` (API especulativa: nunca teve
consumidor de produção, só teste próprio) e seis exports de tipo sem uso.

Para restaurar qualquer bloco, a consulta tem de ser reescrita — está no histórico deste documento,
não no código.

---

## 8. Testes

**Backend** (`backend/tests/`, 273 verdes):

| Suite | Cobre |
| ----- | ----- |
| `Unit/Dashboard/DashboardPeriodResolverTests` | fronteiras em Brasília, intervalo semiaberto, `Custom` inválido, granularidade padrão |
| `Unit/Dashboard/DashboardSeriesBuilderTests` | zeros, recorte de semana/mês nas pontas, rótulos |
| `Unit/Dashboard/DashboardKpiFactoryTests` | percentagem só com base válida, tendência |
| `Unit/Dashboard/DashboardInsightsBuilderTests` | limiares, ausência de dados, categoria/título/severidade, alta vs. queda |
| `Unit/Dashboard/DashboardBreakdownFactoryTests` | percentagens, cauda longa, soma que não fecha |
| `Unit/Dashboard/GetDashboardOverviewHandlerTests` | funil (`Approved + Finished`), indicadores por escopo, plano principal por escopo, taxa nula sem candidatura, uma consulta por agregado |
| `Integration/Handlers/DashboardScopeAccessIntegrationTests` | RBAC com Identity real |

**Frontend** (`frontend/tests/`, 325 cenários verdes):
`dashboard-filters.feature` (recorte → query, chave de cache, período incompleto) e
`dashboard-format.feature` (pt-BR, sinal explícito, unidade).

**Lacunas conhecidas de cobertura:**

- O SQL gerado não é exercitado em teste. O provider InMemory não reproduz `date_part`, tradução de
  `GroupBy` nem semântica de fuso — foi exatamente por isso que a falha de tradução do ranking só
  apareceu na verificação pelo browser. Cobrir isso exige integração contra PostgreSQL real.
- Componentes React não são renderizados em teste (não há Testing Library no projeto); o
  comportamento visual é coberto por regressão pela UI.

---

## 9. Riscos e pontos de atenção

| Risco | Mitigação atual |
| ----- | --------------- |
| Volume de consultas do `overview` (9 agregações) | Output Cache de 300 s; todas indexadas e sem transferência de linhas |
| Horário de verão voltar ao Brasil | Deslocamento único por janela: erro de fronteira num dia de transição, não de total. Documentado no `DashboardFilter` |
| Aproximação de "vagas encerradas" | Declarada no `hint` do cartão e em `meta.unavailable` |
| `status` não recortar tudo | Declarado em `meta.appliedFilters` com o alcance explícito |
| Crescimento da tabela de candidaturas | Avaliar índice em `AppliedAt` com medição antes de aplicar |
