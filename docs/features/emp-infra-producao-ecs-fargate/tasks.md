---
version: 2.0.0
date: 2026-10-19
status: Draft
---

# Tasks — Infraestrutura de produção em ECS Fargate com Auto Scaling

**feature-id:** `emp-infra-producao-ecs-fargate` · **Decisão:** [ADR 0018](../../sdd/adrs/0018-infra-ecs-fargate-com-auto-scaling.md)

Não há `prd.md`, `design.md` nem `spec.md` nesta pasta: contexto, alternativas, custos e gatilhos estão no ADR 0018.
Este plano **substitui** o plano 1.0.0 (EC2 + RDS), baseado no ADR 0017, que ficou superado.

**Gate humano:** nada abaixo é executado sem aprovação deste plano. As fases 1 e 2 não criam recurso permanente na AWS;
a fase 1 usa um ambiente de homologação efêmero (criado e destruído pelo Terraform) e a fase 7 só começa com o custo
máximo projetado aprovado.

**Orçamento de referência (sa-east-1):** mínimo R$ 582/mês, máximo normal R$ 813 (2 tarefas por serviço o mês todo),
estresse R$ 880, teto planejado R$ 1.000, contenção em R$ 800, meta R$ 650–700. Ver o ADR 0018.

## Fase 0 — Decisões e pré-requisitos (humano)

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 0.1 | Decidir região de produção com o jurídico (LGPD, art. 33) | Registrada no ADR 0018 |
| 0.2 | Decidir o titular da conta AWS e quem paga | Registrada no ADR 0018 |
| 0.3 | Definir domínio e nomenclatura (frontend e API na mesma origem) | Hostname de homologação e de produção definidos |
| 0.4 | Pedir a saída do sandbox do SES cedo (prazo não verificado) | Pedido enviado, com domínio remetente definido em 0.3 |
| 0.5 | Registrar que os requisitos de LGPD (termos, consentimento, exportação, anonimização) e a revisão jurídica ficam fora desta feature e bloqueiam a entrada em produção | Acompanhados em feature própria ou no issue tracker |
| 0.6 | Decidir o tratamento do `BypassIps` com SSR em IPs dinâmicos (ADR 0004, item 4) | Decisão registrada |

## Fase 1 — Medir o consumo e validar tamanhos (homologação efêmera)

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 1.1 | Ambiente efêmero com os mesmos tamanhos do ADR 0018, criado e destruído por Terraform; usar dados sintéticos | `apply` e `destroy` sem resíduo |
| 1.2 | Teste de carga k6: 70 % leitura anônima, 20 % autenticada, 10 % escrita; rampa até 50 req/s e campanha de 10 req/s por 30 minutos | Relatório com CPU e memória por tarefa, p95/p99, boot até saudável e 5xx |
| 1.3 | Aplicar o critério: memória p99 até 70 % do limite, CPU p95 até 60 %, zero morte por memória, boot dentro da carência de 120 s | Tamanho final decidido: opção A mantida ou Web em 0,25/0,5 |
| 1.4 | Rever `DOTNET_gcServer=1`, fixar teto de heap da API e `NODE_OPTIONS=--max-old-space-size` do Next (~75 % do limite); avaliar o efeito do `next/image` na memória | Valores registrados e aplicados nas imagens |
| 1.5 | Medir o tempo real de religação do RDS e a subida das tarefas | Horários da agenda ajustados no ADR 0018 |
| 1.6 | Medir a reação do Auto Scaling (métrica até tarefa atendendo) | Tempo registrado; a estimativa de 3 a 10 minutos confirmada ou corrigida |
| 1.7 | Teste de integração com duas tarefas aplicando migrações ao mesmo tempo (bloqueio de migração do EF Core) | Sem corrupção; resultado documentado |

## Fase 2 — Confirmar preços, impostos e limites de custo

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 2.1 | Reconsultar a Price List (Fargate, ALB, IPv4, RDS, CloudWatch, SES), o PTAX e os tributos das faturas | Tabela do ADR 0018 atualizada com a data |
| 2.2 | Verificar o preço do NAT em sa-east-1, do VPC Link, da saída de dados acima de 100 GB e a franquia de backup do RDS | Itens "não verificados" resolvidos ou mantidos como risco |
| 2.3 | Confirmar a cota de vCPU do Fargate da conta e ajustar se necessário | Cota registrada |
| 2.4 | Recalcular o cenário máximo com o tamanho decidido em 1.3 | Máximo e estresse abaixo de R$ 1.000, ou o `max` revisto |
| 2.5 | Aprovar o custo máximo projetado | Aprovação por escrito do responsável |

## Fase 3 — Aplicação (repositório)

| # | Tarefa | Camada | Critério de pronto |
| - | ------ | ------ | ------------------ |
| 3.1 | Persistir as chaves do Data Protection no banco com migration aditiva | Infra | Link de confirmação gerado antes de recriar a tarefa continua válido depois |
| 3.2 | Connection string com TLS (`SSL Mode`), via configuração | Backend | API conecta ao RDS com TLS exigido |
| 3.3 | Validar as migrações (`unaccent`, `pg_trgm`, `CREATE TEXT SEARCH CONFIGURATION`) num RDS de teste | Infra | Todas aplicam do zero e `/health/ready` responde 200 |
| 3.4 | `Redis:Enabled=false` explícito; documentar os efeitos por tarefa (limite por IP, teto de e-mails, cache de saída) | Backend | Configuração sem valor por omissão `true` |
| 3.5 | `output: 'standalone'`, `frontend/Dockerfile` (arm64, usuário não root) e endpoint de saúde do Next | Frontend | Imagem sobe e responde 200 sem depender da API |
| 3.6 | Confirmar `API_INTERNAL_BASE_URL` e `NEXT_PUBLIC_API_BASE_URL` com roteamento por caminho no ALB | Frontend | Login, cookie `Secure` e `proxy.ts` funcionam na mesma origem |
| 3.7 | Encerramento limpo (SIGTERM) na API e no Next, compatível com `stopTimeout` e atraso de desregistro de 30 s | Backend e Frontend | Requisições em andamento terminam em deploy e na redução de escala |
| 3.8 | Tratamento do `BypassIps` conforme a decisão 0.6 | Backend | SSR não esgota o balde do limite por IP |

## Fase 4 — Infraestrutura por Terraform

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 4.1 | Conta dedicada ao cliente, MFA, estado em S3 criptografado com bloqueio, tags de alocação de custo ativadas antes dos recursos | Estado remoto funcionando; tags no Cost Explorer |
| 4.2 | Módulo de rede: VPC, subnets públicas em 2 zonas e privadas isoladas, endpoint de gateway do S3 | Subnets do RDS sem rota para o internet gateway |
| 4.3 | Módulo RDS: `db.t4g.micro`, 20 GB, privado, criptografado, `rds.force_ssl`, backup, deletion protection | Acesso só do security group das tarefas |
| 4.4 | Módulo ALB: HTTPS com ACM, redirecionamento de 80, regras `/api/*` e padrão, health checks `/health/live`, desregistro de 30 s, Route 53 | HTTPS válido e cookie `Secure` aceito |
| 4.5 | Papéis IAM com privilégio mínimo (execução, tarefa, OIDC do GitHub, Scheduler, Budgets); sem chave estática | Nenhum `AWS_ACCESS_KEY_ID` nos Environments |
| 4.6 | Parâmetros SSM para os segredos (sem valores no repositório) | Segredos injetados como `secrets` na task definition |
| 4.7 | Verificação estática do Terraform (ex.: checkov ou tflint) proibindo `0.0.0.0/0` de entrada fora do ALB | Falha na CI se violada |
| 4.8 | `plan` agendado com papel somente leitura para detectar alteração manual | Alerta quando houver diferença |

### Base estrutural do Terraform (referência: IntegradorAnymarket)

O Terraform desta fase segue a estrutura já validada no projeto IntegradorAnymarket. É **referência de
convenções**, não código a copiar: nada foi escrito nem aplicado, e os recursos (ALB, RDS, Scheduler) são outros.

| Convenção do Integrador | Aplicação aqui |
| ----------------------- | -------------- |
| Workspace = ambiente, com guarda que aborta quando o workspace não é o esperado | `homolog` e `prod` como workspaces; o `plan` falha no workspace `default` |
| Backend S3 com `use_lockfile` (sem DynamoDB) e bucket de estado criado por um módulo `bootstrap` separado | Mesmo desenho; resolve a tarefa 4.1 |
| Módulos pequenos com `resource.tf`, `variables.tf` e `outputs.tf` | `network`, `rds`, `alb`, `ecs-service`, `iam`, `scheduler` (tarefas 4.2 a 4.5) |
| Provider com versão fixada e `allowed_account_ids` | Impede aplicar na conta errada; a conta dedicada do cliente entra na lista |
| `lifecycle { ignore_changes = [task_definition] }` no serviço ECS | O deploy da CI troca a task definition sem o `plan` brigar com ele |
| Deployment circuit breaker com rollback | Reversão automática de deploy que não fica saudável (não cobre migração `rename`/`drop`) |
| Tarefas em subnet pública com IP público, sem NAT Gateway | Já decidido no ADR 0018; o isolamento vem dos security groups |
| Regras de security group como recursos próprios | Facilita a verificação estática da tarefa 4.7 |
| Segredos referenciados por `valueFrom` do Secrets Manager/SSM na task definition | Tarefa 4.6; o valor nunca passa pelo Terraform nem pelo repositório |
| CI com `terraform fmt -check` e `validate` | Sem credencial AWS; o `plan` roda só com o papel somente leitura (tarefa 4.8) |

Pré-requisito vindo da Fase 3 que o Terraform precisa honrar: ver o ADR 0019 (variáveis `Startup__RunMigrations`,
`ForwardedHeaders__KnownNetworks`, `INTERNAL_API_SECRET`, `DataProtection__CertificatePfxBase64`).

## Fase 5 — Task definitions, serviços ECS e deploy

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 5.1 | Repositórios ECR com política de ciclo de vida (10 imagens) e jobs de imagem de API e Web com tag imutável | Imagens `api` e `web` publicadas por commit |
| 5.2 | Cluster, task definitions e serviços da API e do Web (opção definida em 1.3), subnets públicas, security group só do ALB | Tarefas saudáveis atrás do ALB |
| 5.3 | `ignore_changes` em `task_definition` e `desired_count` no Terraform | `plan` limpo após um deploy do CI |
| 5.4 | Pipeline de deploy: registra revisão, atualiza o serviço, circuit breaker com rollback; substitui o canário por SSM | Deploy e rollback automáticos testados |
| 5.5 | Deploy restrito à janela 07h–00h, com mensagem clara fora dela | Job falha com explicação quando as tarefas estão zeradas |

## Fase 6 — Auto Scaling, agendas e alarmes

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 6.1 | Alvos escaláveis: API e Web com mínimo 1 e máximo 2 | Máximo respeitado em teste |
| 6.2 | Target tracking de CPU 60 % (expansão 60 s, redução 600 s) e de memória 70 % com `DisableScaleIn` | Expande sob carga e reduz sem oscilar |
| 6.3 | Ações agendadas (fuso `America/Sao_Paulo`): 06:50 `min=1,max=2` e 00:00 `min=max=0` | Uma noite completa sem tarefas rodando |
| 6.4 | EventBridge Scheduler: RDS inicia 06:25 e para 00:15, com retentativas e DLQ | Um ciclo executado sem intervenção |
| 6.5 | Lambda de verificação às 06:45 (RDS `available`, reenvio e alerta) | Falha simulada gera alerta |
| 6.6 | Alarmes: CPU, memória, tarefa parada (evento `OutOfMemoryError`), 5xx do ALB, hosts não saudáveis, tempo de resposta p95, RDS (CPU, memória livre, espaço, conexões), DLQ do Scheduler; ações desativadas às 00:00 e reativadas às 07:10 | Alarme de teste dispara e chega ao destino |
| 6.7 | Logs com retenção de 14 dias | Retenção aplicada nos grupos de logs |
| 6.8 | AWS Budgets: R$ 700 real, R$ 800 previsto, R$ 900 real, R$ 1.000 previsto; ação de IAM de negação em R$ 900; **sem** ação de parada do RDS | Alertas chegam; a política bloqueia aumentar o `max` |
| 6.9 | Lambda que reduz o `max` de um serviço, só com aprovação humana | Executa apenas sob aprovação |

## Fase 7 — Testes finais e produção

| # | Tarefa | Critério de pronto |
| - | ------ | ------------------ |
| 7.1 | Testar início e parada diários, incluindo falha simulada do Scheduler e do RDS | Procedimento de recuperação escrito |
| 7.2 | Testar falha de tarefa (matar tarefa) e falta de memória | O ECS repõe; indisponibilidade medida e registrada |
| 7.3 | Testar expansão horizontal e redução | Gatilhos e `max` confirmados |
| 7.4 | Testar nova revisão de tarefa com mais CPU e memória | Deploy rolante concluído sem queda não prevista |
| 7.5 | Testar a restauração do RDS a partir de snapshot | Banco restaurado e aplicação conectando |
| 7.6 | Smoke e QA E2E com dados sintéticos | Fluxos de candidato e recrutador passam |
| 7.7 | Checklist de entrada em produção abaixo, aprovado pelo responsável | Aprovação registrada |
| 7.8 | Produção: mesmas fases numa conta de produção, domínio definitivo e SES fora do sandbox | Entrada em produção com custo máximo aprovado |

## Checklist de entrada em produção

- Tarefas aprovadas no teste de carga (memória p99 até 70 %, CPU p95 até 60 %, zero morte por memória).
- Chaves do Data Protection no banco, validadas após reiniciar tarefas.
- Cookie, login e `proxy.ts` funcionando na mesma origem pelo ALB.
- RDS privado, TLS obrigatório, backup restaurado em teste.
- Ciclo 00h–07h completo, com falha simulada do Scheduler.
- Falha de tarefa e falta de memória simuladas; o ECS repõe as tarefas.
- Expansão e redução observadas com o `max` respeitado.
- Alertas do Budgets e a política IAM de bloqueio testados.
- `plan` do Terraform limpo, sem alteração manual.
- Custo máximo projetado aprovado.

## Deviation notes

- 2.2.0 (2026-10-20): o Terraform foi escrito em `infra/` na branch `feat/infra-terraform-ecs` (ADR 0020), com a estrutura do
  IntegradorAnymarket: rede, RDS, ALB, ECR, segredo, IAM, ECS (inclui a tarefa de migração), Auto Scaling, agenda, alarmes e
  orçamento. Cobre as tarefas 4.1 (bucket de estado), 4.2 a 4.7 e partes de 5 e 6 (task definitions, serviços, target
  tracking e agendas), **só com `fmt` e `validate`; nenhum `plan` nem `apply`**, porque não há conta, domínio nem certificado.
  Ficam de fora: 4.8 (`plan` agendado), a Lambda das 06:45, a ação do Budgets e os workflows de CD para ECS.
- 2.1.0 (2026-10-19): a Fase 3 foi preparada na branch `feat/ecs-fase1-preparacao-multi-replica` (ADR 0019):
  3.1 feita (Data Protection no PostgreSQL, cifrado por certificado); 3.5 feita (Dockerfile, `/healthz`, verificação na
  CI); 3.8 feita por outro caminho que o `BypassIps` (segredo `X-Internal-Caller`, ver ADR 0019); 3.3, 3.4, 3.6 e 3.7
  seguem pendentes de validação num RDS e no ALB reais; 3.2 não foi tocada. O rate limit e o teto de e-mails continuam
  por réplica (multiplicam por N): risco residual registrado no ADR 0019.

- 2.0.0 (2026-10-19): substitui o plano 1.0.0 (EC2 + RDS, ADR 0017). A pasta passou de `emp-infra-producao-ecs-ready`
  para `emp-infra-producao-ecs-fargate`.
