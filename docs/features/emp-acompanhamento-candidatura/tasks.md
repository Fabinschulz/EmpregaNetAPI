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
| 8.2 | Regressão pela UI real com a [`e2e-qa-skill`](../../../.claude/skills/e2e-qa-skill/SKILL.md), com o sender de dev activo: candidatar-se → receber N1 → recrutador inicia análise → receber N2 → cancelar → receber N7 → recrutador vê "cancelada pelo candidato" → recandidatar-se  — **feita para o Bloco 2** (sem as partes N1/N2/N7, que dependem dos Blocos 3–4): 8 cenários aprovados pela UI + E2E-REG-008 verificado na API (relatório de 2026-09-01 22:14). **N1–N7 verificados em 2026-09-09** com `Smtp:Enabled=false`: 9/9 aprovados, nenhum defeito — conteúdo mínimo do e-mail, aprovação e reprovação visualmente distintas, cascata do encerramento e contagem do modal iguais ao real. Relatórios em `docs/qa/`, não versionados |
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
| 2.9 | Os steps de integração substituem **só** `axiosApi.put`; `cancelJobApplication` e o parse Zod da resposta correm de verdade. *(Movido no Bloco 5 para `tests/support/axios-put-double.ts`, com instalação e restauro em `hooks.ts` — ver a nota 5.6.)* | O Cucumber deste projecto não renderiza React (não há Testing Library). Trocar o transporte mantém o contrato sob teste; simular o service inteiro testaria a simulação. O duplo é restaurado por cenário porque `axiosApi` é módulo único do processo — sobrevivendo, um teste futuro passaria contra esta resposta em vez da chamada real |

**Não feito, por estar fora do bloco delegado:** 5.4 (encerramento de vaga). A tarefa 4.8 estava aqui como
pendente e foi entregue depois, no bloco de frontend do Bloco 4, mais abaixo.

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

### Bloco 4 — frontend (4.8, 4.10) · `frontend-engineer`

| # | Ajuste face ao design | Razão |
| - | --------------------- | ----- |
| 4.8 | O feedback passou a **"Candidatura enviada · Acompanhe o andamento em 'Minhas candidaturas'."** — não menciona e-mail nem a empresa | A frase tem de ser verdadeira **hoje**, e hoje N1 ainda não existe (Bloco 4 backend em curso). Prometer o e-mail agora repetiria o defeito do PRD §1.2 com outro texto; e como o design §6 só exige "deixar de afirmar que a empresa foi notificada", a redação escolhida continua verdadeira depois de N1 entrar — não fica um segundo texto a lembrar de trocar |
| 4.8 | O texto vive em `src/features/candidaturas/apply-feedback-copy.ts`, fora do hook de mutação | Mesmo motivo da confirmação de cancelamento: é uma regra de produto verificável, e isolada permite ao BDD ler a redação sem renderizar React nem importar React Query |
| 4.10 | Os cenários verificam a **ausência de família de termos** e não a igualdade com uma frase. A lista para nos **atos de terceiros** (`notificad`, `avisad`, `comunicad`, `empresa`, `recrutador`); "e-mail" ficou fora dela, num cenário à parte que fixa a redação enquanto N1 não existe | *(correcção após revisão)* Igualdade de string trava a redação sem impedir a regressão real — o que não pode voltar é qualquer variante da promessa ("avisamos o recrutador", "a empresa já recebeu"). Banir "e-mail" na mesma lista contradizia a própria justificação: depois do Bloco 4, "enviamos um e-mail de confirmação" é verdade e reprovaria no teste. Separado, o cenário do canal diz o que é — uma decisão datada, que se remove **deliberadamente** quando N1 entrar, em vez de um teste a falhar por surpresa |

**Não feito, por estar fora do bloco delegado:** 5.4 (encerramento de vaga) e o corpo de 4.1–4.7 e 4.9, que
são backend.

**Verificação (frontend), depois de 4.8 e 4.10:** `pnpm --dir frontend lint` sem erros;
`pnpm --dir frontend test` **407 cenários / 1190 steps, todos verdes** (9 cenários novos);
`pnpm --dir frontend build` verde, `/candidaturas` mantém-se `○` (estático). Prettier corrido só nos
ficheiros tocados — `job-applications-queries.ts` levou reindentação dos blocos de import, que já estavam
fora do padrão do projecto. *(Contagem daquele momento; depois da correcção #4 da revisão, dois cenários do `job-application-apply-feedback.feature` fundiram-se num — a contagem corrente está na verificação do Bloco 5.)*

### Blocos 3 e 4 — backend (3.1–3.5, 4.1–4.7, 4.9) · `dotnet-implementer`

**Tarefa 3.4 — composição dos behaviors: a premissa do design confirma-se.** Verificado no código, não
presumido:

- `Mediator.RequestHandlerWrapperImpl` monta o pipeline iterando `GetServices` **de trás para frente**
  (`for (var i = behaviors.Count - 1; i >= 0; i--)`), logo o **primeiro registado é a camada mais externa**.
- `TransactionBehavior` só devolve depois de `UnityOfWork.ExecuteInTransactionAsync` ter corrido
  `SaveChangesAsync` **e** `CommitAsync`.

Portanto registar `NotificationDispatchBehavior` **antes** de `TransactionBehavior` põe-no de facto fora da
transacção, e o `await next()` dele observa o commit já feito. Nada a contornar.

| # | Ajuste face ao design | Razão |
| - | --------------------- | ----- |
| 3.2 | `DomainEventQueue` ficou em `Infra/Events/`, e não junto dos behaviors | É estado por requisição, não um passo do pipeline; misturá-los tornaria a pasta `Behaviors` um saco |
| 3.3 | `Publish` é invocado por **reflexão com o método fechado cacheado por tipo** | `IMediator.Publish<TNotification>` é genérico e a fila só conhece `INotification`; chamá-lo directamente resolveria `TNotification` para a interface e **nenhum handler concreto seria encontrado** — as notificações desapareciam em silêncio com todos os testes verdes. O cache estático por tipo é o mesmo idioma que o `Mediator` já usa para os wrappers de request |
| 3.3 | O behavior é registado **sem restrição de tipo**, logo também envolve queries | Numa query a fila está vazia e o `Drain()` sai em `O(1)`. Restringir a `ITransactional` obrigaria a duplicar o registo quando aparecer um comando não transaccional que precise de notificar |
| 3.3 | A publicação é envolvida em `try/catch` **por evento**, e o behavior nunca relança | Neste ponto o commit já aconteceu (W1/W2). Uma falha no primeiro evento também não deve impedir a publicação dos seguintes — no encerramento de vaga (Bloco 5) são N eventos e um destinatário com e-mail inválido não pode calar os outros |
| 3.5 | Acrescentado `Pipeline_ComMediatorReal_DeveEntregarAoHandlerConcretoDaNotificacao`, que atravessa o `Mediator` real via `AddMediator(assembly de testes)` | Os outros testes usam `IMediator` falsificado e não provariam a resolução do handler concreto — exactamente o ponto que a reflexão do 3.3 pode errar |
| 3.5 | Acrescentado `RegistoDosBehaviors_DeveColocarODespachoForaDaTransacao`, que afirma que o índice do despacho é **menor** que o da transacção | A ordem de duas linhas de DI é a única coisa que separa "e-mail depois do commit" de "e-mail dentro da transacção", e trocá-las não quebra compilação nem caminho feliz |
| 4.1 | `JobApplicationNotificationReason` ficou no mesmo ficheiro do evento | Não tem uso fora dele; ficheiro próprio para um enum de quatro membros só acrescenta navegação |
| 4.6 | A projecção é indexada pela **candidatura** (`GetNotificationProjectionAsync(applicationId)`), não pela vaga como o design sugeria (`jobId` → título/empresa) | Uma consulta em vez de duas, e satisfaz CA-08 mais fortemente: o destinatário sai da linha da candidatura, não de um id que o handler tenha de cruzar. Novo record `JobApplicationNotificationProjection` no Domain |
| 4.6 | `LEFT JOIN` (`DefaultIfEmpty`) em vaga, empresa e candidato | Mesma razão de `ProjectWithCandidate`: com INNER, qualquer inconsistência numa das pontas faria a notificação **não sair e nada no log dizer porquê**. Com LEFT o campo vem vazio e o handler decide (e regista) |
| 4.6 | O handler ignora e regista quando o candidato não tem e-mail, em vez de tentar enviar | O Identity permite utilizador sem e-mail; tentar enviar produziria uma excepção de transporte por candidatura, com o mesmo desfecho e mais ruído |
| 4.4 | O template reutiliza o `Build` existente, passando a lista de detalhes pelo parâmetro `body` | Evita um segundo layout de e-mail a divergir do primeiro. **Consequência que obrigou a cuidado:** `body` é injectado como HTML sem escape, por isso todo valor dinâmico passa por `WebUtility.HtmlEncode` no `JobApplicationStatus` — o título da vaga é texto que um recrutador escreveu e chega ao e-mail de todos os candidatos daquela vaga |
| 4.4 | `EmpregaNet.Infra.csproj` ganhou `<InternalsVisibleTo Include="tests" />` | `EmpregaNetEmailTemplates` é `internal` e o conteúdo do e-mail é o critério de aceite (CA-01). A alternativa era tornar a classe pública, o que a promoveria a API da Infra sem necessidade. Mesmo padrão já usado em `Api.csproj` |
| 4.9 | Fixado por teste que texto acentuado sai como **entidade numérica** (`Metal&#250;rgica`) | Descoberto ao escrever o teste: `WebUtility.HtmlEncode` escapa não-ASCII. É correcto e os clientes de e-mail renderizam — mas quem vir entidades no fonte pensa que é bug e "corrige" removendo o escape. O teste explica e impede |
| 4.9 | O teste de enfileiramento do `ChangeStatus` afirma `JobId`, estados e razão, **não** `JobApplicationId` | A entidade do teste unitário não passa pelo banco, logo `Id` é `0`. Em produção o handler usa `application.Id`, que é o id real; a cobertura do id efectivo está nos testes de integração |
| 4.9 | `JobApplicationEmailServiceTests` tem, além do `Times.Never` sobre o teto, um teste **estrutural** que afirma que o serviço não recebe `IEmailThrottleService` no construtor | `Times.Never` sobre um mock que ninguém injectou passa por vacuidade e continuaria a passar se alguém acrescentasse a dependência amanhã. O teste estrutural falha nesse momento — é ele que protege CA-09 |
| 4.7 | N1 usa `PreviousStatus = NaoSelecionado` | A candidatura não tinha estado anterior; `NaoSelecionado` é o membro-zero já existente e o consumidor decide pela `Reason`, não pelo estado anterior |

**Risco levantado — interacção entre o retry da execution strategy e a fila de eventos.**
`UnityOfWork.ExecuteInTransactionAsync` corre sob a execution strategy do Npgsql, e `DatabaseConfig.cs:30`
tem `EnableRetryOnFailure(maxRetryCount: 2)`; o `Drain` acontece uma vez, fora dela. Levantei isto como
"pode duplicar notificação". **A análise deste caminho ficou com o coordenador e o utilizador**, e a minha
descrição do mecanismo não é de confiar — não a tomem como diagnóstico fechado.

O que se mantém verdadeiro e verificado: se a operação acabar por falhar, o `Drain` não acontece e nada sai
(`NotificationDispatchBehaviorTests`). A fila com semântica de conjunto, acrescentada depois, é defesa em
profundidade sobre este e sobre qualquer outro caminho que declare o mesmo facto duas vezes na mesma
requisição — ver a secção própria mais abaixo.

**Não feito, por estar fora do escopo delegado:** N6 (encerramento de vaga arrasta candidaturas e notifica
uma por uma) é 5.1 e o `CloseJobHandler` continua intocado; 4.8 é frontend; Blocos 6 e 7 (funil do dashboard
e ADRs) não foram tocados. Os dois **ADRs candidatos** do design §9 continuam por escrever, e o segundo deles
— despacho de eventos após commit — passou de hipótese a mecanismo em produção nesta leva, portanto o custo
de o deixar sem registo aumentou.

**Verificação (backend, Blocos 3 e 4):** `dotnet build backend/EmpregaNet.sln` verde — 0 erros, 0 avisos.
`dotnet test backend/tests/tests.csproj` — **401 aprovados, 0 falhas, 0 ignorados** (39 novos nesta leva:
362 → 401). Corridos com `BaseOutputPath` fora da árvore, com a API a correr na 5225. `Bff/` não foi tocado.
`appsettings.Development.json` não foi alterado — com `Smtp:Enabled=true` nessa máquina o transporte é o SMTP
real, portanto verificar N1–N7 em dev exige desligar o SMTP localmente (ver `docs/README.md`).

### Fila de eventos com semântica de conjunto · `dotnet-implementer`

`Enqueue` passou a descartar o mesmo facto declarado duas vezes na mesma requisição. **Não afirmo que isto
resolve o retry da execution strategy** — a análise desse caminho está com o coordenador e o utilizador, e a
minha justificação anterior estava errada. O que esta mudança é, com segurança: **defesa em profundidade**.
A garantia primária de não duplicar continua a ser do domínio (`ChangeStatus` recusa transição para o status
actual; `CancelByCandidate` recusa a partir de estado não cancelável); a fila impede que uma segunda
declaração do mesmo facto, venha ela de onde vier, se torne um segundo e-mail.

| # | Ajuste | Razão |
| - | ------ | ----- |
| 3.1 / 3.2 | `Enqueue` com semântica de conjunto. Identidade vem de `IIdentifiableNotification.DeduplicationKey` quando o evento a declara, e da igualdade estrutural do record quando não | Declarar duas vezes o mesmo facto na mesma requisição nunca deve produzir dois efeitos externos, independentemente de como a segunda declaração aconteceu |
| 4.1 | `JobApplicationStatusChanged.DeduplicationKey` = `{JobApplicationId}:{NewStatus}:{Reason}`, com `OccurredAt` **fora** da identidade | O facto é "esta candidatura passou a este status, por esta razão"; o instante em que o objecto foi criado é acidental. Com `OccurredAt` na identidade, dois objectos que descrevem o mesmo facto não se reconheceriam como iguais |
| 3.1 | Interface `IIdentifiableNotification` em vez de parâmetro extra no `Enqueue` | A identidade do facto é conhecimento do evento, não de cada ponto de chamada. Com parâmetro, a regra ficaria copiada em quatro sítios e divergiria |
| 3.5 | Novo `Unit/Behaviors/DomainEventQueueTests` (8 testes) | Cobre o descarte do repetido e, sobretudo, o que **tem** de continuar a passar: candidaturas diferentes, status diferente e razão diferente são factos distintos — senão o encerramento de vaga notificaria uma pessoa só |
| 3.5 | `RecordingDomainEventQueue` (Support) alinhada à mesma semântica, lendo a identidade do próprio evento | Se o duplo de teste aceitasse repetidos, os testes dos handlers passariam a contradizer a produção |

**Verificado que não existe caso legítimo de dois eventos com a mesma chave na mesma requisição:**
`ChangeStatus` recusa transição para o status actual, `CancelByCandidate` recusa a partir de estado não
cancelável, e no `JobClosed` cada evento tem `JobApplicationId` próprio.

### Blocos 5, 6 e 7 — backend (5.1–5.3, 5.5, 6.1, 6.2, 7.1, 7.2) · `dotnet-implementer`

| # | Ajuste face ao design | Razão |
| - | --------------------- | ----- |
| 5.1 | `JobApplication.OpenStatuses` passa a ser a **definição única** de "processo em aberto", usada pelo cancelamento do candidato, pelo arrasto do encerramento e pela contagem de CA-17 | As três regras usavam a mesma dupla `Pending`/`Processing` e não por coincidência: o candidato pode desistir exactamente enquanto não há desfecho, e é isso que o encerramento arrasta. Três literais separados divergiriam quando aparecer um estado novo |
| 5.1 | O handler carrega as candidaturas **rastreadas** (`GetOpenByJobIdAsync` sem `AsNoTracking`, contra a convenção de leitura do repositório) e muda o estado pelo agregado | `ExecuteUpdateAsync` faria um `UPDATE` só e seria mais rápido, mas passaria ao lado de `ChangeStatus` — a guarda que impede a empresa sobrepor uma desistência do candidato. A guarda vale mais que a instrução única, e está documentado nos dois lados |
| 5.1 | **Escala:** uma consulta para as candidaturas em aberto (sem N+1) e N `UPDATE` na mesma transacção, com `LogWarning` acima de 200 candidaturas | Ver nota de escala abaixo — não assumi que está bem |
| 5.1 | Retirado o `try/catch (Exception) { LogError; throw; }` que envolvia todo o handler | Transformava recusas esperadas (vaga inexistente, vaga já encerrada) em `LogError`, e o `GlobalExceptionHandler` já registra. Alteração num ficheiro que esta tarefa reescreveu, registada aqui por não ser pedida |
| 5.2 | `CloseJobCommand` passa de `IRequest<bool>` para `IRequest<CloseJobResult>`; o record vive no mesmo ficheiro do comando | Convenção local (comando + handler no mesmo ficheiro). **Contrato HTTP quebrado** — ver aviso ao frontend abaixo |
| 5.3 | A contagem é resolvida em `GetJobByIdHandler` (que ganhou `IJobApplicationRepository`), **só no detalhe** | `JobViewModel` é partilhado com as listagens; contar por linha seria uma consulta agregada por vaga listada. Nas listagens o campo fica `0`, e o `<remarks>` do campo diz isso para ninguém o ler como "sem candidaturas" |
| 5.5 | `CloseJobHandlerIntegrationTests` (8 testes) cobre N abertas → N eventos, candidaturas com desfecho intocadas, vaga de outra empresa não afectada, vaga já encerrada e perfil sem recrutamento | O `<remarks>` declara o que o InMemory **não** prova: atomicidade. O que se prova é quais candidaturas mudam, quais não, e quantos eventos saem |
| 6.1 | A exclusão dos cancelados ficou num helper `EffectiveApplications(byStatus, previousPeriod)` usado nos **dois** períodos | O cálculo do período anterior (linha 49) somava `item.Previous` sem filtro. Corrigir só o actual faria a seta de variação comparar duas definições diferentes e mostrar uma queda inventada. Não estava no design |
| 6.1 | A `Note` do funil passa a declarar que canceladas ficam fora da base, e há teste que a verifica | Quem lê o painel tem de poder descobrir a definição sem ler o código — é o mesmo princípio das lacunas declaradas do ADR 0010 |
| 7.1 / 7.2 | ADRs **0011** e **0012**, com o índice de `docs/sdd/adrs/README.md` actualizado | O 0011 delimita o âmbito do 0003 em vez de o revogar; o 0012 registra a ordem de registo dos behaviors como o mecanismo frágil que é |

**Escala do encerramento (pedido explícito para não assumir).** Fiz três coisas e nenhuma é um limite:
uma consulta única para as candidaturas em aberto (não há N+1); ordenação por `Id` para a ordem de
bloqueio ser determinística entre chamadas concorrentes; e `LogWarning` a partir de 200 candidaturas para o
volume aparecer em log em vez de degradar em silêncio. **Não** impus um teto: recusar encerrar uma vaga
popular seria pior do que a escrita demorar. O que fica de pé é que N `UPDATE` numa transacção mantêm as
linhas bloqueadas enquanto durarem, e que N é limitado pelas candidaturas em aberto de **uma** vaga.
*Gatilho de retorno:* aparecer esse `LogWarning` em produção, ou queixa de lentidão ao encerrar — a partir
daí a troca é para actualização em bloco, e o custo dessa troca é perder a guarda do agregado (§5.1).

**Aviso obrigatório ao consumidor — `PUT /api/jobs/{id}/close` quebrou o contrato.** A resposta era
`200` com a string `"Vaga encerrada com sucesso."` e passa a ser
`{ "jobId": 36, "closedAt": "2026-09-02T...Z", "affectedApplications": 3 }` (camelCase). Qualquer cliente
que lesse a resposta como texto passa a receber um objecto. A tarefa 5.4 é do agent de frontend e depende
disto; API e frontend têm de ir no mesmo deploy, como já vale para o resto da feature.

**Aviso de negócio — os números do dashboard mudam.** O funil e a taxa de conversão passam a excluir
`Canceled` e `CanceledByCandidate` da base. A base fica menor e **a conversão sobe**. Não é correcção de
bug de cálculo, é mudança de definição da métrica: quem acompanha a série histórica precisa de saber a data
do corte. A tarefa 6.3 (comunicar a quem lê o dashboard) é de comunicação e continua **por fazer** — não é
algo que eu possa fechar no código.

**Não feito, por estar fora do escopo delegado:** 5.4 (frontend), 6.3 (comunicação), 8.1–8.3 (verificação
final, regressão E2E pela UI e revisão de diff).

**Verificação (backend, Blocos 5, 6 e 7 + correcção da duplicação):**
`dotnet build backend/EmpregaNet.sln` verde — 0 erros, 0 avisos.
`dotnet test backend/tests/tests.csproj` — **423 aprovados, 0 falhas, 0 ignorados** (401 → 423: 22 novos).
Corridos com `BaseOutputPath` fora da árvore, com a API a correr na 5225. `Bff/` não foi tocado;
`frontend/` não foi tocado; `appsettings.Development.json` não foi tocado. **Sem migration nesta leva** — o
`OpenApplicationsCount` é calculado, não persistido, e o `CloseJobResult` não muda o modelo.

### Bloco 5 — frontend (5.4, 5.6) · `frontend-engineer`

| # | Ajuste face ao design | Razão |
| - | --------------------- | ----- |
| 5.4 | `openApplicationsCount` **saiu do detalhe da vaga**: passou a ser leitura própria (`GET /api/jobs/{id}/open-applications-count`, sob a policy de Recrutamento e sem cache), feita **ao abrir a confirmação**. O `recruiterJobResponseSchema` da correcção anterior deixou de ter razão de ser e foi removido | *(decisão do utilizador sobre a origem do dado)* O campo vinha de `GET /api/jobs/{id}`, que é `[AllowAnonymous]` com `OutputCache` de 5 min: a contagem vazava para anónimos e podia estar velha ao ponto de a confirmação dizer "Nenhuma candidatura em aberto será cancelada" numa vaga com fila. Ler no instante da decisão é o que torna o número verdadeiro; e o detalhe da vaga deixa de depender de campo novo, o que dissolve o acoplamento de deploy que eu tinha registado |
| 5.4 | Contagem indisponível **não vira zero**: a confirmação passa a dizer "Não foi possível verificar quantas candidaturas em aberto serão canceladas. Se houver candidaturas em aberto, elas serão canceladas mesmo assim", e o encerramento continua possível | *(decisão pedida na revisão)* Zero seria uma afirmação sobre o que não se sabe — a promessa falsa que esta feature corrige, agora dentro do aviso que existe para a evitar. Bloquear o encerramento por falha de uma leitura auxiliar seria pior: deixaria a empresa sem acção sobre a própria vaga por causa de um endpoint secundário. Enquanto a contagem não chega, o texto diz "Verificando…" e o botão fica em espera |
| 5.4 | As três redacções vivem em `describeCloseJobConfirmation(count)`, sobre a união `counting | unavailable | ready` | A tela escolhe o estado; o texto é função pura desse estado, e é a mesma função que o BDD exercita. Com a escolha inline no componente, o teste verificaria uma cópia da regra |
| 5.6 | O duplo de transporte passou a cobrir **GET** além de PUT (`tests/support/axios-double.ts`, `respondToGet`) | Sem um duplo de GET não era possível distinguir "a contagem não foi lida ao abrir a tela" de "foi lida e ignorada" — e é justamente isso que o cenário novo fixa: a leitura acontece ao abrir a confirmação, não com o detalhe da vaga |
| 5.4 | A resposta de `PUT /api/jobs/{id}/close` passou da **string** `"Vaga encerrada com sucesso."` — que o service devolvia sem parse (`Promise<string>`) — para `closeJobResponseSchema` com `jobId`/`closedAt`/`affectedApplications` | Contrato tipado na fronteira (ADR 0009). **O design §4.3 dizia `bool`; o corpo antigo era texto** (confirmado pelo backend durante a implementação), e o novo contrato substitui esse caminho em vez de conviver com ele: nenhum ecrã montava mensagem a partir do corpo, por isso a troca não deixa consumidor de texto para trás. Uma mensagem pronta não permitia dizer o efeito, que é o que CA-17 exige |
| 5.4 | `useCloseJobMutation` invalida também `jobApplicationsKeys.all`, o que o design não pedia; para isso `jobApplicationsKeys` passou a ser exportado na fronteira pública de `features/candidaturas/service` | O encerramento move as candidaturas em aberto para "Cancelada" na mesma transacção (5.1). Sem invalidar, a tela de candidatos da vaga continuaria a mostrar "Em análise" em linhas que a API já mudou — e o recrutador agiria sobre estado inexistente |
| 5.4 | O feedback de sucesso usa `affectedApplications` e **não** afirma que os candidatos foram avisados | N6 só existe com o Bloco 4. Mesmo critério da tarefa 4.8: a frase tem de ser verdadeira hoje |
| 5.4 | Vaga encerrada ganha um `Alert` (role `status`) a explicar que o encerramento não tem retorno — não estava no design | V2 pede mostrar o estado e retirar a acção. Retirar o botão sem explicar deixa o utilizador a procurar uma reabertura que não existe (D4); o aviso responde à pergunta que a ausência do botão levanta |
| 5.4 | Extraído `jobStatusLabel(isActive)` para `close-job-copy.ts` e **também** aplicado na listagem de vagas, onde a expressão estava inline | O rótulo "Ativa"/"Encerrada" estava escrito em três lugares (listagem, detalhe público e o step de BDD que dizia por comentário ser "a mesma expressão"). Com a gestão da vaga a exibi-lo, seriam quatro — e o teste passaria a afirmar uma cópia |
| 5.4 | A contagem do efeito vive em `describeOpenApplicationsEffect` / `describeClosedApplicationsEffect`, com a concordância de número explícita | Os cenários 0/1/N apanharam um defeito real na primeira versão: com zero, o texto saía "Nenhuma candidatura em aberto **serão** canceladas". Sujeito singular ("Nenhuma", "1") leva verbo singular; só a partir de 2 vai ao plural |
| 5.6 | O duplo de `axiosApi.put` saiu dos ficheiros de steps para **`tests/support/axios-double.ts`**, instalado e restaurado por cenário em `hooks.ts`; cada cenário declara a resposta com `respondToPut` | *(defeito real apanhado pelos testes)* Com dois ficheiros de steps a instalar o seu próprio duplo num `Before`, o último registado ganhava: os cenários de encerramento recebiam a resposta de cancelamento e falhavam no parse do contrato **com o código correcto**. Era exactamente o falso-positivo que a revisão do Bloco 2 antecipou, materializado na direcção oposta |
| 5.6 | `respondToPut(pattern, responder)` regista **por endpoint** e o duplo escolhe a declaração mais recente que casa com a URL, em vez de um slot único sobrescrito | *(correcção após revisão)* Com um só slot, os dois `Dado` do cancelamento escreviam no mesmo lugar e a ordem dos passos passava a ser significativa por acidente: a `.feature` só funcionava na ordem em que estava escrita. Por padrão de URL, endpoints diferentes coexistem no mesmo cenário, e redeclarar o mesmo endpoint continua a sobrepor-se — que é a semântica pretendida de um "Dado ... vai recusar" a refinar o estado montado antes |
| 5.6 | O step de asserção da chamada chama-se "a API **de vagas** deve ter recebido" | As definições de step do Cucumber são globais ao processo: "a API deve ter recebido {string}" já existia em `my-applications-cancel.steps.ts` e a repetição dava `ambiguous`, não reutilização |
| 5.6 | `job-form-submission.steps.ts` (fora do escopo nominal): o step de rótulo passou a chamar `jobStatusLabel` de produção. O `openApplicationsCount` chegou a entrar no fixture e **saiu de novo** quando o campo deixou o `JobViewModel` | O step reimplementava o rótulo, que agora tem função única. E um fixture que se declara "resposta completa de `JobViewModel`" não pode listar um campo que a API já não envia — seria documentação a mentir sobre o contrato |

**Não feito, por decisão de produto:** nenhuma acção de reabrir vaga, em nenhum ecrã. `D4` fixa o
encerramento como terminal, e a UI não oferece o que a API não suporta.

**Não feito, por estar fora do bloco delegado:** 5.1–5.3 e 5.5 (backend), e os Blocos 3, 4 (backend) e 6.

**Acoplamento de deploy, depois da mudança de origem do dado:** o detalhe da vaga deixou de depender de
campo novo — nem a gestão nem o feed nem `/vagas/[id]` público. O que passa a exigir o backend é a
confirmação de encerramento: sem `GET /api/jobs/{id}/open-applications-count`, ela abre a dizer que não
conseguiu verificar a contagem, e o encerramento continua a funcionar. Degradação legível, não ecrã partido.

**Assunção confirmada (2026-09-09):** o corpo da contagem foi escrito como `{ "openApplicationsCount": 3 }`,
por ser a forma dos outros contratos JSON desta API. **O endpoint devolve `{ "jobId": 36,
"openApplicationsCount": 3 }`** — a suposição estava certa e o campo extra `jobId` não afecta o schema.
Nada a mudar.

Fica registado o que tornou esta assunção segura de fazer: se estivesse errada, o parse falhava para o
caminho já coberto de "contagem indisponível", que **não** afirma zero. A escolha do estado de falha é o
que permitiu ao frontend avançar sobre um contrato ainda não verificado sem arriscar mostrar um número
inventado.

**Verificação (frontend), depois de 5.4, 5.6, das três correcções da revisão (#3, #4, #6) e da mudança de
origem do `openApplicationsCount`:** `pnpm --dir frontend lint` sem erros;
`pnpm --dir frontend test` **423 cenários / 1262 steps, todos verdes**;
`pnpm --dir frontend build` verde, `/recrutamento/vagas/[id]` mantém-se `◐`, `/recrutamento/vagas` `○` e
`/vagas/[id]` `◐`. Prettier corrido só nos ficheiros tocados.

### Correcções da revisão dos Blocos 3–7 · `dotnet-implementer`

| # | Correcção | Razão |
| - | --------- | ----- |
| Importante 1 | Extraído `DependencyInjection.AddPipelineBehaviors` (`internal`), e **os testes passam a resolver os behaviors a partir dele**. Removido o teste de ordem que montava a sua própria `ServiceCollection` | O teste anterior era uma tautologia: verificava que a `ServiceCollection` preserva ordem de inserção. **Confirmado por mutação:** inverti as duas linhas em produção e o teste novo falha (2 testes), enquanto o antigo ficaria verde |
| Importante 1 | Corrigido o comentário no ponto de registo e a frase do ADR 0012, que afirmava existir um teste a garantir a ordem | A afirmação era falsa enquanto o teste não tocava a produção. **Nota factual:** `PipelineBehaviorRegistrationTests` **existe** no repositório desde antes deste trabalho (`tests/Unit/Behaviors/`) — o problema não era o ficheiro não existir, era ele recopiar a ordem em vez de a ler. Foi actualizado para chamar `AddPipelineBehaviors` e ganhou o teste de índices |
| Importante 4 | A descrição do KPI `conversionRate` deixa de dizer "sobre o total recebido" e passa a declarar que canceladas ficam fora da base, com teste (`Handle_DescricaoDaTaxa_...`) | É o único sítio onde a definição aparece ao lado do número; quem confere a conta à mão obtinha outro valor e concluía que o painel estava errado |
| Importante 5 | Removido o `UpdateAsync` de dentro do laço de `CloseJobHandler`; os `<remarks>` deixam de dizer "cada uma com o seu UPDATE" | `BaseRepository.UpdateAsync` chama `SaveChangesAsync` a cada volta — convertia uma gravação em lote em N round-trips sequenciais. As entidades ficam rastreadas e o `UnityOfWork` grava no fim; a guarda de `ChangeStatus` mantém-se |
| Importante 5 | `CloseJobHandlerIntegrationTests` ganhou `PersistAsync` explícito depois de cada `Handle` | Sem gravação, as asserções liam o *identity map* em memória e passariam mesmo que nada fosse escrito — o teste tinha de deixar de mentir junto com a correcção |
| Menor 1 | `CandidateName` passa a ser usado: saudação "Olá, {nome}!" quando há nome, nada quando não há | Campo preenchido e nunca lido é ou desperdício ou esquecimento; aqui era esquecimento. `"Olá, !"` é pior do que não saudar |
| Menor 2 | O template ramifica por `NewStatus` além da `Reason` (`StatusChangeTreatment`), e `JobApplicationEmailModel` ganhou `NewStatus` | Aprovada e reprovada chegavam ambas como `StatusChanged`: a recusa saía com o verde e o sino da aprovação. Um e-mail que engana antes de ser lido é pior do que um e-mail feio. Status sem tratamento próprio (Timeout, Error) usam tom neutro em vez de escolher uma emoção errada |
| Menor 3 | `JobApplication.OpenStatuses` passa a `IReadOnlyList<ApplicationStatusEnum>` | Array público `static readonly` é mutável por qualquer chamador; o objecto em runtime continua o mesmo, portanto a tradução EF não muda |
| Menor 5 | `SendStatusNotificationAsync` passa a chamar `cancellationToken.ThrowIfCancellationRequested()` em vez de descartar o token | `IEmailSender.SendEmailAsync` não aceita token, portanto o cancelamento só é observável **antes** do envio — que é o caso que importa (não disparar e-mail de requisição abandonada). O `<remarks>` diz exactamente isso, em vez de a assinatura prometer o que não cumpre |
| Menor 7 | A resolução de `IMediator.Publish` passa a seleccionar a sobrecarga por forma (genérica, dois parâmetros, o segundo `CancellationToken`) | `GetMethod(nome)` lança `AmbiguousMatchException` com uma segunda sobrecarga; num inicializador estático isso chega como `TypeInitializationException` ao resolver o behavior — em **todas** as requisições, incluindo as que não têm notificações |

**Não tocados, por estarem com o coordenador:** Importantes 2 e 6.

**Verificação:** `dotnet build backend/EmpregaNet.sln` verde — 0 erros, 2 avisos `CS1573` pré-existentes em
`StringHelper.cs`. `dotnet test backend/tests/tests.csproj` — **432 aprovados, 0 falhas, 0 ignorados**
(423 → 432). Além disso, **teste de mutação manual** da ordem dos behaviors: invertidas as duas linhas em
`AddPipelineBehaviors`, `PipelineBehaviorRegistrationTests` falha em 2 testes; reposta a ordem, verde.

### Importante #2 — contagem de candidaturas em aberto sai do detalhe público · `dotnet-implementer`

Decisão do utilizador: **endpoint próprio, autenticado, sem cache.**

O defeito: a tarefa 5.3 pôs `OpenApplicationsCount` no `JobViewModel`, que é servido por
`GET /api/jobs/{id}` — `[AllowAnonymous]` + `[OutputCache(PublicCatalog)]` (`JobsController.cs:101-107`).
Duas consequências de uma vez: **qualquer visitante anónimo passava a ver a concorrência de cada vaga**, e o
recrutador recebia um número com minutos de atraso, capaz de fazer a confirmação afirmar "nenhuma
candidatura em aberto será cancelada" sobre uma vaga com fila. É a mesma classe de defeito que esta feature
existe para corrigir — informação que contradiz a realidade no momento da decisão — a entrar por uma porta
lateral que eu próprio abri.

| # | Mudança | Razão |
| - | ------- | ----- |
| 5.3 | `JobViewModel.OpenApplicationsCount` **removido**; `GetJobByIdHandler` volta a não depender de `IJobApplicationRepository` | O detalhe da vaga é catálogo público e cacheado; nada que dependa de frescura ou de autorização pode viajar nele |
| 5.3 | Novo `GET /api/jobs/{id}/open-applications-count`, `[Authorize(Policy = Recrutamento)]`, **sem `OutputCache`**, devolvendo `{ jobId, openApplicationsCount }` | Contagem fresca por construção, e visível só a quem decide o encerramento |
| 5.3 | O handler aplica `IJobEmployerAccess.EnsureCanManageCompanyAsync`, como o `CloseJobHandler` | Pertencer ao recrutamento não basta: a fila de candidatos de uma vaga é informação competitiva da empresa que a publicou. Sem isto, qualquer recrutador media a procura das vagas de qualquer concorrente |
| 5.3 | O handler também chama `RecruitmentAccess.EnsureRecruitmentStaff` | Defesa em profundidade além do `[Authorize]`, como já era prática nos comandos de vaga |
| 5.5 | `Unit/Jobs/GetJobOpenApplicationsCountHandlerTests` (8 testes): contagem correcta, zero, recusa a recrutador de outra empresa **sem chegar a contar**, verificação de acesso pela empresa dona, recusa a perfil sem recrutamento antes de tocar na vaga, vaga inexistente e excluída | Cobre os três casos pedidos e os caminhos de recusa |
| 5.5 | Teste de regressão `JobViewModel_NaoDeveExporContagemDeCandidaturas`, por reflexão sobre as propriedades do view model | O defeito foi acrescentar um campo a um DTO público. Se alguém o repuser, volta a sair por endpoint anónimo e cacheado — e nada mais no sistema acusaria. Este teste falha nesse momento |
| 5.5 | Os três testes de contagem saíram de `GetJobByIdHandlerTests`, que voltou ao estado anterior | Deixaram de descrever aquele handler |

**Contrato alterado, com consumidor já escrito.** O frontend tinha acabado de fixar
`openApplicationsCount` como campo do detalhe da vaga; passa a precisar de uma chamada autenticada a
`GET /api/jobs/{id}/open-applications-count`. O coordenador está a avisar o agent de frontend em paralelo.
A chamada deve ser feita **ao abrir a confirmação de encerramento**, não ao carregar a página — é o que
mantém o número fresco no instante da decisão.

**Verificação:** `dotnet build backend/EmpregaNet.sln` verde — 0 erros, 2 avisos `CS1573` pré-existentes em
`StringHelper.cs`. `dotnet test backend/tests/tests.csproj` — **437 aprovados, 0 falhas, 0 ignorados**
(432 → 437).

### Correcção de infraestrutura partilhada — estado por tentativa no retry transaccional · `dotnet-implementer`

**Não é tarefa desta feature.** É um defeito de correcção em `UnityOfWork`/`TransactionBehavior` que afecta
**todo** create/update/delete do sistema (`ITransactional` está em `CreateCommand`, `UpdateCommand` e
`DeleteCommand`), e que a feature apenas **expôs** — as guardas novas de `CloseJobHandler`,
`ChangeJobApplicationStatusHandler` e `CancelJobApplicationHandler` tornaram visível um erro que antes era
só escrita perdida em silêncio. Entrou neste PR por decisão do utilizador. Decisão registada no
[ADR 0013](../../sdd/adrs/0013-estado-por-tentativa-no-limite-de-retry-transaccional.md).

| # | Mudança | Razão |
| - | ------- | ----- |
| D1 | `UnityOfWork.ExecuteInTransactionAsync` limpa o `ChangeTracker` **a partir da segunda tentativa** da execution strategy (contador local; `if (attempt++ > 0)`) | A estratégia (`EnableRetryOnFailure(maxRetryCount: 2)`) reexecuta o delegate inteiro sobre o **mesmo** `DbContext`, e o EF não sobrescreve entidade já rastreada. Sem repor o tracker: guarda a disparar sobre estado nunca commitado (400 "A vaga já está encerrada." num encerramento que não aconteceu) e, onde não há guarda, `UPDATE` nunca emitido — **200 sem escrita, sem log**. `DeleteUserHandler` juntava os dois: a guarda de idempotência transformava a 2.ª tentativa em sucesso falso |
| D1 | A limpeza é **condicional**, não incondicional | Limpar antes da primeira tentativa destacaria entidades rastreadas mais cedo na requisição por código alheio à transacção (Identity, `UserManager` em `JobEmployerAccess`). A semântica é "repor o que uma tentativa falhada sujou" |
| D1 | O `UnityOfWork` **não** passou a conhecer `IDomainEventQueue`, `IMediator` nem notificações | Repor a fila daqui faria a persistência depender de notificações para corrigir um problema de persistência |
| D1 | `verifySucceeded` continua `null`, com o limite escrito no `<remarks>` | Commit com êxito e ACK perdido faz a tentativa seguinte ler estado já commitado: a guarda dispara com mensagem agora **verdadeira**. Verificar o desfecho exigiria predicado por comando, e o `TransactionBehavior` é genérico em `TRequest` |
| D2 | `RegisterUserHandler` deixa de enviar o e-mail de confirmação: enfileira `UserRegistered`; o novo `UserRegisteredEmailHandler` (Application/Auth/Events) gera o token e envia depois do commit | O envio dentro da transacção tinha dois modos de falha que nenhuma limpeza de tracker corrige: link enviado para registo que o commit não gravou, e segundo e-mail por reexecução da tentativa. Reutiliza o mecanismo do [ADR 0012](../../sdd/adrs/0012-despacho-de-eventos-de-dominio-apos-commit.md) — encaixou sem alteração nenhuma ao behavior nem à fila |
| D2 | O token é gerado **no consumidor**, contra o utilizador lido por id, e não viaja no evento | Token de confirmação é credencial; e gerá-lo pós-commit garante que deriva do `SecurityStamp` persistido |
| D5 | ADR 0013 escrito e indexado; **estende o 0012** de "e-mail de candidatura" para "qualquer efeito externo" | A regra que torna o retry defensável é "handler `ITransactional` não executa efeito externo que um rollback não desfaça", não o `Clear()` |
| D3 | **Adiado:** a `IDomainEventQueue` não é reposta por tentativa. Dano contido pela `DeduplicationKey` (inclui o `JobApplicationId`) e pela guarda de projecção nula em `JobApplicationStatusChangedEmailHandler` — degrada para uma linha de log. Gatilho de retorno: primeiro `INotification` cujo consumidor **não** re-resolva o payload por id a partir da base, ou primeira publicação não idempotente | Repor a fila exigiria acoplar persistência a notificações |

**Varredura dos handlers `ITransactional` (pedida, e completa).** Procurado `IEmailSender`,
`IAccountEmailService`, `HttpClient`, cache e qualquer efeito externo no corpo:

| Handler | O que tem dentro da transacção | Decisão |
| ------- | ------------------------------ | ------- |
| `RegisterUserHandler` | `IAccountEmailService.SendEmailConfirmationLinkAsync` | **Corrigido** (D2) |
| `DeleteUserHandler`, `UpdateAdminUserHandler` | `IOutputCacheManager.InvalidateAdminUsersAsync` | **Não corrigido, reportado.** Purga sobre operação revertida é auto-curável (o pedido seguinte repovoa com o dado verdadeiro) e repetida é idempotente. Mover para pós-commit é trabalho de outro diff |
| `LoginWithGoogleHandler` | `IGoogleIdTokenValidator.ValidateAsync` (HTTP para a Google) | **Não corrigido, reportado.** Leitura idempotente, sem efeito no exterior; repeti-la custa latência |
| `LoginWithGoogleHandler`, `DeleteUserHandler`, `UpdateAdminUserHandler` | `IRefreshTokenService` (`IssueAsync` / `RevokeAllForUserAsync`) | Escreve no **mesmo** `DbContext`, portanto participa da transacção. Chama `SaveChangesAsync` por si — mesma classe de padrão de `BaseRepository`, neutralizada por D1 |
| `ApplyToJobHandler`, `ChangeJobApplicationStatusHandler`, `CancelJobApplicationHandler`, `CloseJobHandler` | `IDomainEventQueue.Enqueue` | Correcto por construção (ADR 0012) |
| `CreateJobHandler`, `UpdateJobHandler`, `DeleteJobHandler`, `CreateCompanyCommandHandler`, `UpdateCompanyHandler`, `DeleteCompanyHandler` | Só repositório e `IJobEmployerAccess`/`IHttpCurrentUser` (leitura) | Nada a fazer |

**Lacuna de verificação, declarada e não escondida.** A parte grave do defeito **não é reproduzível** com o
stack de testes actual: `InMemoryIdentityFixture` usa `UseInMemoryDatabase`, sem execution strategy e com
transacção no-op — sem rollback real, a escrita perdida em silêncio não é observável. Foi essa a razão de o
defeito passar 437 testes verdes. **Não** se criou abstracção de `IExecutionStrategy` para contornar isso
(seria interface com uma implementação cujo único consumidor é o teste). O que se conseguiu fixar:
`Unit/Persistence/UnityOfWorkTests` injecta uma estratégia com uma repetição pelo ponto de extensão do
próprio EF (`IExecutionStrategyFactory` via `ReplaceService`, zero código novo em produção) e afirma
**quando** o tracker é reposto. Cobertura do comportamento transaccional real exige base real
(Testcontainers) — decisão do utilizador, já na fila dele.

**Testes acrescentados (+8):** `Unit/Persistence/UnityOfWorkTests` (3: 1.ª tentativa não limpa; 2.ª limpa;
falha não transitória não repete), `Integration/Handlers/UserRegisteredEmailHandlerIntegrationTests` (4:
link com `userId` e token, o link **confirma** o e-mail de verdade via `ConfirmEmailHandler`, conta
inexistente não envia nem relança, falha de transporte não relança) e
`RegisterUserHandlerIntegrationTests.Handle_RegistoRecusado_NaoDeveEnfileirarEvento`. O teste do registo
válido passou a exigir `Times.Never` no envio — é ele que impede o e-mail de voltar para dentro da
transacção.

**Teste de mutação da condição, nos dois sentidos:** `attempt++ > 5` (nunca limpa) → falha
`..._SegundaTentativa_DeveLimparOChangeTracker`; `attempt++ >= 0` (limpa sempre) → falham os dois testes de
tentativa. Reposto `attempt++ > 0`, verde.

**Verificação:** `dotnet build backend/EmpregaNet.sln` verde — 0 erros, 2 avisos `CS1573` pré-existentes em
`StringHelper.cs`. `dotnet test backend/tests/tests.csproj` — **445 aprovados, 0 falhas, 0 ignorados**
(437 → 445).
