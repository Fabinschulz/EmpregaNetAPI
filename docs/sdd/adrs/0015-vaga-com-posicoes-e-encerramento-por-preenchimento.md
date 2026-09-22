# ADR 0015: Vaga com total de posições e encerramento automático por preenchimento

## Contexto

A vaga tinha um único estado — `IsActive: bool` — e nenhuma noção de quantidade. Três consequências
práticas:

1. **Uma vaga nunca enchia.** Aprovar candidatos não tinha efeito nenhum sobre a vaga: era possível
   aprovar cinquenta pessoas numa vaga de três posições, e a vaga continuava no feed a receber
   candidaturas que nunca teriam para onde ir.
2. **"Encerrada" não dizia o que aconteceu.** `IsActive = false` cobre tanto "a empresa desistiu da
   vaga" como "contratámos toda a gente". São desfechos opostos para o candidato e para o
   recrutador, e a tela mostrava a mesma palavra nos dois.
3. **O encerramento não ficava datado.** `CloseJobResult.ClosedAt` era `DateTimeOffset.UtcNow`
   calculado no handler e devolvido na resposta; nada disso era gravado. Passado o pedido, não havia
   como saber quando a vaga fechou.

Sobre como representar a quantidade preenchida havia dois caminhos:

- **Derivar por contagem** de candidaturas em estado que ocupa posição, a cada leitura. Sem risco de
  divergência, mas `JobViewModel` é mapeado directamente da entidade em três handlers (`GetById`,
  `GetAll`, `Update`), que passariam todos a precisar de projecção, e cada leitura da vaga ganharia
  uma subconsulta.
- **Contador no agregado.** Uma coluna que o próprio `Job` mantém.

E sobre o modelo de status:

- **Substituir `IsActive` por um `JobStatusEnum`.** Um campo só, mais limpo — mas é migration com
  `drop` de coluna, reescrita de `IX_Jobs_Feed` e `IX_Jobs_IsActive`, e quebra do contrato HTTP em
  todos os consumidores.
- **Aditivo:** manter `IsActive` e acrescentar o que falta.

## Decisão

- **`Job` recebe `Positions` e `FilledPositions`.** O total é obrigatório na escrita (mínimo 1,
  máximo 999) e `AvailablePositions` é derivado. O contador fica no agregado, não numa contagem: a
  regra "encerra ao encher" é invariante **da vaga**, e mantê-la aqui deixa o agregado recusar a
  posição que não tem em vez de depender de quem se lembrar de contar.
- **Ocupam posição os estados `Approved` e `Finished`**, declarados em
  `JobApplication.PositionHoldingStatuses`. A aprovação é o acto que toma a posição; `Finished`
  fecha o processo daquele aprovado e **não** a devolve — se devolvesse, concluir um processo
  reabriria a vaga. `Rejected`, `Canceled`, `Timeout` e `CanceledByCandidate` libertam ou nunca
  ocuparam.
- **Preencher a última posição encerra a vaga**, com `ClosureReason = Fulfilled`, e arrasta as
  candidaturas ainda em aberto para `Canceled` — o mesmo efeito do encerramento manual, mas com a
  razão de notificação `JobFilled`, que escolhe um texto de e-mail diferente. Deixar candidatos em
  "Em análise" numa vaga que já não pode aprová-los seria pior do que cancelar.
- **O modelo de status é aditivo:** `IsActive` continua a ser o predicado do feed e dos índices, e
  entram `ClosedAt` e `ClosureReason` (`Manual` | `Fulfilled`). A API ganha um campo `status`
  derivado (`Active` | `ClosedManually` | `ClosedByFulfillment`), calculado por `JobStatus.Resolve`,
  função única partilhada pelo mapper da vaga e pelo do feed.
- **O encerramento continua terminal.** `ReleasePosition` baixa o contador para o número continuar
  verdadeiro, mas **não reabre** a vaga: ao encerrar, as candidaturas em aberto já foram canceladas,
  e reabrir devolveria uma vaga sem funil nenhum. Precisar de mais gente significa publicar vaga nova.
- **Aprovação e edição leem a vaga com a linha bloqueada** (`GetByIdForUpdateAsync`,
  `SELECT … FOR UPDATE`). Duas aprovações simultâneas leriam o mesmo saldo e ambas incrementariam; e
  a edição, que grava a linha inteira porque o EF marca todas as colunas como modificadas, reporia
  `IsActive`, `FilledPositions`, `ClosedAt` e `ClosureReason` por cima de uma aprovação concorrente —
  ressuscitando em silêncio uma vaga já encerrada. Os dois escritores têm de passar pelo mesmo
  bloqueio; proteger só um não protege nenhum. No provider in-memory dos testes degrada para leitura
  rastreada normal, onde não há concorrência a proteger.
- **O total não pode descer abaixo do preenchido**, porque isso tornaria `AvailablePositions` mentira
  e deixaria aprovados sem posição correspondente. **Nem pode mudar numa vaga encerrada**, porque
  subi-lo criaria uma vaga encerrada com posição livre — contradição que o encerramento terminal
  nunca resolveria. Editar o resto de uma vaga encerrada continua permitido.
- **As invariantes de posição também vivem no banco:** `CK_Jobs_Positions` e
  `CK_Jobs_FilledPositions`. O agregado só protege quem passa por ele; as constraints são a rede para
  SQL manual, migration futura ou handler novo que mexa no contador sem o saber, e custam uma
  comparação de inteiros por escrita.
- **Recusa do agregado chega como erro de negócio, não como conflito genérico.** `UpdateJobHandler` e
  `ChangeJobApplicationStatusHandler` traduzem `InvalidOperationException` para
  `ValidationAppException` com `INVALID_ACTION_FOR_STATUS`. Sem a tradução, o handler global devolve
  409 "Operação inválida." e, fora de Development, suprime o detalhe — o recrutador fica sem saber o
  que corrigir.
- **Mudar o status de uma candidatura invalida a cache de leitura da vaga.** A aprovação altera o
  contador e pode encerrar a vaga; as tags de candidatura não tocam nas de vaga, e sem esta
  invalidação o feed público continuaria a anunciar, até ao fim do TTL, uma vaga que já não aceita
  ninguém.

## Consequências

**Positivas:**

- A vaga deixa de aceitar candidaturas que nunca teriam posição, sem ninguém ter de se lembrar de a
  encerrar à mão.
- Recrutador e candidato passam a ler "3 de 5 vagas preenchidas" e "Restam 2 vagas" — as três
  quantidades pedidas, das quais nenhuma existia.
- "Vagas preenchidas" deixa de ser pintado como um encerramento negativo: é o desfecho
  bem-sucedido do processo e tem tom próprio no badge.
- O instante do encerramento passa a existir no banco, e não apenas no corpo de uma resposta.
- `FilledPositions` é coluna indexável: um filtro futuro por "vagas com posição disponível" não
  precisa de subconsulta.

**Negativas / obrigações futuras:**

- **A migration move dados** (ver `20260921020147_VagasPorPosicaoEEncerramentoAutomatico`) e exige
  revisão humana antes de aplicar. As `CHECK` entram depois dos backfills de propósito: entre o
  primeiro e o segundo, uma vaga antiga fica momentaneamente com mais preenchidas do que o total. O
  `Down` apaga as quatro colunas e, com elas, todo o histórico de preenchimento e de motivo —
  reaplicar o `Up` não o recupera.
- **O contrato de escrita quebra:** `CreateJobCommand`/`UpdateJobCommand` passam a exigir
  `Positions`. O consumidor é único e interno (o formulário de recrutamento), actualizado na mesma
  entrega.
- **Uma vaga antiga com mais aprovados do que uma posição fica activa e cheia** depois do backfill:
  a migration não encerra vagas em produção por uma regra que não existia quando foram publicadas.
  A candidatura é recusada com mensagem própria e o recrutador resolve subindo o total na edição.
- **`FilledPositions` é denormalizado.** O único ponto de mutação é a transição de status da
  candidatura; qualquer escrita futura em `JobApplications.Status` que não passe por
  `ChangeJobApplicationStatusHandler` tem de mexer no contador, ou o número diverge. `CK_Jobs_FilledPositions`
  apanha a divergência que exceda o total, e a guarda de `AvailablePositions == 0` em
  `ApplyToJobHandler` existe para a que não exceda não ser paga pelo candidato. **Concretamente:
  `DELETE /api/jobapplications/{id}` não tem handler hoje; quando alguém o implementar, tem de
  libertar a posição se a candidatura estiver em `Approved`/`Finished`.**
- **`Positions` é duplicado no frontend** (`MIN_JOB_POSITIONS`/`MAX_JOB_POSITIONS` em
  `shared/schema/job-vocabulary.ts`) para o formulário recusar antes da ida ao servidor. Mudar o
  intervalo no domínio obriga a mudar lá.
- Reabrir uma vaga encerrada continua a não existir. Se essa necessidade aparecer, terá de decidir
  também o que fazer com as candidaturas canceladas por arrasto — que é a razão de hoje não existir.

## Superfícies fechadas numa segunda passagem

Uma auditoria pós-implementação encontrou quatro lugares que ainda liam ou mostravam o estado
antigo, binário, de `Job`:

- **Ranking de desempenho do dashboard** (`GetDashboardJobsHandler`): rotulava toda vaga inactiva
  como "Encerrada", incluindo as preenchidas com sucesso. Passou a usar `JobStatus.Resolve` e
  devolve `Status` no `DashboardJobPerformanceViewModel`, tal como no resto da API.
- **Tela de candidatos** (`CandidatesByJobPage`): já buscava a vaga só para o título; passou a
  mostrar o badge de situação e "N de M vagas preenchidas" — a superfície onde as três quantidades
  pedidas pelo produto (total, preenchidas, disponíveis) precisam de aparecer, e a única que ainda
  não as mostrava.
- **Aprovar a última posição não pedia confirmação**: tem o mesmo efeito de arrasto do botão
  "Encerrar vaga" (cancela as candidaturas ainda em aberto), que já pede confirmação — "Aprovar"
  disparava a cascata com um clique só. Passou a exigir confirmação, com a contagem de outras
  candidaturas em aberto reaproveitando `describeOpenApplicationsEffect`.
- **`ChangeJobApplicationStatusCommandHandler` sem teste de integração**: o encerramento manual
  tinha `CloseJobHandlerIntegrationTests` contra repositórios reais; o automático — o núcleo desta
  capacidade — só tinha teste unitário com mocks. Ver
  `ChangeJobApplicationStatusHandlerIntegrationTests`.
- **Contrato do frontend fechado a valor novo de `status`/`closureReason`**: um valor que a API
  ganhasse depois (motivo de encerramento novo, por exemplo) reprovava o `.parse()` da resposta
  inteira. `status` passou a ter uma variante tolerante (`jobStatusResponseSchema`, com
  `.catch('ClosedManually')` — pessimista de propósito: um estado desconhecido nunca deve parecer
  activo) e `closureReason`, que nenhuma tela lê, deixou de ser um enum fechado.

## Revisado e mantido como está

- **Ordem "mutar a candidatura antes de verificar capacidade"** em
  `ChangeJobApplicationStatusCommandHandler`: a auditoria apontou que, se `ApplyPositionEffect`
  recusar depois de `application.ChangeStatus` já ter corrido, a candidatura fica com o `Status`
  alterado, mas não gravado, no `ChangeTracker`. Reanalisado ao fechar a auditoria: como nada é
  persistido em qualquer dos dois casos de falha (a excepção interrompe antes do `SaveChangesAsync`)
  e o `DbContext` é descartado no fim do pedido, isto não tem efeito observável. Inverter a ordem
  trocaria esse não-problema por um real: no caso em que a candidatura já está
  `CanceledByCandidate` **e** a vaga está cheia, a ordem actual recusa pela razão mais fundamental
  ("candidatura cancelada pelo candidato"); invertida, recusaria pela coincidência de a vaga estar
  cheia. Mantido como está.

## Adiado, com gatilho de retorno

Levantado na auditoria da implementação e deliberadamente não resolvido agora:

- **Arrasto síncrono do encerramento automático.** `JobClosureCascade` carrega todas as candidaturas
  em aberto e o `NotificationDispatchBehavior` envia um e-mail por candidatura, sequencialmente,
  dentro do pedido. Antes isto só acontecia quando um humano clicava "Encerrar vaga"; agora é
  consequência de aprovar o último candidato. **Gatilho:** a primeira vaga com mais de ~200
  candidaturas em aberto, ou o primeiro timeout no `PUT` de status. **Saída:** tecto explícito que
  recuse o arrasto síncrono e o empurre para processamento posterior.
- **Candidatura na janela do encerramento.** `ApplyToJobHandler` lê a vaga sem bloqueio, logo uma
  candidatura pode entrar entre a leitura do arrasto e o commit do encerramento, ficando `Pending`
  numa vaga encerrada. Não corrompe o contador. Bloquear a linha aqui serializaria todas as
  candidaturas à mesma vaga — o caminho mais quente do sistema — para evitar um dano raro e
  corrigível à mão; mau negócio sem medição. **Gatilho:** aparecerem órfãs a sério. **Saída:**
  varredura das candidaturas em aberto de vagas inactivas, não um lock.

## Referências

- `backend/src/EmpregaNet.Domain/Entities/Job.cs`, `JobApplication.cs`
- `backend/src/EmpregaNet.Domain/Enums/JobClosureReason.cs`, `JobStatus.cs`
- `backend/src/EmpregaNet.Application/Jobs/UseCase/JobClosureCascade.cs`
- `backend/src/EmpregaNet.Application/JobApplications/Commands/UpdateStatus/ChangeJobApplicationStatusHandler.cs`
- `backend/src/EmpregaNet.Infra/Persistence/Migrations/20260920214519_VagasPorPosicaoEEncerramentoAutomatico.cs`
- `frontend/src/features/recrutamento/vagas/domain/job-status.ts`
- [ADR 0006](0006-agregado-job-enriquecido.md) — agregado `Job` que esta decisão estende
- [ADR 0012](0012-despacho-de-eventos-de-dominio-apos-commit.md) — fila que publica as notificações
  de arrasto depois do commit
