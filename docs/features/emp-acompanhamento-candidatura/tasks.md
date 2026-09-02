---
version: 1.0.0
date: 2026-09-01
status: Approved
---

# Tasks — Acompanhamento da candidatura pelo candidato

**feature-id:** `emp-acompanhamento-candidatura` · **PRD:** [`prd.md`](prd.md) v1.1.0 · **Design:** [`design.md`](design.md) v1.0.0 · **Spec:** [`spec.md`](spec.md) v1.0.0

Sequenciado para o **cancelamento poder ser entregue antes da notificação**: o Bloco 2 é utilizável por si
só (é o controle que o candidato hoje não tem), e nada nele depende da infra de eventos.

Restrição de release que atravessa todos os blocos: **API e frontend vão no mesmo deploy** (design §2.1 — o
`z.enum` do frontend rejeita status desconhecido).

## Bloco 0 — Pré-requisito de ambiente

| # | Tarefa | Camada |
| - | ------ | ------ |
| 0.1 | Sender de e-mail de desenvolvimento que escreve assunto/corpo/destinatário em log (ou ficheiro), substituindo `NoOpEmailSender` quando `Smtp:Enabled=false`. Sem isto nenhum critério de notificação é observável em dev | Infra |
| 0.2 | Registar no `docs/README.md` (ou no appsettings de exemplo) como ligar esse sender | Docs |

Sem dependências. Pode ser feito em paralelo com o Bloco 1.

## Bloco 1 — Domínio

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 1.1 | `ApplicationStatusEnum`: acrescentar `CanceledByCandidate` **no fim**, com comentário a explicar que a ordem é contrato de dados (coluna `integer`, sem valores explícitos) | Domain | — |
| 1.2 | `Pending`: `Description` passa a "Recebida" | Domain | CA-08b |
| 1.3 | `JobApplication`: construtor nasce em `Pending` | Domain | CA-08b |
| 1.4 | `JobApplication.CancelByCandidate()` com guarda de estados permitidos (Pending, Processing) | Domain | CA-10, CA-11 |
| 1.5 | `ChangeStatus`: recusar `CanceledByCandidate` como destino e recusar qualquer transição a partir dele | Domain | CA-11, RBAC-2 |
| 1.6 | Testes de agregado (`Unit/Jobs/JobApplicationAggregateTests`): status inicial, `[Theory]` de estados canceláveis e não canceláveis, guardas de `ChangeStatus` | Testes | CA-04, CA-08b, CA-10, CA-11 |

## Bloco 2 — Cancelamento ponta a ponta (entregável independente)

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 2.1 | `CancelJobApplicationCommand` + handler: carrega a candidatura, verifica posse (`IHttpCurrentUser.UserId`), recusa staff de recrutamento, chama `CancelByCandidate`, persiste | Application | CA-10, CA-13 |
| 2.2 | Validator do comando | Application | — |
| 2.3 | `PUT /api/jobapplications/{id}/cancel` no controller; **404 uniforme** para inexistente e alheia | Api | CA-13 |
| 2.4 | `IJobApplicationRepository`: `ExistsAsync` → `ExistsActiveAsync`, ignorando `CanceledByCandidate`; ajustar `ApplyToJobHandler` | Infra + Application | CA-14 |
| 2.5 | Frontend `domain/application-status.ts`: `CanceledByCandidate` em `APPLICATION_STATUSES`, rótulo por perfil, ícone, `transitions: []` | Frontend | CA-15 |
| 2.6 | Service da feature: `cancelJobApplication(id)` com contrato tipado + invalidação da query da lista | Frontend | CA-10 |
| 2.7 | `/candidaturas`: acção "Cancelar candidatura" visível só em Recebida/Em análise + modal de confirmação que diz que não há retorno + toast | Frontend | CA-10, CA-11, CA-12 |
| 2.8 | Testes: `CancelJobApplicationHandlerTests`, `Integration/Api/JobApplicationsCancelEndpointTests`, `Integration/Handlers/ApplyToJobHandlerTests` (recandidatura) | Testes | CA-10, CA-13, CA-14 |
| 2.9 | BDD: `my-applications-cancel.feature`, `application-status-vocabulary.feature` | Testes | CA-11, CA-12, CA-15 |

> N7 (e-mail de confirmação do cancelamento) **não** entra aqui — depende do Bloco 4. O handler do 2.1 já
> enfileira o evento; sem o Bloco 4 ninguém o consome, e isso é inofensivo.

## Bloco 3 — Despacho de eventos após commit

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 3.1 | `IDomainEventQueue` (`Enqueue`/`Drain`) | Application | — |
| 3.2 | `DomainEventQueue` *scoped* | Infra | — |
| 3.3 | `NotificationDispatchBehavior`: após `next()` com sucesso, drena e faz `Publish`; registado **antes** de `TransactionBehavior` em `DependencyInjection` | Infra | CA-03 |
| 3.4 | **Confirmar a composição real** dos behaviors (`Mediator.cs:62`) — se a ordem de registo não colocar o despacho fora da transacção, parar e registar em *Deviation notes* antes de contornar | Infra | CA-03 |
| 3.5 | `Unit/Behaviors/NotificationDispatchBehaviorTests`: commit → drena; **rollback → não drena** | Testes | CA-03 |

## Bloco 4 — Notificação por e-mail

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 4.1 | Evento `JobApplicationStatusChanged` com `Reason` (Applied, StatusChanged, JobClosed, CanceledByCandidate) | Application | — |
| 4.2 | `AppUrlsOptions.ApplicationsPath` (`/candidaturas`) | Application | — |
| 4.3 | `IJobApplicationEmailService` + `JobApplicationEmailModel` | Application | — |
| 4.4 | Templates em `EmpregaNetEmailTemplates`: assunto e corpo por `Reason`/status, com vaga, empresa, status e data (W4) | Infra | CA-01 |
| 4.5 | `JobApplicationEmailService` delegando a `IEmailSender`, **sem** passar por `IEmailThrottleService` | Infra | CA-09 |
| 4.6 | `JobApplicationStatusChangedEmailHandler`: projecção (vaga → título/empresa; candidato → e-mail/nome), monta o payload a partir do agregado, **captura toda excepção e registra** | Application | CA-03, CA-08 |
| 4.7 | Enfileirar o evento em `ApplyToJobHandler` (N1) e em `ChangeJobApplicationStatusCommandHandler` (N2–N5) | Application | CA-01, CA-02, CA-05 |
| 4.8 | Frontend: texto do feedback ao candidatar-se deixa de afirmar que a empresa foi notificada | Frontend | CA-05 |
| 4.9 | Testes: handlers (evento enfileirado por status), `JobApplicationStatusChangedEmailHandlerTests` (falha não relança; payload do agregado), `JobApplicationEmailServiceTests` (throttle não invocado), template | Testes | CA-01…CA-04, CA-08, CA-09 |
| 4.10 | BDD: `job-application-apply-feedback.feature` | Testes | CA-05 |

## Bloco 5 — Encerramento de vaga (V1–V3)

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 5.1 | `CloseJobHandler`: na mesma transacção, mover candidaturas em `Pending`/`Processing` para `Canceled` e enfileirar um evento `JobClosed` por candidatura; não tocar em Approved/Finished/Rejected/canceladas | Application | CA-06 |
| 5.2 | Resposta do endpoint passa a `{ jobId, closedAt, affectedApplications }` | Api | CA-17 |
| 5.3 | `JobViewModel.openApplicationsCount` (Pending + Processing) | Application | CA-17 |
| 5.4 | Frontend: contrato tipado da nova resposta; badge de estado da vaga; "Encerrar vaga" só em vaga activa; modal de confirmação com a contagem | Frontend | CA-17, CA-18 |
| 5.5 | Testes: `Integration/Handlers/CloseJobHandlerTests` (N abertas → N eventos; outras intocadas), `Unit/Jobs/GetJobByIdHandlerTests` (contagem) | Testes | CA-06, CA-17 |
| 5.6 | BDD: `job-close-confirmation.feature` | Testes | CA-17, CA-18 |

## Bloco 6 — Dashboard

| # | Tarefa | Camada | CA |
| - | ------ | ------ | -- |
| 6.1 | `BuildFunnel`: etapa "Candidaturas" exclui `Canceled` e `CanceledByCandidate`; actualizar o `<remarks>` com a razão | Application | CA-16 |
| 6.2 | `Unit/Dashboard/GetDashboardOverviewHandlerTests` | Testes | CA-16 |
| 6.3 | Avisar quem lê o dashboard de que a base do funil muda (risco de negócio do PRD §8) | Comunicação | — |

## Bloco 7 — ADRs

| # | Tarefa |
| - | ------ |
| 7.1 | ADR: notificações de andamento fora do teto anti-abuso de e-mail — qualifica o [ADR 0003](../../sdd/adrs/0003-teto-diario-de-emails-por-destinatario.md) |
| 7.2 | ADR: despacho de eventos de domínio após commit — onde passam a viver os efeitos colaterais não-transaccionais |

## Bloco 8 — Verificação

| # | Tarefa |
| - | ------ |
| 8.1 | Build + suíte completa (backend e frontend) — **feito para os Blocos 0–2**: backend 362 aprovados / 0 falhas, frontend 398 cenários / 1171 steps verdes |
| 8.2 | Regressão pela UI real com a [`e2e-qa-skill`](../../../.claude/skills/e2e-qa-skill/SKILL.md), com o sender de dev activo: candidatar-se → receber N1 → recrutador inicia análise → receber N2 → cancelar → receber N7 → recrutador vê "cancelada pelo candidato" → recandidatar-se  — **feita para o Bloco 2** (sem as partes N1/N2/N7, que dependem dos Blocos 3–4): 8 cenários aprovados pela UI + E2E-REG-008 verificado na API (relatório de 2026-09-01 22:14 em `docs/qa/`, não versionado) |
| 8.3 | Revisão de diff com o agent `code-reviewer` antes do merge — **feita**; 1 Bloqueante (índice único), 2 Importantes e 4 Menores, todos corrigidos |

## Delegação sugerida

| Blocos | Agent |
| ------ | ----- |
| 1, 3, 4 (backend), 5 (backend), 6 | `dotnet-implementer` |
| 2 (backend) | `dotnet-implementer` |
| 2 (frontend), 4.8, 5.4 | `frontend-engineer` |
| Testes de cada bloco | quem implementa o bloco; lacunas com `test-engineer` |
| 8.2 | skill `e2e-qa-skill` |
| 8.3 | agent `code-reviewer` |

## Adiado (do design §10)

| Item | Gatilho de retorno |
| ---- | ------------------ |
| Máquina de estados completa das transições do recrutador | Transição incorrecta em produção, ou consumidor da API fora do frontend |
| *Outbox* persistente de notificações | Perda de notificação com impacto reportado |
| Abstracção de canal (WhatsApp, in-app) | Existência do segundo canal |
| Coluna de autoria/histórico de transições | Necessidade de auditar *quem* mudou o status |
| Título da vaga e empresa em `/candidaturas` (BUG-03 do QA) | Fora desta feature — defeito pré-existente, corrigir em tarefa própria |

## Deviation notes

> Preencher durante a implementação: ajustes feitos face ao design, com a razão. Manter rastreio honesto —
> divergência registada é informação; divergência silenciosa é dívida.

### Bloco 2 — frontend (2.5, 2.6, 2.7, 2.9) · `frontend-engineer`

| # | Ajuste face ao design | Razão |
| - | --------------------- | ----- |
| 2.5 | O "rótulo por perfil" ficou como função `applicationStatusLabel(status, audience)` + mapa de excepções, e não como dois mapas completos. `applicationStatusLabels` continua a existir com a redação do recrutador (usado por dashboard e filtros de recrutamento) | Só um status muda de redação; duplicar o mapa inteiro faria cada status novo ter de ser escrito duas vezes, e a segunda cópia envelhece em silêncio |
| 2.5 | `ApplicationStatusBadge` ganhou a prop opcional `audience` (padrão `recruiter`); `/candidaturas` passa `candidate` | O badge é partilhado por quatro telas, três delas do recrutador. Prop obrigatória obrigaria a tocar telas fora do bloco sem ganho |
| 2.5 | `CanceledByCandidate` recebeu tom **neutro** no badge (`Canceled` continua negativo) | Desistência do próprio candidato não é recusa da empresa; pintá-la de vermelho na lista do candidato lê-se como reprovação |
| 2.5 | Entradas obrigatórias de `applicationTransitionLabels`/`Icons` para `CanceledByCandidate` (nunca renderizadas, porque o status não é destino de nenhuma transição) | Os mapas são `Record` total; torná-los `Partial` degradaria a indexação para `string \| undefined` em todos os consumidores do recrutador |
| 2.6 | `useCancelMyApplicationMutation` invalida também `jobsFeedKeys.all`, além de `jobApplicationsKeys.all` que o design pedia | O "já candidatado" do cartão de vaga vem de `jobsFeedKeys.interactions`, cache distinta. Sem isso, X4 (recandidatar-se) ficaria bloqueado na UI até o próximo refetch, mesmo com a API a permitir |
| 2.7 | O modal reutilizado é o `ConfirmDialog` canónico; **não** se usou `FormSubmitButton` | Não há formulário nesta acção: o `ConfirmDialog` já desabilita o botão, mostra `Spinner` e marca `aria-busy` — que é o que o `FormSubmitButton` traria |
| 2.7 | O botão de recusa da confirmação chama-se **"Manter candidatura"**, não "Cancelar" | No contexto, "Cancelar" significaria justamente cancelar a candidatura. Ambiguidade num ato terminal (X5) |
| 2.7 | O texto da confirmação vive em `my/cancel-application-dialog-copy.ts`, fora do componente | X5 é regra de produto verificável; separado, o BDD confirma que a confirmação diz que a acção não tem retorno, sem renderizar React |
| 2.9 | `application-status-vocabulary.feature` ficou em `tests/specs/unit/` e `my-applications-cancel.feature` em `tests/specs/integration/` | Segue a `spec.md`: vocabulário é lógica pura; o cancelamento atravessa regra → confirmação → HTTP → contrato de leitura |
| 2.9 | Os steps de integração substituem **só** `axiosApi.put` (instalado no `Before`, restaurado no `After`); `cancelJobApplication` e o parse Zod da resposta correm de verdade | O Cucumber deste projecto não renderiza React (não há Testing Library). Trocar o transporte mantém o contrato sob teste; simular o service inteiro testaria a simulação. O duplo é restaurado por cenário porque `axiosApi` é módulo único do processo — sobrevivendo, um teste futuro passaria contra esta resposta em vez da chamada real |

**Não feito, por estar fora do bloco delegado:** 4.8 (texto do feedback ao candidatar-se — o toast de
`useApplyToJobMutation` continua a afirmar que a empresa foi notificada) e 5.4 (encerramento de vaga).

**Verificação (frontend):** `pnpm --dir frontend lint` sem erros; `pnpm --dir frontend test` 398 cenários /
1171 steps, todos verdes; `pnpm --dir frontend build` verde, `/candidaturas` mantém-se `○` (estático).

### Blocos 0, 1 e 2 — backend (0.1, 0.2, 1.1–1.6, 2.1–2.4, 2.8) · `dotnet-implementer`

| # | Ajuste face ao design | Razão |
| 0.1 | O sender de desenvolvimento escreve **em log**, não em ficheiro, e chama-se `DevelopmentLogEmailSender` | O log já é o canal observável do processo em dev; um ficheiro acrescentaria caminho a configurar, rotação e limpeza sem tornar nada mais verificável |
| 0.1 | O log fica **preso a `Development`** (`builder.Environment.IsDevelopment()`), e não ao `else` da condição de SMTP; os outros ambientes sem SMTP mantêm o `NoOpEmailSender` | *(correcção após revisão)* Staging, homologação e QA com `Smtp:Enabled=false` caíam no mesmo `else` e ficavam com o sender de log activo. O corpo do e-mail carrega **tokens vivos** de reset de senha e de confirmação de conta: um link registado é um link utilizável por quem leia o log, e o Sentry deste projecto colecciona breadcrumbs a partir de `Information` |
| 0.1 | Destinatário e assunto em `LogInformation`; **corpo HTML só em `LogDebug`** | *(correcção após revisão)* Mesmo em `Development`, o corpo não precisa de sair por omissão. Em `Debug`, ver o token exige baixar o nível de propósito, na própria máquina — e o `docs/README.md` diz como |
| 0.1 | `NoOpEmailSender` era o tipo do framework (`Microsoft.AspNetCore.Identity.UI.Services`), não uma classe do repositório | Registado para quem procurar o ficheiro do no-op e não o encontrar |
| 1.5 | As duas guardas novas de `ChangeStatus` lançam `InvalidOperationException`, como as guardas que já existiam no método | Manter uma só forma de recusa no agregado; a tradução para o erro HTTP é responsabilidade da Application |
| 1.6 | Acrescentado `ApplicationStatusEnum_OrdemDosValores_DeveSerEstavel`, que fixa o valor numérico de **todos** os membros | O design diz que a ordem é contrato de dados mas só a registava num comentário. Um comentário não falha o build; este teste falha assim que alguém inserir um valor no meio, que é a única forma de reescrever em silêncio o status de linhas já gravadas |
| 1.5 / 2.1 | **`ChangeJobApplicationStatusHandler` também traduz** a recusa do agregado para `ValidationAppException` com `INVALID_ACTION_FOR_STATUS` — ficheiro fora do escopo nominal do bloco | *(correcção após revisão)* As guardas novas da tarefa 1.5 vivem no caminho do **recrutador**, e ali `ChangeStatus` não estava envolvido: quem tentasse mover uma candidatura cancelada pelo candidato recebia `409 "Operação inválida."`, código que o endpoint nem declara. A tarefa 1.5 não fica completa sem isto |
| 2.1 | O handler traduz a `InvalidOperationException` de `CancelByCandidate()` para `ValidationAppException` com `INVALID_ACTION_FOR_STATUS`, em vez de repetir a lista de estados canceláveis antes de chamar o agregado | `InvalidOperationException` mapeia para **409** no `GlobalExceptionHandler`, e o design fixa **400** com `INVALID_ACTION_FOR_STATUS`. Repetir a lista no handler duplicaria a regra e deixá-la-ia derivar; o `catch` estreito mantém o agregado como fonte única |
| 2.1 | "Não encontrada" usa `NotFoundException`, e não `ValidationAppException` com `RESOURCE_ID_NOT_FOUND` como faz `ChangeJobApplicationStatusCommandHandler` | No `GlobalExceptionHandler`, `ValidationAppException` devolve sempre **400**, seja qual for o `Code`. O design exige **404**, e só `NotFoundException` o produz. O handler de mudança de status tem o defeito oposto (declara 404 no `ProducesResponseType` e devolve 400) — é pré-existente e fica fora deste bloco |
| 2.1 | Mensagem única (`"Candidatura não encontrada."`) para inexistente e alheia, numa constante do handler | A uniformidade do 404 não é só o código: mensagens diferentes voltariam a distinguir os dois casos no corpo da resposta |
| 2.1 | A recusa a perfis de recrutamento acontece **antes** da leitura da candidatura, e devolve 400 (`INVALID_ACTION_FOR_RECORD`), não 404 | Segue o padrão de `ApplyToJobHandler`. Recusar por papel não revela nada sobre o id, por isso não precisa do 404 uniforme |
| 2.4 | Além de `ExistsAsync` → `ExistsActiveAsync`, **`GetAppliedJobIdsAsync` passa a excluir `CanceledByCandidate`** — não estava no design | É a consulta que faz o feed mostrar "Já candidatado". Sem o mesmo filtro, a API aceitaria a recandidatura (CA-14) e a UI continuaria a bloquear o botão na vaga de que o candidato desistiu: o critério ficaria verde no backend e falso na tela |
| 2.4 | O predicado de "activa" ficou numa `static readonly Expression<Func<JobApplication, bool>> IsActiveApplication`, partilhada pelas duas consultas | *(sugestão da revisão, aceite)* Quem decide se pode candidatar-se e quem decide se o feed mostra o botão têm de responder o mesmo; com a condição escrita duas vezes, a próxima regra entra só numa delas. `Expression` e não `Func` para o EF traduzir a SQL |
| 2.8 | `Integration/Api/JobApplicationsCancelEndpointTests` **não sobe servidor HTTP**: exercita o handler sobre o repositório e o EF reais, e faz a excepção atravessar o `GlobalExceptionHandler` para conferir código e corpo | O projecto de testes não tem `Microsoft.AspNetCore.Mvc.Testing` nem `WebApplicationFactory`, e o arranque real exige PostgreSQL, Redis e JWT. Fica de fora o roteamento MVC e o `[Authorize]` da classe; tudo o que **decide** o código de estado está coberto. Acrescentar a dependência era decisão de fronteira, não de implementação |
| 2.8 | Os ficheiros de integração usam o sufixo `IntegrationTests` (`ApplyToJobHandlerIntegrationTests`, `GetMyJobApplicationsHandlerIntegrationTests`, `GetJobApplicationsByJobIdHandlerIntegrationTests`), e não o nome exacto da `spec.md` | Convenção já usada em `tests/Integration/Handlers/` |
| 2.8 | Testes de `Canceled` (ato da empresa) tratam-no como estado não cancelável pelo candidato | Consequência directa de X1/X2: só `Pending` e `Processing` permitem cancelar |
| 2.8 | Acrescentados após a revisão: `GetMyJobApplicationsHandlerIntegrationTests` (filtro `Pending` devolve resultados — CA-08b), `GetJobApplicationsByJobIdHandlerIntegrationTests` (os dois cancelamentos distinguem-se na leitura — CA-15), staff de recrutamento recusado em `JobApplicationsCancelEndpointTests` (CA-13, agora também na integração) e `Unit/JobApplications/ChangeJobApplicationStatusHandlerTests` (a tradução do 409 para 400) | *(correcção após revisão)* Os três primeiros estão nomeados na `spec.md` e faltavam sem estar declarados como adiados — que é exactamente a divergência silenciosa que este documento existe para evitar. O quarto cobre a mudança de comportamento introduzida na correcção do 409 |
| 2.4 | **Índice único `(JobId, UserId)` passa a parcial** (`HasFilter("\"Status\" <> 9")`) + migration `20260902003229_IndiceUnicoParcialDeCandidatura` | *(bloqueante da revisão; decisão do utilizador)* Permitir a recandidatura no código não bastava: `IX_JobApplications_JobId_UserId` era **total e único**, e a segunda candidatura à mesma vaga violava-o no PostgreSQL. Com o filtro, coexistem N canceladas pelo candidato e no máximo 1 activa — que é X4 (recandidatura) e X6 (histórico preservado) ao mesmo tempo |
| 2.4 | A migration só faz `DropIndex` + `CreateIndex`; verificado que **não** gera `rename`/`drop` de coluna nem de tabela | Forward-only é seguro aqui: recriar um índice não destrói dados, e o `Down` repõe o índice total |
| 2.4 | O literal `9` do filtro fica amarrado ao enum por teste, não por confiança | O filtro é **texto**: o compilador não o verifica, e a coluna é `integer` sobre um enum sem valores explícitos, portanto o número é a *posição* do membro. `FiltroDoIndice_DeveExcluirExactamenteOStatusCanceladoPeloCandidato` compara-o com `(int)ApplicationStatusEnum.CanceledByCandidate`, e falha em conjunto com `ApplicationStatusEnum_OrdemDosValores_DeveSerEstavel` se alguém inserir um valor no meio |
| 2.8 | Novo `Unit/Persistence/JobApplicationIndexTests` afirma o índice **ao nível do modelo do EF**, não contra uma base real; `ApplyToJobHandlerIntegrationTests` ganhou um `<remarks>` a declarar o que **não** prova | *(bloqueante da revisão)* Foi o provider InMemory que escondeu o defeito: ele não aplica índices, por isso o teste da recandidatura passava e continuaria a passar com o índice total. O teste de modelo cobre "a configuração declara o índice certo e é dela que a migration deriva"; **não** cobre "o PostgreSQL aceita mesmo a segunda linha". Testcontainers seria a cobertura real — é decisão de fronteira e **não** foi tomada aqui |

**Não feito, por estar fora do bloco delegado:** nenhum evento de domínio é enfileirado pelo
`CancelJobApplicationHandler` — `IDomainEventQueue`, `NotificationDispatchBehavior` e o e-mail N7 são os
Blocos 3 e 4. O cancelamento funciona ponta a ponta sem notificação; quando o Bloco 4 entrar, o ponto de
enfileiramento é logo a seguir ao `UpdateAsync` do handler.

**Achado fora de escopo, não corrigido — corrigido na redacção após verificação:**
`backend/src/EmpregaNet.Api/appsettings.Development.json` tem credenciais SMTP reais (host, utilizador e
palavra-passe do Brevo) **no ficheiro local**. A primeira redacção desta nota dizia "versionadas", o que está
**errado**: o ficheiro é ignorado por `backend/.gitignore:6` (`appsettings*.json`), `git check-ignore`
confirma-o, e `git log --all` não devolve nenhum commit que o inclua. **Não há secret no repositório nem no
histórico.** A recomendação que se mantém é migrar para User Secrets; rotação só é necessária se o ficheiro
tiver circulado fora da máquina.

**Consequência prática que essa configuração tem para esta feature:** com `Smtp:Enabled=true` e `Host`/
`FromEmail` preenchidos, a condição em `DependencyInjection.cs` resolve para `SmtpEmailSender` — ou seja,
nesta máquina o `DevelopmentLogEmailSender` do Bloco 0 **não** é usado, e testar os Blocos 3/4 localmente
enviaria e-mail real. Pôr `Smtp:Enabled=false` no ficheiro local antes de implementar a notificação.

**Pergunta deixada em aberto pela revisão — `IsDeleted` no filtro do índice. Escolhida a opção
conservadora: ficou de fora.** O filtro é só `"Status" <> 9`, portanto uma candidatura *soft-deleted*
continua a ocupar o par `(JobId, UserId)` e a bloquear nova candidatura — exactamente como antes desta
feature. Incluir `IsDeleted` mudaria comportamento pré-existente que nem o PRD nem o design pediram, e o
`DELETE` de candidatura é do recrutamento, não do candidato: alterá-lo não é decisão desta feature.

**Risco pré-existente que a análise expôs e que não foi corrigido:** `ExistsActiveAsync` (como o
`ExistsAsync` antes dele) filtra `!a.IsDeleted`, mas o índice não. Ou seja, se o recrutamento apagar
logicamente uma candidatura e o candidato se candidatar de novo, **o código autoriza e o banco recusa** com
violação de índice único. É o mesmo tipo de divergência código-vs-índice que a revisão apanhou, noutro
caminho, e é anterior a este trabalho. *Gatilho de retorno:* primeira violação de
`IX_JobApplications_JobId_UserId` em log de produção, ou tarefa que torne o `DELETE` de candidatura
alcançável pelo fluxo normal.

**Verificação (backend), após aplicar a revisão e o índice parcial:** `dotnet build backend/EmpregaNet.sln`
verde — 0 erros, 2 avisos `CS1573` pré-existentes em `StringHelper.cs`, iguais aos do baseline medido antes
de tocar em nada. `dotnet test backend/tests/tests.csproj` — **362 aprovados, 0 falhas, 0 ignorados** (52
deles novos neste bloco: 38 na primeira leva, 12 da revisão, 2 do índice parcial). Ambos corridos com
`BaseOutputPath` a apontar para fora da árvore, para não colidir com o file lock dos DLLs da API em execução
na 5225 — a API não foi derrubada, e a migration foi gerada com a mesma técnica. `Bff/` não foi tocado, por
isso não foi construído.
