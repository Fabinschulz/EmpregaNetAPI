---
version: 1.1.0
date: 2026-09-01
status: Approved
---

# PRD — Acompanhamento da candidatura pelo candidato

**feature-id:** `emp-acompanhamento-candidatura`

Entrega duas capacidades do mesmo workflow — o candidato **fica sabendo** o que acontece com a sua
candidatura e **pode agir** sobre ela — além de registar a decisão de produto sobre o encerramento de vaga.

## 1. Problema e motivação

Hoje o processo de recrutamento avança sem que o candidato seja informado, e ele não tem nenhum controle
sobre a própria candidatura. A regressão E2E de 2026-09-01
constatou, navegando o produto real (relatório em `docs/qa/`, **não versionado** — evidência de execução,
local à máquina de quem correu a bateria):

1. **Nenhuma comunicação existe.** Ao mudar o status da candidatura, nada é enviado ao candidato — sem
   e-mail, sem notificação in-app, sem mensagem. O candidato só descobre a decisão se entrar no site e
   abrir "Minhas candidaturas". Um candidato aprovado pode não saber que foi aprovado.
2. **O produto afirma algo que não acontece.** Ao candidatar-se, a interface diz "A empresa foi notificada
   da sua candidatura". Nenhuma notificação é disparada. É uma promessa falsa ao utilizador.
3. **O candidato não tem saída.** Não existe nenhuma ação sobre a própria candidatura: não pode cancelar
   nem retirar. Quem já aceitou outra proposta continua a constar como candidato ativo, e o recrutador
   trabalha sobre um funil poluído por candidatos que já saíram do mercado.
4. **Assimetria de controle.** A empresa decide tudo (aprovar, reprovar, cancelar, concluir); o candidato
   não decide nada sobre um processo que é sobre ele.

O custo disso é duplo: **experiência** (o candidato percebe o produto como um buraco negro e não volta) e
**qualidade do funil** (o recrutador decide sobre candidaturas que já não têm interesse real).

## 2. Decisões de produto que delimitam esta feature

Tomadas pelo responsável de produto em 2026-09-01, antes da Fase 2:

| # | Decisão |
| - | ------- |
| D1 | **Canal único: e-mail.** WhatsApp não entra nesta feature (§6, com gatilho de retorno). |
| D2 | **Cancelamento até "Em análise".** Depois de aprovada, a candidatura é da empresa; o candidato não se retira pelo produto. |
| D3 | **Recandidatura permitida** à mesma vaga após cancelar, enquanto a vaga estiver ativa. |
| D4 | **Vaga encerrada não volta a ser ativada** — estado terminal por design. |
| D5 | **A candidatura nasce em "Recebida"** e passa a "Em análise" por ato do recrutador. Hoje ela nasce diretamente em "Em análise", o que torna N1 e N2 o mesmo instante e deixa o status "Recebida" sem uso. Com esta decisão, o candidato passa a saber quando alguém de facto pegou o seu processo. |

## 3. Personas e RBAC

| Persona | Pode |
| ------- | ---- |
| **Candidato** | Receber por e-mail as notificações do andamento da sua candidatura; **cancelar a própria candidatura** enquanto ela estiver em Recebida ou Em análise |
| **Recrutador / Gestor** | Continuar a mudar o status das candidaturas das vagas da sua empresa (já existe); ver que uma candidatura foi **cancelada pelo candidato**, distinguindo-a de um cancelamento feito pela empresa |
| **Administrador** | Tudo o que o recrutador pode |
| **Visitante (sem sessão)** | Nada — nenhuma notificação e nenhuma ação |

Regras de autorização de negócio:

- **RBAC-1** — Um candidato só pode cancelar candidatura **da qual ele é o autor**. Tentativa sobre a
  candidatura de outra pessoa é negada, e a resposta não deve revelar se a candidatura existe.
- **RBAC-2** — Recrutador **não** cancela em nome do candidato: o cancelamento pela empresa e o
  cancelamento pelo candidato são atos distintos, de autores distintos, e ficam distinguíveis no histórico.
- **RBAC-3** — Notificação é sempre dirigida ao candidato dono da candidatura; nenhum dado de outro
  candidato pode aparecer numa notificação.

## 4. Workflows

### 4.1 Notificação do andamento (e-mail)

| # | Evento | Notifica o candidato |
| - | ------ | -------------------- |
| N1 | Candidatura registada (confirmação de recebimento) | Sim |
| N2 | Candidatura passa a **Em análise** | Sim |
| N3 | Candidatura **Aprovada** | Sim |
| N4 | Candidatura **Reprovada** | Sim |
| N5 | Candidatura **Concluída** | Sim |
| N6 | Vaga **encerrada pela empresa** com candidatura em aberto | Sim |
| N7 | Candidatura **cancelada pelo candidato** (confirmação do próprio ato) | Sim |

Regras de negócio da notificação:

- **W1 — A notificação nunca bloqueia a decisão.** Se o envio falhar (serviço de e-mail fora do ar,
  endereço recusado), a mudança de status **conclui** normalmente. O recrutador nunca fica impedido de
  decidir por causa de uma mensagem.
- **W2 — Falha de envio não é silenciosa para a operação.** O insucesso fica registado de forma
  observável por quem opera o sistema, mesmo não sendo mostrado ao recrutador.
- **W3 — Cada evento notifica uma vez.** Uma mudança de status não pode gerar e-mails repetidos ao mesmo
  destinatário, mesmo que a operação seja tentada mais de uma vez.
- **W4 — Conteúdo mínimo de toda notificação:** título da vaga, nome da empresa, novo status e data. O
  candidato precisa reconhecer de que processo se trata sem abrir o site.
- **W5 — Estas notificações não podem ser descartadas pelo teto anti-abuso dos e-mails de conta.** O teto
  diário por destinatário existe para conter abuso de `forgot-password` e `resend-confirmation`
  ([ADR 0003](../../sdd/adrs/0003-teto-diario-de-emails-por-destinatario.md)), onde o volume é controlado
  por um atacante. Aqui o volume é controlado pela empresa e pelo próprio candidato, e descartar em
  silêncio significaria o candidato nunca saber que foi aprovado. Um teto próprio, com fundamento
  diferente, fica para a Fase 2.
- **W6 — Sem preferência de canal nesta entrega.** Todo candidato recebe as notificações do andamento das
  suas candidaturas; não há opção de desligar (§6).
- **W7 — "Recebida" passa a ser o estado inicial** (decisão D5). N1 confirma o recebimento; N2 só dispara
  quando o recrutador move para "Em análise". São dois momentos distintos do ponto de vista do candidato:
  "chegou" e "alguém está a olhar".

### 4.2 Cancelamento da candidatura pelo candidato

- **X1** — O candidato pode cancelar a sua candidatura enquanto ela estiver em **Recebida** ou
  **Em análise**.
- **X2** — A partir de **Aprovada** (inclusive), e em qualquer estado final (Reprovada, Concluída,
  Cancelada, Prazo expirado, Erro), o cancelamento é recusado — decisão D2.
- **X3** — O cancelamento **é terminal**: o candidato não "descancela".
- **X4** — Cancelar **libera o candidato para se candidatar de novo** à mesma vaga, enquanto a vaga
  estiver ativa — decisão D3. Sem isto, um clique acidental excluiria o candidato da vaga para sempre.
- **X5** — Toda ação de cancelamento pede **confirmação explícita** antes de executar, e diz ao candidato
  o que ele perde.
- **X6** — Depois de cancelada, a candidatura continua visível ao candidato com o status "Cancelada" e ao
  recrutador identificada como **cancelada pelo candidato** — não desaparece do histórico de nenhum dos dois.

### 4.3 Encerramento de vaga — decisão de produto registada

**Uma vaga encerrada NÃO volta a ser ativada** (decisão D4). O encerramento é um estado terminal por
design, e a ausência de reabertura deixa de ser tratada como defeito — reclassifica o BUG-01 do relatório
de QA de 2026-09-01.

Consequências que esta feature assume:

- **V1** — Como a ação não tem retorno, encerrar uma vaga passa a exigir **confirmação explícita**, dizendo
  quantas candidaturas em aberto serão afetadas.
- **V2** — A tela de gestão da vaga deve **mostrar o estado atual** da vaga (ativa / encerrada) e não
  oferecer "encerrar" numa vaga já encerrada.
- **V3** — Ao encerrar a vaga, as candidaturas em aberto passam a um estado que informa o candidato de que
  o processo terminou por decisão da empresa, e disparam N6 (§4.1).

## 5. Critérios de aceite

Verificáveis por observação do produto ou de teste automatizado.

**Notificação**

- **CA-01** — Dada uma candidatura em Em análise, quando o recrutador a move para Aprovada, então o
  candidato recebe um e-mail com o título da vaga, o nome da empresa, o novo status e a data.
- **CA-02** — O mesmo vale para Reprovada, Concluída e Em análise (N2–N5): cada transição gera o e-mail
  correspondente ao candidato dono da candidatura.
- **CA-03** — Com o serviço de e-mail indisponível, a mudança de status **conclui** com sucesso e a
  interface do recrutador não mostra erro; a falha de envio fica registada de forma observável.
- **CA-04** — Repetir a mesma tentativa de transição não gera um segundo e-mail do mesmo evento.
- **CA-05** — Ao candidatar-se, o candidato recebe o e-mail de confirmação de recebimento (N1) e a
  interface deixa de afirmar que a empresa foi notificada, passando a dizer o que realmente acontece.
- **CA-06** — Ao encerrar uma vaga com candidaturas em aberto, cada candidato afetado recebe o e-mail (N6).
- **CA-07** — Ao cancelar a própria candidatura, o candidato recebe a confirmação por e-mail (N7).
- **CA-08** — Nenhuma notificação contém dado de outro candidato.
- **CA-08b** — Uma candidatura recém-criada aparece com o status **"Recebida"** para o candidato e para o
  recrutador; ao ser movida para "Em análise", o candidato recebe o e-mail N2. O filtro "Recebida" nas
  listagens passa a devolver resultados.
- **CA-09** — Um candidato com várias candidaturas em movimento no mesmo dia recebe o e-mail de todas
  elas; o teto anti-abuso dos e-mails de conta não descarta estas notificações (W5).

**Cancelamento**

- **CA-10** — Numa candidatura Em análise, o candidato vê a ação de cancelar; ao confirmar, o status passa
  a Cancelada e a lista reflete isso após recarregar a página.
- **CA-11** — A ação de cancelar não aparece (ou é recusada) numa candidatura Aprovada, Reprovada,
  Concluída, Cancelada, Prazo expirado ou Erro.
- **CA-12** — Cancelar exige confirmação explícita; abandonar a confirmação não altera nada.
- **CA-13** — Um candidato não consegue cancelar candidatura de outra pessoa, e a recusa não revela se
  aquela candidatura existe.
- **CA-14** — Depois de cancelar, o candidato consegue candidatar-se de novo à mesma vaga, se ela estiver ativa.
- **CA-15** — O recrutador vê a candidatura como cancelada **pelo candidato**, distinguível de um
  cancelamento feito pela empresa.
- **CA-16** — Uma candidatura cancelada deixa de contar como candidatura ativa nas métricas do funil de
  recrutamento.

**Encerramento de vaga**

- **CA-17** — Encerrar uma vaga pede confirmação e informa quantas candidaturas em aberto serão afetadas.
- **CA-18** — A tela de gestão mostra o estado da vaga e não oferece "encerrar" numa vaga já encerrada.

## 6. Non-goals

Fora de escopo, explicitamente:

- **WhatsApp e qualquer canal fora o e-mail** (SMS, push) — decisão D1.
- **Reabertura de vaga** — decisão D4.
- **Chat ou mensagens bidirecionais** entre empresa e candidato. As notificações desta feature são
  unidirecionais e informativas.
- **Notificação in-app** (sino, central de avisos).
- **Preferência de canal / opt-out** das notificações de andamento.
- **Notificar o recrutador** de qualquer evento — o recrutador trabalha na aplicação e vê o estado na lista.
- **Templates de mensagem editáveis** pelo recrutador ou pela empresa.
- **Motivo ou justificativa da decisão** escrita pelo recrutador e exibida ao candidato.
- **Currículo / anexos** e qualquer outra alteração no perfil do candidato.

## 7. Adiado, com gatilho de retorno

| Item | Gatilho de retorno |
| ---- | ------------------ |
| **Canal WhatsApp** | Quando houver decisão de provedor com conta e credenciais reais, e teto de custo por mensagem definido. O desenho da Fase 2 não deve criar abstração de canal antecipando isto — um único canal não justifica a indireção |
| Preferência de canal / opt-out | Quando existir mais de um canal, ou quando houver reclamação real de volume de e-mail |
| Motivo da reprovação exibido ao candidato | Quando houver pedido de recrutador real ou reclamação recorrente sobre falta de retorno |
| Notificação in-app / central de avisos | Quando existir mais de um tipo de evento além do andamento da candidatura para notificar |
| Notificar o recrutador quando um candidato cancela | Quando um recrutador relatar que perdeu tempo com candidatura já cancelada |
| Digest (agrupar várias mudanças num só envio) | Quando o volume por candidato tornar o envio por evento incómodo |
| Reenvio manual de uma notificação que falhou | Quando houver evidência de falha de entrega com impacto reportado |

## 8. Riscos de negócio

- **Confiança**: hoje o produto já afirma uma notificação que não existe (CA-05). Entregar a notificação
  pela metade — prometer e falhar em silêncio — repete o mesmo dano de forma pior. Por isso W1 e W2
  separam "a decisão conclui" de "a falha fica visível para quem opera".
- **Entregabilidade**: o volume de e-mail transacional cresce com o funil. Domínio sem reputação de envio
  configurada faz a notificação cair em spam, o que equivale funcionalmente a não notificar.
- **Funil**: permitir cancelamento reduz o número de candidaturas ativas exibidas. Isso é ganho de
  qualidade, mas muda as métricas do dashboard (CA-16) e deve ser comunicado a quem lê esses números.
- **Janela de cancelamento estreita** (D2): o candidato que desiste após ser aprovado não tem caminho no
  produto e vai resolver por fora (telefone/e-mail direto), o que o recrutador pode não registar. Se isso
  se mostrar frequente, é gatilho para rever D2.
