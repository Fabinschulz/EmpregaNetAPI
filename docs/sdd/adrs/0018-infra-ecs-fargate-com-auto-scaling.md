# ADR 0018: Infraestrutura em ECS Fargate com Auto Scaling, ALB e RDS; tarefas zeradas entre 00h e 07h

## Status

Aceite. Substitui o [ADR 0017](0017-infra-inicial-ec2-rds-preparada-para-ecs.md). Decisões pendentes no fim.

## Contexto

O [ADR 0017](0017-infra-inicial-ec2-rds-preparada-para-ecs.md) escolheu EC2 única com RDS e deixou o Fargate
adiado. A revisão crítica de 2026-10-19 mostrou que essa escolha **não cumpre o requisito obrigatório** de a
infraestrutura aumentar a capacidade automaticamente durante a janela de operação: a EC2 única só escala na
vertical e à mão, e "preparada para ECS" significava apenas contêiner sem estado local. Decisão do responsável:
seguir com **ECS Fargate + Auto Scaling + ALB + RDS PostgreSQL**.

Requisitos que moldam a decisão:

- Operação **07h–00h** (`America/Sao_Paulo`); entre 00h e 07h as tarefas ficam zeradas e a indisponibilidade é aceita.
- Uma tarefa inicial por serviço, escala horizontal automática e limite explícito de tarefas.
- Orçamento: teto planejado de **R$ 1.000/mês**, gatilho de contenção em **R$ 800** e meta de **R$ 650–700**
  (o pedido original citava R$ 800 e R$ 1.000; ficou assim).
- Sem Redis, WAF, NAT Gateway nem alta disponibilidade completa no lançamento.
- Infraestrutura por Terraform, deploy por GitHub Actions com OIDC, mesmo domínio para frontend e API.

Estado do repositório (evidência):

- Os cookies de autenticação são host-only, sem `Domain`, e o `proxy.ts` os lê no servidor Next; API e frontend
  precisam estar na **mesma origem**.
- Não há `AddDataProtection` em `backend/src`: com tarefas efémeras, as chaves somem e os links de confirmação de
  e-mail e de redefinição de senha quebram.
- Cache de saída, teto diário de e-mails e rate limit são por processo; Redis é opcional e o `cd-ec2.yml` assume
  `Redis__Enabled=true` por omissão.
- `/health/live` não consulta dependências; `/health/ready` consulta o banco. O Next não tem endpoint de saúde.
- A API valida tokens do Google (`Google.Apis.Auth`) e usa Sentry, logo precisa de saída para a internet.
- Migrações são aplicadas no boot; release com `RenameColumn`/`DropColumn` só avança.
- O frontend não tem Dockerfile e o repositório não tem Terraform.

## Decisão

**Arquitetura**

1. Dois serviços ECS Fargate (ARM64): **API** (porta 8080) e **Web/Next SSR** (porta 3000), atrás de um único **ALB**
   com HTTPS (ACM): `/api/*` → API, restante → Web. Mesma origem, cookie host-only preservado.
2. **RDS PostgreSQL `db.t4g.micro` Single-AZ**, 20 GB gp3, privado, criptografado, TLS obrigatório, backup automático.
3. VPC dedicada: subnets **públicas** em 2 zonas para ALB e tarefas; subnets **privadas isoladas** (sem rota para o
   internet gateway) para o RDS. Endpoint de gateway do S3 (sem custo).
4. Tarefas **sem NAT Gateway**, com IPv4 público e security group que só aceita entrada do ALB. Controles na seção
   "Rede e segurança".

**Tamanho e escala**

| | API | Web |
| - | - | - |
| Tarefa inicial (até a medição) | 0,5 vCPU, 1 GB | 0,5 vCPU, 1 GB |
| Mínimo / máximo (07h–00h) | 1 / 2 | 1 / 2 |
| Madrugada | 0 / 0 | 0 / 0 |
| Target tracking | CPU 60 %; memória 70 % só para expandir (`DisableScaleIn`) | idem |
| Cooldown | expansão 60 s; redução 600 s | idem |
| Health check do ALB | `/health/live` | endpoint próprio (a criar) |
| Carência do health check | 120 s | 120 s |

O tamanho é a **opção A** até a medição: o Web em 0,25 vCPU e 0,5 GB só é adotado se o teste de aceitação passar
(memória p99 até 70 % do limite, CPU p95 até 60 % na carga esperada, nenhuma morte por memória, boot dentro da
carência). O 0,25 vCPU só aceita 0,5, 1 ou 2 GB de memória no Fargate. Não se recomenda 0,25/0,5 para a API.

**Agenda** (`America/Sao_Paulo`)

| Hora | Ação |
| ---- | ---- |
| 06:25 | EventBridge Scheduler inicia o RDS (retentativas e DLQ) |
| 06:45 | Lambda confere o RDS `available`; se não, reenvia o início e alerta |
| 06:50 | Ação agendada do Application Auto Scaling: `min=1`, `max=2` nos dois serviços |
| 07:10 | Reativa as ações dos alarmes |
| 00:00 | Ação agendada: `min=max=0`; desativa as ações dos alarmes |
| 00:15 | Scheduler para o RDS |

O ALB usa `/health/live`: com `/health/ready` o ECS substituiria tarefas em laço se o banco estivesse fora. Atraso de
desregistro do alvo: 30 s (padrão: 300 s).

**Controle de custo**

- O **limite rígido** é `max=2` por serviço (e a cota de vCPU do Fargate da conta). Alertas do AWS Budgets
  (R$ 700 real, R$ 800 previsto, R$ 900 real, R$ 1.000 previsto) são **informativos e atrasados**; não são teto.
- Ação do Budgets em R$ 900: aplicar política IAM de negação que impede aumentar o máximo de capacidade e registrar
  tamanhos maiores. **Nunca configurar parada do RDS de produção.**
- Redução automática do `max` de um serviço (Lambda) só com aprovação humana; nunca mexer no `min`.

**Terraform**

- Módulos de rede, ALB, ECS, RDS, IAM, agendas, alarmes e orçamentos; estado em S3 criptografado com bloqueio;
  aplicação com aprovação humana; `plan` agendado com papel somente leitura para detectar alteração manual.
- O CI registra revisões da task definition, então o serviço usa `ignore_changes` em `task_definition` e
  `desired_count`; mínimos e máximos ficam nas ações agendadas.

## Rede e segurança

| Controle | Configuração |
| -------- | ------------ |
| ALB | 443 (ACM) e 80 com redirecionamento; entrada da internet, saída só para as tarefas |
| Tarefas | Entrada só do security group do ALB (8080 e 3000); saída 443 e 5432 para o RDS; sem SSH |
| RDS | `PubliclyAccessible=false`, entrada 5432 só das tarefas, `rds.force_ssl`, criptografia em repouso |
| Segredos | Parâmetros SSM injetados como `secrets` na task definition; nada na imagem |
| Papéis | Execução: ECR, logs, leitura dos parâmetros. Tarefa: só `ses:SendEmail`. OIDC: registrar task definition, atualizar serviço, `iam:PassRole` restrito |
| Guardrail | Verificação estática (ex.: checkov ou tflint) proibindo `0.0.0.0/0` de entrada fora do ALB |

Subnets públicas sem NAT são válidas para o lançamento **com essas condições**: a proteção depende dos security
groups. Subnets privadas exigiriam NAT por causa da validação do Google e do Sentry; o custo de NAT em sa-east-1 não
foi verificado.

## Alternativas avaliadas

| Alternativa | Resultado |
| ----------- | --------- |
| EC2 única + RDS (ADR 0017) | R$ 346/mês, **sem Auto Scaling**; substituída |
| EC2 com Auto Scaling Group + ALB | ~R$ 515 com reação mais lenta (nova instância, imagem e boot); pior que o Fargate |
| Fargate sem ALB (API Gateway HTTP + VPC Link + Cloud Map) | ~R$ 380 estimado; viável pela documentação, **não validado** para Next SSR (streaming, WAF, esvaziamento de conexões, preço do VPC Link) |
| NLB no lugar do ALB | Sem economia: US$ 0,034/h, igual ao ALB, e sem roteamento por caminho |
| Apagar e recriar o ALB toda noite | Economiza ~R$ 56/mês; o DNS do balanceador muda e há risco às 07h |
| Fargate com 2 tarefas por serviço, Multi-AZ, Redis e WAF | ~R$ 1.116/mês; alta disponibilidade, fora do lançamento |

## Custos (sa-east-1, 515 h, tributos de 13,6 %, PTAX R$ 5,0521)

| Cenário | Opção A (0,5/1 + 0,5/1) | Opção B (Web 0,25/0,5) |
| ------- | ----------------------: | ---------------------: |
| 1 tarefa por serviço | 582 | 532 |
| 2 tarefas por serviço o mês todo | 813 | 712 |
| 1 tarefa e +60 h extras em cada | 609 | 553 |
| Logs, tráfego e e-mails moderados, +60 h | 637 | 581 |
| API revisada para 1 vCPU e 2 GB, moderado, +60 h | 749 | 693 |
| Estresse: 2+2, LCU alto, 6 GB de log | 880 | 779 |

Fixos independentes do tamanho: R$ 352/mês (ALB R$ 184 mais LCU, RDS R$ 128, alarmes R$ 18, logs, DNS e ECR). Piso com
tarefas zeradas e RDS parado: ~R$ 232/mês; a janela 00h–07h custa ~R$ 68. Cada tarefa extra de 0,5/1 custa ~R$ 0,22
por hora com IPv4.

**Não cabe no teto de R$ 1.000:** API com 1 vCPU e 2 GB em 2 tarefas junto com o Web 0,5/1 em 2 (R$ 1.014 sem estresse,
R$ 1.080 com). Se a API precisar desse tamanho, o `max` de um dos serviços volta a 1.

## Adiado — com gatilho de retorno

| Capacidade | Gatilho |
| ---------- | ------- |
| 2 tarefas por serviço permanentes (`min=2`), em 2 zonas | Parada por falha de tarefa inaceitável, ou meta formal de disponibilidade (+R$ 231/mês) |
| RDS Multi-AZ | Exigência de failover automático do banco |
| Redis (ou Valkey) | Limite por IP, teto de e-mails e cache de saída precisarem ser compartilhados entre tarefas |
| WAF | Abuso de tráfego medido ou exigência contratual |
| CloudFront à frente do ALB | Saída de dados ou LCU relevantes; tráfego de estáticos do Next |
| Subnets privadas com NAT | Exigência da prefeitura ou do jurídico |
| Página "fora do horário" à noite | Exigência de UX; o ALB responde 503 simples sem tarefas |

## Consequências

- Atende o requisito de aumento automático de capacidade, mas **não** evita queda na falha de uma tarefa com `min=1`
  (só `min=2` evita) nem protege contra vazamento de memória ou requisição maior que o limite da tarefa, porque todas
  as tarefas têm o mesmo limite. O Auto Scaling reduz o risco de sobrecarga por volume.
- A reação do Auto Scaling não é instantânea (estimativa de 3 a 10 minutos até a nova tarefa atender, não medida).
- Mudar CPU ou memória de uma tarefa exige nova revisão da task definition e deploy; o Fargate não redimensiona tarefa
  em execução.
- Com até 2 tarefas, o rate limit por IP e o teto diário de e-mails valem por tarefa (limite efetivo ×2). O SSR chama a
  API de IPs de tarefa dinâmicos, o que invalida o `BypassIps` por IP do [ADR 0004](0004-proxy-confiavel-explicito.md);
  decidir em homologação.
- O deploy deixa de ser por SSM com canário: o canário atual precisa ser substituído por tarefa única de verificação ou
  circuit breaker do ECS. Release destrutivo continua forward-only.
- Deploy fora da janela não valida nada com tarefas zeradas; deploys acontecem entre 07h e 00h.
- Este ADR torna obrigatório persistir as chaves do Data Protection no banco.

## Premissas e limites da estimativa

Preços dos arquivos oficiais da Price List da AWS de 19/10/2026 (Fargate ARM US$ 0,0557/vCPU-h e US$ 0,00612/GB-h;
ALB US$ 0,034/h e US$ 0,011/LCU-h; IPv4 US$ 0,005/h; alarme US$ 0,135; SES US$ 0,0001; WAF e Redis só como referência).
RDS (US$ 0,034/h e US$ 0,219/GB) vem da consulta de 16/10, não renovada. PTAX de 15/10 e 13,6 % de tributos observados em
duas faturas. São estimativas, não valores garantidos.

Não verificados: preço de NAT em sa-east-1, VPC Link, franquia de backup do RDS, saída de dados acima de 100 GB, número
de pontos do alarme de target tracking, tempo de religação do RDS, LCU real, frequência de atualização do Budgets e a
memória real das aplicações.

## Decisões pendentes

1. Tamanho final das tarefas, após o teste de aceitação em homologação.
2. Região de produção (LGPD, art. 33), titular da conta AWS e quem paga.
3. Domínio e nomenclatura (mesma origem para frontend e API).
4. Tratamento do `BypassIps` com SSR em IPs dinâmicos.
5. Tempo real de religação do RDS, para fixar os horários da agenda.
6. Se a prefeitura exigir subnets privadas, custo do NAT.
