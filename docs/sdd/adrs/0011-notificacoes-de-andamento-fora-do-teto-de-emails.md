# ADR 0011: Notificações de andamento de candidatura fora do teto diário de e-mails

## Status
Aceite

## Contexto
O [ADR 0003](0003-teto-diario-de-emails-por-destinatario.md) instituiu um teto de 5 e-mails por dia por destinatário (`IEmailThrottleService`), com **descarte silencioso** quando o teto é atingido. Foi a decisão certa para `forgot-password` e `resend-email-confirmation`: nesses fluxos o volume é disparado por um **anónimo**, o e-mail é uma arma potencial contra a vítima, e a resposta pública já tinha de ser uniforme por anti-enumeração — omitir o envio não muda nada do que o chamador vê.

A feature de acompanhamento de candidatura (`emp-acompanhamento-candidatura`) introduziu uma segunda família de e-mails transacionais, com propriedades diferentes em três eixos:

1. **Quem gera o volume.** Cada e-mail corresponde a um ato **autenticado**: o candidato candidatou-se ou desistiu, ou a empresa moveu o processo. Não há caminho anónimo para os disparar, e o domínio já limita a repetição — `JobApplication.ChangeStatus` recusa transição para o status atual, portanto clicar duas vezes não gera dois e-mails.
2. **O que o silêncio custa.** Em `forgot-password`, um e-mail descartado significa "tenta outra vez amanhã". Aqui significaria o candidato **nunca** saber que foi aprovado — a notificação não se repete, porque a mudança de status não volta a acontecer. Contraria CA-09 do PRD directamente.
3. **Qual é o volume realista.** Um candidato activo com várias candidaturas em curso pode legitimamente receber mais de 5 e-mails num dia em que várias empresas avancem os processos — por exemplo quando uma vaga é encerrada e arrasta todas as candidaturas em aberto. O teto de 5 seria atingido por uso **normal**, não por abuso.

Passar estas notificações pelo mesmo balde teria ainda um efeito de contaminação cruzada: um pedido de reset de senha gastaria orçamento da notificação de aprovação, e vice-versa.

## Decisão
- `JobApplicationEmailService` (as notificações N1–N7 de andamento de candidatura) **não** consulta `IEmailThrottleService`. A dependência não é injectada — o serviço não tem como consultar o teto, e há um teste estrutural que falha se alguém a acrescentar.
- O `IEmailThrottleService` mantém-se **exactamente** como está e onde está: `forgot-password` e `resend-email-confirmation`. O ADR 0003 continua válido no seu âmbito; este ADR delimita esse âmbito em vez de o revogar.
- O critério que separa os dois casos, para decisões futuras: **o teto anti-abuso aplica-se onde um anónimo controla o volume e o descarte silencioso é aceitável**. Onde o volume é consequência de atos autenticados e o e-mail é a única forma de a pessoa saber de um facto que não se repete, o teto protege a coisa errada.
- Se o volume destas notificações vier a ser um problema de custo, o limite correcto é **agregado por remetente/período** (proteger o orçamento total de envio), não um balde por destinatário partilhado com o fluxo de recuperação de acesso.

## Consequências

**Positivas:**
- Nenhuma notificação de andamento é descartada em silêncio: o candidato que foi aprovado é informado, o que é o objectivo da feature.
- O orçamento anti-abuso do `forgot-password` deixa de ser consumido por e-mails legítimos de candidatura, e vice-versa — os dois fluxos param de interferir um no outro.
- O critério de aplicação fica escrito. Sem isto, a próxima família de e-mails seria ligada ao throttle por analogia superficial ("é e-mail transacional, logo passa pelo teto") ou deixada de fora sem fundamento.

**Negativas / cuidados:**
- **O custo de envio desta família não tem limite superior no código.** É limitado pelo volume de atos autenticados, o que é razoável em operação normal, mas um recrutador que mova centenas de candidaturas em minutos gera centenas de e-mails, e uma vaga muito procurada que seja encerrada gera um e-mail por candidatura em aberto. Não há hoje alerta de custo; se o plano do provedor for excedido, descobre-se pela factura ou pela falha de envio.
- Um candidato com muitas candidaturas activas pode achar o volume incómodo, e **não existe preferência de notificação** — ele não tem como reduzir a frequência nem desligar canais. É lacuna conhecida, adiada por não haver segundo canal (PRD §7); a chegada de preferências de notificação é o gatilho para revisitar.
- A garantia de "não duplica" depende de invariantes do domínio (`ChangeStatus` recusa status igual; `CancelByCandidate` recusa estado não cancelável) e da semântica de conjunto de `IDomainEventQueue`, e não de uma tabela de controlo de envios. Se alguma dessas guardas for relaxada, a duplicação volta sem nada no fluxo de e-mail a travá-la.
- Retirar o throttle deste caminho remove também a proteção contra um **cenário interno** de abuso: uma conta de recrutamento comprometida pode gerar e-mails em volume dentro do que a API considera legítimo. Mitigação existente é apenas o RBAC do endpoint de status.
