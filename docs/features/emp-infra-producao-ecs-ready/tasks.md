---
version: 1.0.0
date: 2026-10-19
status: Draft
---

# Tasks — Infraestrutura de produção em EC2 + RDS, preparada para ECS

**feature-id:** `emp-infra-producao-ecs-ready` · **Decisão:** [ADR 0017](../../sdd/adrs/0017-infra-inicial-ec2-rds-preparada-para-ecs.md)

Não há `prd.md`, `design.md` nem `spec.md` nesta pasta: o contexto, as alternativas e os custos estão no
ADR 0017. Este plano cobre só o cenário A e o que o deixa pronto para ECS. O cenário B só vira tarefa
quando um gatilho do ADR disparar.

A homologação reaproveita os mesmos blocos numa conta separada, com a mesma agenda. Para ela se repetem
as tarefas 3.4 e 4.1 a 4.7; a decisão da região (0.1) não se aplica enquanto usar só dados sintéticos.

**Gate humano:** nada abaixo é executado sem aprovação deste plano. Os blocos 0 a 3 não alteram a
infraestrutura de produção. As tarefas 1.2 e 1.3 usam um RDS temporário de teste, apagado ao fim.

## Bloco 0 — Decisões e pré-requisitos (humano)

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 0.1 | Decidir a região de produção com o jurídico (LGPD, art. 33) | Registrada no ADR 0017 |
| 0.2 | Decidir o titular da conta AWS e quem paga | Registrada no ADR 0017 |
| 0.3 | Definir domínio e nomenclatura (frontend e API na mesma origem) | Hostname de produção e de homologação definidos |
| 0.4 | Decidir IaC (CloudFormation ou CDK) antes da segunda conta de cliente | Decisão registrada em ADR novo |
| 0.5 | Reconfirmar preços e câmbio no dia da criação da conta | Estimativa do ADR 0017 atualizada |
| 0.6 | Pedir a saída do sandbox do SES o quanto antes: o prazo de resposta não foi verificado e bloqueia a 4.8 | Pedido enviado, com domínio remetente definido em 0.3 |
| 0.7 | Registrar que os requisitos de LGPD (termos, consentimento, exportação e anonimização) e a revisão jurídica ficam fora desta feature, mas bloqueiam a entrada em produção | Acompanhados em feature própria ou no issue tracker |

## Bloco 1 — Aplicação (repositório)

| # | Tarefa | Camada | Critério de pronto |
| - | ------ | ------ | ------------------ |
| 1.1 | Persistir as chaves do Data Protection no banco (`PersistKeysToDbContext`) com migration aditiva | Infra | Link de confirmação de e-mail gerado antes de reiniciar o contêiner continua válido depois |
| 1.2 | Connection string com TLS (`SSL Mode`) via configuração; ajustar `cd-ec2.yml:151` | Pipeline | API conecta ao RDS de teste com TLS exigido |
| 1.3 | Validar as migrações (`unaccent`, `pg_trgm`, `CREATE TEXT SEARCH CONFIGURATION`) num RDS de teste | Infra | Todas as migrações aplicam do zero e `/health/ready` responde 200 |
| 1.4 | `REDIS_ENABLED=false` explícito no Environment e documentar os efeitos por processo | Pipeline | `cd-ec2.yml` não assume mais `true` quando a variável está ausente |

## Bloco 2 — Frontend

| # | Tarefa | Camada | Critério de pronto |
| - | ------ | ------ | ------------------ |
| 2.1 | `output: 'standalone'` em `next.config.ts` e `frontend/Dockerfile` (arm64, usuário não root) | Frontend | Imagem sobe localmente e serve `/` e `/vagas` |
| 2.2 | Endpoint de saúde do Next para o orquestrador | Frontend | Responde 200 sem depender da API |
| 2.3 | Confirmar `API_INTERNAL_BASE_URL` e `NEXT_PUBLIC_API_BASE_URL` no cenário de mesma origem | Frontend | Login, cookie `Secure` e `proxy.ts` funcionam atrás do Caddy |

## Bloco 3 — Pipeline

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 3.1 | Job de imagem do frontend (CI → ECR, tag imutável) | Imagem `web` publicada por commit |
| 3.2 | Ajustar `deploy-container.sh` para Caddy + API + Next, mantendo o canário da API; `HOST_PORT` e o health check não podem divergir | Deploy completo com rollback da API testado |
| 3.3 | Deploy restrito à janela 07h–00h, com falha clara fora dela | Mensagem explícita no job quando a EC2 está parada |
| 3.4 | Environments `Homolog` e `Production` com OIDC, sem chave estática | Nenhum `AWS_ACCESS_KEY_ID` nos secrets dos dois |
| 3.5 | Versionar o `Caddyfile` e o arquivo de composição dos contêineres no repositório (sem segredos) e fazer o deploy usá-los | Mudança de roteamento passa por PR, não por edição manual na EC2 |

## Bloco 4 — AWS (cenário A)

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 4.1 | Conta dedicada ao cliente, MFA, budgets de 80 % e 100 %, tags de alocação de custo ativadas antes dos recursos | Tags aparecem no Cost Explorer |
| 4.2 | RDS `db.t4g.micro` Single-AZ, 20 GB, privado, criptografado, backup de 7 dias, deletion protection | Acesso só do security group da EC2 |
| 4.3 | EC2 `t4g.medium`, IP elástico, security group 443, SSM sem SSH, IMDSv2, EBS criptografado | Instância gerenciável só por SSM |
| 4.4 | Route 53, Caddy com TLS automático e `/api/*` para a API | HTTPS válido e cookie `Secure` aceito |
| 4.5 | `ForwardedHeaders:KnownNetworks` com a rede do Caddy (ADR 0004) e `RateLimiting` coerente | Rate limit enxerga o IP real |
| 4.6 | EventBridge Scheduler: RDS 06:30 / EC2 06:45, EC2 00:00 / RDS 00:10, fuso `America/Sao_Paulo`; validar `startDBInstance` e `stopDBInstance` como alvo | Um ciclo completo executado sem intervenção |
| 4.7 | Alarmes: status da instância, créditos de CPU, memória e disco (CloudWatch Agent), CPU, memória livre e espaço do RDS; ações de alarme desativadas à noite | Alarme de teste dispara e chega ao destino |
| 4.8 | Domínio verificado no SES (DKIM) e envio testado após a saída do sandbox pedida em 0.6 | Entrega a destinatário não verificado |

## Bloco 5 — Verificação e medição

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 5.1 | Medir memória e CPU reais (`docker stats`, CloudWatch) e rodar teste de carga (k6: 70 % leitura anônima, 20 % autenticada, 10 % escrita, rampa até 50 req/s) | Tamanho da instância confirmado ou reduzido para `t4g.small` |
| 5.2 | Teste de restauração do RDS a partir de snapshot | Banco restaurado e aplicação conectando |
| 5.3 | Medir o tempo real de religação do RDS e da aplicação | Horários da agenda ajustados no ADR 0017 |
| 5.4 | Ciclo completo 00h → 07h acompanhado, incluindo falha simulada da regra de ligar | Procedimento de recuperação escrito |
| 5.5 | Smoke e QA E2E com dados sintéticos | Fluxos de candidato e recrutador passam |
| 5.6 | Com os números de 5.1: rever `DOTNET_gcServer=1`, fixar teto de heap da API e `NODE_OPTIONS=--max-old-space-size` do Next | Valores registrados e aplicados na imagem |

## Bloco 6 — Referência: migração para ECS Fargate (não executar sem gatilho)

Só entra em execução quando um gatilho do ADR 0017 disparar. Ao disparar, abrir feature própria com PRD e design.

| # | Tarefa |
| - | ------ |
| 6.1 | Task definitions de API e Web (0,5 vCPU / 1 GB), serviços separados, health check grace de 120 s |
| 6.2 | ALB com regras de caminho (`/api/*` → API), tarefas em subnet pública, security group só do ALB |
| 6.3 | Application Auto Scaling: metas de CPU e memória; ações agendadas `min=max=0` às 00h e `min=1,max=3` às 06:50 |
| 6.4 | Substituir o canário por tarefa única de verificação ou circuit breaker; atualizar o desired count sem forçar valor no deploy |
| 6.5 | Alarme de tarefa parada por `OutOfMemoryError` e de hosts não saudáveis |
| 6.6 | Teste de integração com duas instâncias aplicando migrações ao mesmo tempo (bloqueio de migração do EF Core), antes de rodar mais de uma tarefa |
| 6.7 | Redis, quando houver mais de uma tarefa servindo tráfego (cache de saída e teto diário de e-mails por processo divergem) |

## Deviation notes

Nenhuma até agora.
