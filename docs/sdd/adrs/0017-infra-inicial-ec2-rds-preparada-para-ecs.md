# ADR 0017: Infraestrutura inicial em EC2 + RDS preparada para ECS; Fargate adiado com gatilho de retorno

## Status

Aceite. Decisões pendentes no fim do documento.

## Contexto

A plataforma vai para produção com **baixo volume, poucos dados e orçamento reduzido**, numa conta AWS
dedicada ao cliente, e precisa poder evoluir para ECS/Fargate com Auto Scaling sem reestruturar a aplicação.
A janela de operação é **07h–00h** (`America/Sao_Paulo`); entre 00h e 07h a indisponibilidade é intencional.

Estado do repositório em 2026-10-19 (evidência, não suposição):

- API em contêiner `aspnet:10.0-alpine`, deploy por SSM com canário em `.github/scripts/deploy-container.sh`;
  a imagem roda em `t4g` (arm64), logo serve também ao Fargate ARM.
- O PostgreSQL roda em contêiner na mesma EC2 (`DB_HOST=172.17.0.1`), sem backup. Não há RDS.
- O frontend (Next 16, SSR, `proxy.ts`, `'use cache'`) não tem Dockerfile; hoje está no Vercel, que reescreve
  `/api/*` para a API.
- Os cookies de autenticação são host-only, sem `Domain` (`AuthCookieService`), e o `proxy.ts` os lê no
  servidor Next. API e frontend precisam, portanto, estar na **mesma origem**.
- **Não há `AddDataProtection`** em `backend/src`: as chaves vivem no sistema de ficheiros do contêiner e
  somem a cada recriação, o que quebra links de confirmação de e-mail e de redefinição de senha.
- Cache de saída, teto diário de e-mails e rate limit são por processo; Redis é opcional
  (`Redis:Enabled`), mas o workflow `cd-ec2.yml` assume `true` por omissão.
- Migrações são aplicadas no boot (`ApplyPendingMigrations` incondicional, validadas pelo canário de
  `deploy-container.sh` antes da promoção); release com `RenameColumn`/`DropColumn` só avança, porque o
  rollback devolve uma imagem antiga a um schema que já avançou (aviso no resumo do job de `cd-ec2.yml`).

Foram comparados três cenários com as mesmas premissas (30 dias, ~518 h de computação e ~533 h de RDS por
mês, ~52 mil requisições, `db.t4g.micro` Single-AZ de 20 GB), em sa-east-1, com 13,6 % de tributos da AWS e
PTAX R$ 5,0521. Preços da Price List da AWS de 16/10/2026; IPv4 e VPC endpoint reconfirmados em 19/10/2026.

| Cenário | R$/mês (sa-east-1) | R$/mês (us-east-1) | Residual entre 00h e 07h |
| ------- | -----------------: | -----------------: | -----------------------: |
| A: EC2 única com Docker + RDS | 343 | 206 | 71 |
| B: ECS Fargate econômico (API e Web, ALB, RDS) | 569 | 367 | 219 |
| C: Fargate com 2 tarefas por serviço, Multi-AZ, Redis e WAF | 1 118 | 757 | 419 |

O ALB, que não pode ser desligado, só apagado, responde por cerca de R$ 190 dos R$ 226 que separam o B do A.

## Decisão

1. **Cenário A agora:** uma EC2 `t4g.medium` com Caddy (TLS), API e Next em contêineres, e um **RDS PostgreSQL
   `db.t4g.micro` Single-AZ** privado, com TLS, criptografia e backup automático. Conta AWS dedicada ao cliente.
   `t4g.small` fica como redução possível se a medição de memória a permitir.
2. **Preparada para ECS** — o que custa pouco hoje e evita reconstruir depois:
   - chaves do Data Protection persistidas no banco;
   - nenhum estado local; segredos no SSM; imagens no ECR; autenticação do deploy por OIDC;
   - API e frontend na mesma origem, com `/api/*` roteado para a API (Caddy hoje, regra de caminho do ALB
     depois);
   - Dockerfile do frontend com `output: 'standalone'` e endpoint de saúde próprio;
   - `Redis:Enabled=false` explícito, com os efeitos por processo documentados.
3. **Agenda de custo:** RDS liga às 06:30 e EC2 às 06:45; EC2 para às 00:00 e RDS às 00:10 (EventBridge
   Scheduler, fuso `America/Sao_Paulo`). O RDS parado cobra storage e backup, não instância, e a AWS o
   religa sozinha após 7 dias parado; a agenda diária contorna isso, mas uma falha em "ligar" deixa o site
   fora do ar às 07h.
4. **Sem NAT e sem VPC endpoints de interface** quando houver Fargate: as tarefas ficam em subnet pública
   com IPv4 e security group aberto só para o ALB. Três endpoints custariam ~R$ 264/mês.
5. **Sem Vercel** no destino. Mantê-lo é uma opção só enquanto o frontend não for containerizado (ver
   "Adiado").

## Adiado — com gatilho de retorno

| Capacidade | Gatilho |
| ---------- | ------- |
| **ECS Fargate (cenário B)**, com a agenda por Application Auto Scaling (`min=max=0` às 00h, `min=1,max=3` às 06:50) e metas de CPU 55–60 % e memória 70 % | Qualquer um, por duas semanas: CPU p95 acima de 60 % ou memória acima de 75 % na `t4g.medium` em horário comercial; necessidade de uma `t4g.large` (o A passa a ~R$ 502 e converge para o B); mais de uma instância; três ou mais serviços na mesma conta; deploy sem queda como requisito |
| **Alta disponibilidade (cenário C)**: 2 tarefas por serviço, Multi-AZ, Redis e WAF | Cláusula contratual de disponibilidade; parada de instância ou zona inaceitável para o negócio; RTO em horas incompatível; carga que exija 2 ou mais tarefas permanentes |
| **Redis** | Mais de uma tarefa ou instância servindo tráfego (o teto de e-mails e o cache de saída passam a divergir por processo) |
| **RDS maior** | CPU do RDS acima de 60 %, memória livre abaixo de 15 % ou conexões perto do `max_connections` real, de forma sustentada. `db.t4g.small` Single-AZ: +R$ 107/mês |
| **RDS Multi-AZ** | Exigência de failover automático do banco; `micro` Multi-AZ: +R$ 172/mês |
| **ALB recriado diariamente** para poupar ~R$ 56/mês | Não recomendado: muda o DNS do balanceador e cria risco às 07h |
| **API Gateway HTTP API + VPC Link + Cloud Map** no lugar do ALB | Só após prova de conceito (streaming do Next, WAF, preço); nenhum desses pontos foi verificado |

## Consequências

- O custo inicial fica em ~R$ 343/mês (sa-east-1), contra ~R$ 569 do B. A diferença, ~R$ 2,7 mil por ano, só se
  justifica quando um gatilho da tabela acima disparar.
- Chaves persistidas no banco passam a ser pré-requisito de qualquer migração para tarefas efémeras; o
  trabalho entra já, no cenário A.
- A migração A → B muda pipeline (task definitions, serviços, escala), ALB e DNS; o código e o banco ficam
  iguais. O canário atual do `deploy-container.sh` precisa ser refeito (tarefa única de verificação ou
  circuit breaker do ECS). A política de release destrutivo continua forward-only.
- Deploy fora da janela falha no A (o SSM não alcança a EC2 parada) e é sem efeito no B com contagem zero.
  Deploys acontecem entre 07h e 00h.
- Com várias tarefas, o limite por IP, o teto diário de e-mails e o cache de saída passam a ser por tarefa
  até existir Redis; aceitável no B, motivo para Redis no C.
- A escolha de região continua jurídica (LGPD, art. 33): us-east-1 reduz o custo em cerca de 40 %, mas
  produção com dados pessoais usa sa-east-1 até haver decisão em contrário.
- Replicar contas por cliente à mão não escala; a decisão anterior de não usar IaC valia para o ambiente
  de aprendizagem e deve ser reavaliada (CloudFormation ou CDK) antes da segunda conta de cliente.

## Premissas e limites da estimativa

Valores verificados: preços unitários de EC2, RDS, Fargate, ALB, EBS, IPv4 e endpoint de interface. Vêm de
sessões anteriores e não foram reconferidos: ElastiCache, WAF, CloudWatch em sa-east-1 e SES. Não foi possível
confirmar o preço do NAT em sa-east-1. Não foram medidos: memória e CPU reais, tempo de religação do RDS,
volume real de e-mails e logs. Se as tarefas Fargate precisarem de 1 vCPU e 2 GB cada, o B sobe para ~R$ 771.
O A com `t4g.small` custaria ~R$ 263, e com `t4g.large`, ~R$ 502.

## Decisões pendentes

1. Região de produção (sa-east-1 ou us-east-1), a decidir com o jurídico.
2. Titular da conta AWS e quem paga (Mariliz com repasse ou a prefeitura).
3. Domínio e nomenclatura de produção.
4. Medição de memória e carga antes de fixar `t4g.medium`.
5. Tempo real de religação do RDS, para fixar os horários da agenda.
