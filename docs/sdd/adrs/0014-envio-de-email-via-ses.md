# ADR 0014: Envio transacional pela API do Amazon SES, não por relay SMTP

## Status
Aceite

## Contexto
Todo o e-mail transacional da API — reset de senha, confirmação de conta e as notificações de andamento de
candidatura N1–N7 do [ADR 0011](0011-notificacoes-de-andamento-fora-do-teto-de-emails.md) — saía por
`SmtpEmailSender` (MailKit) contra um relay SMTP genérico, configurado pela secção `Smtp` com host, porta,
modo de TLS, utilizador e palavra-passe.

Três problemas concretos com esse arranjo, dado que a API já corre em EC2 dentro da AWS:

1. **Credencial estática obrigatória.** `Smtp:UserName` e `Smtp:Password` tinham de existir como secrets do
   GitHub Environment, ser escritos no ficheiro `.env` publicado por SSM e viver no ambiente do container.
   São credenciais de longa duração, sem rotação automática, que dão a quem as leia a capacidade de enviar
   e-mail em nome do domínio — e que se espalham por tantos sítios quantos forem os ambientes. Em
   desenvolvimento, o caminho de menor resistência era escrevê-las em `appsettings.Development.json`, e era
   isso que estava lá: só o `.gitignore` de `appsettings*.json` é que separava a credencial do repositório.
2. **Nenhuma ligação entre o envio e o seu destino.** O SMTP devolve "aceite" e mais nada. Não há
   identificador para correlacionar um envio com o bounce ou complaint que ele provoca, e o ADR 0011 já
   registou que o custo desta família de e-mails não tem limite superior no código — descobre-se pela
   factura. Sem `MessageId` não há como sequer começar a instrumentar isso.
3. **Dependência de saída SMTP.** A porta 587/465 tem de estar aberta no security group e não ser
   estrangulada pelo provedor. É uma superfície de rede a manter para uma coisa que a AWS já expõe por HTTPS.

## Decisão
- O transporte de `IEmailSender` passa a ser `SesEmailSender`, usando a **API do SES v2**
  (`AWSSDK.SimpleEmailV2`, `SendEmailAsync`). `SmtpEmailSender`, `SmtpEmailOptions`, `MailKit` e `MimeKit`
  saem da solução — não ficam como transporte alternativo.
- **A autenticação usa a cadeia de credenciais padrão da AWS**: IAM role da instância EC2 em produção,
  perfil ou variáveis `AWS_*` localmente. A secção de configuração `Ses` (`Enabled`, `Region`, `FromEmail`,
  `FromName`) **não contém segredo nenhum** — `SES_USERNAME`/`SES_PASSWORD` não existem, e os secrets
  `SMTP_USERNAME`/`SMTP_PASSWORD` saem dos workflows.
- **`Ses:Region` é obrigatório em `Production`** e falha no boot se ausente. A identidade do remetente é
  verificada por região no SES: deixar o SDK resolver a região sozinho transforma uma mudança de região da
  instância numa falha de envio em runtime, em vez de um erro de configuração no arranque.
- A escolha dev/no-op mantém-se onde estava e com a mesma forma: `Ses:Enabled=false` resolve para
  `DevelopmentLogEmailSender` em `Development` e `NoOpEmailSender` nos restantes ambientes — a distinção que
  o task 0.1 de `emp-acompanhamento-candidatura` introduziu deliberadamente, porque o corpo do e-mail
  carrega tokens vivos.
- As falhas de primeira configuração do SES (identidade inexistente na região, recusa por sandbox, envio
  suspenso) são traduzidas em mensagens accionáveis; o resto cai numa mensagem genérica. O tipo lançado
  continua a ser `InvalidOperationException`, para que os handlers que já capturam falha de e-mail
  (`UserRegisteredEmailHandler`, `ResendEmailConfirmationCommand`, `JobApplicationStatusChangedEmailHandler`)
  se comportem exactamente como antes.

## Consequências

**Positivas:**
- Deixa de existir credencial de e-mail para gerir, rodar ou vazar. A permissão é `ses:SendEmail` na role da
  instância, revogável em IAM sem redeploy.
- Cada envio passa a ter `MessageId` no log — a chave que permite, quando for necessário, ligar um envio ao
  evento de bounce/complaint do SES e finalmente fechar a lacuna de custo apontada no ADR 0011.
- Menos duas dependências (`MailKit`, `MimeKit`) e uma porta de saída menos no security group.

**Negativas / cuidados:**
- **O deploy passa a exigir uma IAM role com `ses:SendEmail` na instância.** Se a role não tiver a permissão,
  a API sobe normalmente e falha no primeiro envio — a validação de arranque confere configuração, não
  permissão. Vale confirmar com um envio real após o primeiro deploy.
- **A identidade do remetente tem de estar verificada na região de `Ses:Region`,** e a conta tem de ter saído
  do sandbox do SES; enquanto estiver no sandbox só entrega a endereços verificados. É o modo de falha mais
  provável do primeiro deploy e por isso tem mensagem própria.
- **As credenciais do relay antigo continuam válidas até serem revogadas no provedor.** Deixaram de ser
  lidas pela aplicação, mas continuam a poder enviar e-mail em nome do domínio para quem as tenha na sua
  máquina; o mesmo vale para os secrets `SMTP_USERNAME`/`SMTP_PASSWORD` que ficam órfãos nos GitHub
  Environments. Revogar num sítio e apagar no outro é trabalho de operação, fora deste diff.
- **Bounce e complaint continuam sem tratamento.** O SES publica esses eventos via SNS e cobra reputação por
  eles; hoje ninguém os consome, e um endereço morto continua a ser tentado a cada notificação. Não se
  constrói agora por não haver consumidor (YAGNI). **Gatilho de retorno:** a primeira taxa de bounce que o
  console do SES sinalize, ou a chegada de preferências de notificação (já apontada como gatilho no ADR 0011).
- Nenhum `ConfigurationSet` é enviado no pedido, o que significa que não há métricas de entrega por conjunto
  de configuração. É uma linha de código quando houver quem leia essas métricas.
