# ADR 0003: Teto diário de e-mails transacionais por destinatário

## Status
Aceite

## Contexto
Os endpoints `forgot-password` e `resend-email-confirmation` disparam um e-mail real (via SMTP/Brevo) a cada chamada bem-sucedida, sem qualquer memória de envios anteriores. O rate limit por IP (ADR 0002) desacelera um único atacante, mas não impede o abuso: um IP respeitando o limite de 5 req/10s ainda consegue gerar dezenas de milhares de e-mails por dia, e um atacante com múltiplos IPs multiplica isso.

Duas consequências práticas de um ataque assim: (1) custo direto — o plano gratuito do provedor de e-mail transacional se esgota em minutos, e um plano pago vira custo crescente sob controle do atacante; (2) email bombing — a caixa de entrada de uma vítima específica pode ser inundada usando o próprio `forgot-password` como arma, sem que a vítima tenha feito nada.

A resposta desses endpoints já era uniforme por design ("Se existir uma conta para este e-mail...") para não vazar se um e-mail existe na base. Qualquer solução de throttle precisa preservar essa uniformidade — sinalizar ao chamador que ele foi limitado reabriria o vazamento por um caminho lateral.

## Decisão
- Cada destinatário tem um **teto de 5 e-mails por dia** (`EmailThrottle:MaxPerDay`, configurável), controlado por `IEmailThrottleService.TryAcquireAsync(email)`.
- Implementação dupla: Redis (`INCR` atômico + expiração de 25h, para múltiplas instâncias e persistência entre restarts) quando disponível; fallback em memória (processo único) quando Redis está desabilitado.
- Quando o teto é atingido, o handler **omite o envio silenciosamente** e devolve a mesma mensagem pública de sempre — o chamador não recebe nenhum sinal de que foi limitado.
- Aplicado apenas onde o volume é controlável pelo atacante contra uma conta já existente (`forgot-password`, `resend-email-confirmation`). Não aplicado ao `register`: cada e-mail novo só gera um envio (duplicata é rejeitada antes de qualquer e-mail sair).

## Consequências

**Positivas:**
- Custo de envio por ataque passa a ser limitado a `5 × número de contas reais atacadas`, independente de quantos IPs o atacante controle.
- Email bombing de uma vítima específica é limitado a 5 e-mails/dia, não importa quantas vezes o endpoint seja chamado.
- A proteção anti-enumeração de contas permanece intacta — a resposta pública não muda quando o throttle age.

**Negativas / cuidados:**
- Um usuário legítimo que peça reset de senha 6+ vezes no mesmo dia (esqueceu de novo, tentou de outro dispositivo) simplesmente não recebe mais e-mails, sem aviso — isso é intencional (não vazar o motivo), mas é uma fonte real de "não estou recebendo o e-mail" em suporte.
- Sem Redis ativo, o teto vive em memória de processo único: reinício da API zera os contadores, e múltiplas instâncias não compartilham o teto entre si (o abuso ainda tem uma pequena folga extra proporcional ao número de instâncias).
