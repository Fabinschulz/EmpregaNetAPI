---
version: 1.0.0
date: 2026-09-01
status: Approved
---

# Spec — Acompanhamento da candidatura pelo candidato

**feature-id:** `emp-acompanhamento-candidatura` · **PRD:** [`prd.md`](prd.md) v1.1.0 · **Design:** [`design.md`](design.md) v1.0.0

Mapa de rastreio: cada critério de aceite aprovado no PRD tem um local de verificação. Endpoints e
contratos ficam no `design.md`; passos de implementação, no `tasks.md`.

## 1. Matriz — critério de aceite → local de verificação

### Notificação

| CA | Critério (resumo) | Onde se verifica | Tipo |
| -- | ----------------- | ---------------- | ---- |
| CA-01 | Aprovada gera e-mail com vaga, empresa, status e data | `Unit/JobApplications/ChangeJobApplicationStatusHandlerTests` (evento enfileirado com `Reason=StatusChanged`) + `Unit/Email/JobApplicationEmailTemplateTests` (conteúdo mínimo do corpo) | Unitário |
| CA-02 | N2–N5 geram o e-mail correspondente | `Unit/JobApplications/ChangeJobApplicationStatusHandlerTests` — `[Theory]` por status (Processing, Approved, Rejected, Finished) | Unitário |
| CA-03 | Envio indisponível: transição conclui, sem erro na UI, falha registada | `Unit/JobApplications/JobApplicationStatusChangedEmailHandlerTests` (transporte lança → handler não relança e registra) + `Unit/Behaviors/NotificationDispatchBehaviorTests` (rollback → fila **não** drenada) | Unitário |
| CA-04 | Repetir a mesma transição não gera segundo e-mail | `Unit/Jobs/JobApplicationAggregateTests` (status igual lança) + `Unit/JobApplications/ChangeJobApplicationStatusHandlerTests` (nenhum evento enfileirado no caminho de falha) | Unitário |
| CA-05 | N1 no acto de candidatar-se + toast deixa de prometer notificação à empresa | `Unit/JobApplications/ApplyToJobHandlerTests` (evento `Reason=Applied`) + `frontend/tests/specs/unit/job-application-apply-feedback.feature` (texto do feedback) | Unitário + BDD |
| CA-06 | Encerrar vaga notifica cada candidatura em aberto | `Integration/Handlers/CloseJobHandlerTests` (N candidaturas abertas → N eventos `JobClosed`; Approved/Finished/Rejected intocadas) | Integração |
| CA-07 | Cancelamento pelo candidato gera confirmação | `Unit/JobApplications/CancelJobApplicationHandlerTests` (evento `Reason=CanceledByCandidate`) | Unitário |
| CA-08 | Nenhuma notificação contém dado de outro candidato | `Unit/JobApplications/JobApplicationStatusChangedEmailHandlerTests` — destinatário e conteúdo derivam do `CandidateUserId` do agregado, nunca da requisição | Unitário |
| CA-08b | Candidatura nasce "Recebida"; filtro "Recebida" devolve resultados; mover para "Em análise" dispara N2 | `Unit/Jobs/JobApplicationAggregateTests` (status inicial) + `Integration/Handlers/GetMyJobApplicationsHandlerTests` (filtro `Pending`) + `frontend/tests/specs/unit/application-status-vocabulary.feature` | Unitário + Integração + BDD |
| CA-09 | Várias candidaturas no mesmo dia: teto anti-abuso não descarta estas notificações | `Unit/Email/JobApplicationEmailServiceTests` — `IEmailThrottleService` **não** é invocado neste caminho | Unitário |

### Cancelamento

| CA | Critério (resumo) | Onde se verifica | Tipo |
| -- | ----------------- | ---------------- | ---- |
| CA-10 | Em análise: candidato cancela, status passa a Cancelada e persiste | `Unit/Jobs/JobApplicationAggregateTests` (`CancelByCandidate` a partir de Pending/Processing) + `Integration/Api/JobApplicationsCancelEndpointTests` (200 e leitura subsequente) | Unitário + Integração |
| CA-11 | Cancelar recusado em Aprovada, Reprovada, Concluída, Cancelada, Expirado, Erro | `Unit/Jobs/JobApplicationAggregateTests` — `[Theory]` sobre todos os status não canceláveis + `frontend/tests/specs/unit/application-status-vocabulary.feature` (acção ausente) | Unitário + BDD |
| CA-12 | Cancelar exige confirmação; abandonar não altera nada | `frontend/tests/specs/integration/my-applications-cancel.feature` | BDD |
| CA-13 | Não cancela candidatura de outrem, e a recusa não revela existência | `Integration/Api/JobApplicationsCancelEndpointTests` — candidatura de outro utilizador devolve **404**, nunca 403; staff de recrutamento também recusado | Integração |
| CA-14 | Após cancelar, consegue candidatar-se de novo se a vaga estiver activa | `Integration/Handlers/ApplyToJobHandlerTests` (cancelada não conta como duplicata; vaga encerrada continua a recusar) | Integração |
| CA-15 | Recrutador vê "cancelada pelo candidato", distinta da da empresa | `Integration/Handlers/GetJobApplicationsByJobIdHandlerTests` (status devolvido) + `frontend/tests/specs/unit/application-status-vocabulary.feature` (rótulo por perfil) | Integração + BDD |
| CA-16 | Candidatura cancelada não conta como activa no funil | `Unit/Dashboard/GetDashboardOverviewHandlerTests` — etapa "Candidaturas" exclui `Canceled` e `CanceledByCandidate` | Unitário |

### Encerramento de vaga

| CA | Critério (resumo) | Onde se verifica | Tipo |
| -- | ----------------- | ---------------- | ---- |
| CA-17 | Encerrar pede confirmação e informa quantas candidaturas serão afectadas | `Unit/Jobs/GetJobByIdHandlerTests` (`openApplicationsCount` conta só Pending+Processing) + `frontend/tests/specs/integration/job-close-confirmation.feature` | Unitário + BDD |
| CA-18 | Tela mostra o estado da vaga e não oferece "encerrar" em vaga encerrada | `frontend/tests/specs/integration/job-close-confirmation.feature` | BDD |

## 2. Cobertura por camada

| Camada | Alvos |
| ------ | ----- |
| Domínio | Status inicial `Pending`; `CancelByCandidate` e os seus estados permitidos; recusa de `CanceledByCandidate` via `ChangeStatus`; recusa de transição a partir de `CanceledByCandidate` |
| Application | Enfileiramento do evento em cada um dos quatro handlers; handler de e-mail que nunca relança; projecção do payload a partir do agregado |
| Infra | `NotificationDispatchBehavior` (drena só após commit) e `DomainEventQueue` (escopo por requisição) |
| Api | Endpoint de cancelamento: posse, códigos, uniformidade do 404 |
| Frontend | Vocabulário de status (incluindo o novo valor no `z.enum`), acção e modal de cancelamento, confirmação de encerramento, texto do feedback de candidatura |

## 3. Gaps e riscos de cobertura

1. **Entrega real de e-mail não é verificada por teste.** Os testes cobrem *que o envio foi solicitado com o
   conteúdo certo*, não que chegou a uma caixa. Em desenvolvimento o transporte é `NoOpEmailSender`; sem o
   sender de desenvolvimento previsto no `tasks.md`, nenhum critério de notificação é observável na UI.
2. **Ordem dos behaviors é a hipótese mais frágil do design.** O teste de rollback (CA-03) é o que a
   protege; se a composição do mediator não permitir posicionar o despacho fora da transacção, o desenho
   muda e isso tem de ir para as *deviation notes* do `tasks.md`, não ser contornado em silêncio.
3. **Regressão do funil.** CA-16 muda números já exibidos no dashboard. O teste fixa o comportamento novo,
   mas quem lê a métrica precisa de ser avisado — é risco de negócio, não de código.
4. **Candidaturas históricas em `Processing`** não são migradas (design §7). Nenhum teste cobre "o que o
   utilizador vê em dado antigo"; a verificação é a regressão pela UI real.
5. **Verificação ponta-a-ponta** (candidato recebe, cancela, recrutador vê) fica com a
   [`e2e-qa-skill`](../../../.claude/skills/e2e-qa-skill/SKILL.md), sobre o ambiente com o sender de
   desenvolvimento activo. Os dados `[QA]` da bateria de 2026-09-01 continuam disponíveis
   (§3 do relatório de 2026-09-01 em `docs/qa/`, não versionado).
6. **Acoplamento de deploy** (design §2.1): não há teste que impeça publicar a API sem o frontend. É
   restrição de release, e fica registada no `tasks.md`.
