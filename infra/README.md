# Infraestrutura (Terraform)

Recursos AWS da EmpregaNet em ECS Fargate, como Terraform. Decisões: [ADR 0018](../docs/sdd/adrs/0018-infra-ecs-fargate-com-auto-scaling.md) (arquitetura, tamanho, agenda, custo), [ADR 0019](../docs/sdd/adrs/0019-preparacao-multi-replica-para-ecs.md) (o que a aplicação exige da infraestrutura) e [ADR 0020](../docs/sdd/adrs/0020-layout-do-terraform.md) (layout e convenções, herdados do IntegradorAnymarket). Plano: [`tasks.md`](../docs/features/emp-infra-producao-ecs-fargate/tasks.md), fases 4 a 6.

**Nada aqui foi aplicado.** Não existe conta, domínio nem certificado ainda; o código foi validado só de forma estática (ver "Verificações").

| Caminho | O que é |
| ------- | ------- |
| `providers.tf`, `variables.tf`, `resource.tf`, `output.tf` | Configuração raiz. O **workspace é o ambiente** (`homolog`, `prod`); estado remoto no S3 com lock nativo, bucket informado em `backend.hcl` |
| `terraform.tfvars.example`, `backend.hcl.example` | Modelos dos valores que não ficam no repositório; copie para `terraform.tfvars` e `backend.hcl` (gitignore) |
| `modules/network/` | VPC dedicada: subnets públicas em 2 zonas (ALB e tarefas), privadas isoladas (RDS), endpoint de gateway do S3. Sem NAT |
| `modules/rds/` | PostgreSQL `db.t4g.micro`, Single-AZ, 20 GB gp3, privado, criptografado, `rds.force_ssl`, senha mestra gerenciada pelo RDS |
| `modules/ecr/` | Repositórios `api` e `web`: tags imutáveis, scan no push, guarda as 20 imagens mais recentes |
| `modules/secrets/` | O segredo `empreganet/<env>/app` do Secrets Manager, criado **sem valor** |
| `modules/iam/` | Execution role, task roles (a da API só envia e-mail pelo SES) e o papel OIDC da CI (só a `master`) |
| `modules/alb/` | ALB público: 443 com ACM, 80 redireciona; `/api/*` vai para a API (`/health/live`) e o resto para o Next (`/healthz`) |
| `modules/ecs/` | Cluster com Container Insights, log groups, security groups, task definitions (ARM64) e serviços da API e do Web, mais a tarefa de migração |
| `modules/autoscaling/` | Target tracking (CPU 60 %; memória 70 % só expande) e as ações agendadas que abrem (06:50) e fecham (00:00) a janela |
| `modules/scheduler/` | EventBridge Scheduler: liga o RDS às 06:25, desliga às 00:15 e silencia os alarmes de "sem task" de 00:00 a 07:10 |
| `modules/cloudwatch/`, `modules/sns/` | Alarmes (tarefas, 5xx do ALB, CPU e espaço do RDS) e o tópico de e-mail |
| `modules/budgets/` | Orçamento mensal com alertas em 70 % e 90 % do real e 80 % e 100 % do previsto. Informativo, não é teto |
| `bootstrap/` | Cria o bucket de estado uma vez por conta; o estado dele é local |
| `scripts/check-open-ingress.sh` | Falha se um security group aceitar entrada da internet fora do módulo `alb` |

### Variáveis que você informa

| Variável | O que é |
| -------- | ------- |
| `aws_account_id` | Conta dedicada; o provider recusa qualquer outra |
| `domain_name` | Hostname único do frontend e da API (mesma origem: o cookie é host-only) |
| `certificate_arn` | Certificado ACM do hostname, na região do ALB |
| `image_tag` | SHA completo da primeira imagem de `api` e `web`; só vale para o primeiro apply |
| `alarm_email` | Recebe alarmes e alertas do orçamento (a AWS pede confirmação) |
| `route53_zone_id`, `ses_identity`, `aspnetcore_environment`, `service_capacity`, `budget_limit_usd` | Opcionais, com padrão; veja `variables.tf` |

## Pré-requisitos

- Terraform **≥ 1.11** (lock nativo do S3). Validado com a 1.16.5; provider AWS fixado em `6.66.0`.
- AWS CLI v2 com um perfil da conta dedicada e Docker para publicar as imagens (**arm64**: o Fargate usa ARM).
- Um administrador da conta: o papel que aplica cria IAM, VPC e RDS.

## Bootstrap do bucket de estado (uma vez por conta)

```bash
terraform -chdir=infra/bootstrap init
terraform -chdir=infra/bootstrap apply -var aws_account_id=<conta>
```

O bucket tem `prevent_destroy`, versionamento, criptografia e bloqueio de acesso público. Copie a saída `bucket_state` para `infra/backend.hcl`.

## Primeiro apply de um ambiente

A API recusa subir com migração pendente (`Startup__RunMigrations=false`, ADR 0019); por isso o banco é migrado **antes** de os serviços existirem, por uma tarefa avulsa.

```bash
terraform -chdir=infra init -backend-config=backend.hcl
terraform -chdir=infra workspace new homolog
```

**1. Base.** Rede, banco, ECR, segredo, IAM e alertas. Escolha a senha do usuário mestre do RDS e informe-a só nesta execução; ela é efêmera e write-only (não entra no state nem no plan), e é a mesma que vai na connection string do passo 3:

```bash
TF_VAR_db_master_password='<senha>' terraform -chdir=infra apply -target=module.network -target=module.rds -target=module.ecr -target=module.secrets -target=module.iam -target=module.sns
```

**2. Configurar o papel da CI.** Saída `github_actions_role_arn` para a variável de repositório `AWS_DEPLOY_ROLE_ARN`.

**3. Preencher o segredo.** O Terraform só o cria. Todas as chaves abaixo precisam existir, senão a tarefa não inicia (`ResourceInitializationError`). Com `Staging`, as duas do certificado podem ser strings vazias; com `Production` são obrigatórias. A senha é a do passo 1 e o host está na saída `database_endpoint`. Ela não rotaciona sozinha; para trocá-la, atualize o segredo e aumente `db_master_password_version` (o RDS gerenciado rotacionaria a cada 7 dias e invalidaria a connection string, por isso não é usado).

```json
{
  "ConnectionStrings__PostgreSQLConnection": "Host=<host>;Port=5432;Database=empreganet;Username=empreganet_admin;Password=<senha>;SSL Mode=Require;Trust Server Certificate=true",
  "JwtSettings__SecretKey": "<32+ bytes aleatórios>",
  "DataProtection__CertificatePfxBase64": "<PFX com chave privada, em Base64>",
  "DataProtection__CertificatePassword": "<senha do PFX>",
  "INTERNAL_API_SECRET": "<32+ caracteres aleatórios; o mesmo valor vai para a API e o frontend>",
  "Seed__AdminPassword": "<senha do administrador inicial>"
}
```

```bash
aws secretsmanager put-secret-value --secret-id empreganet/homolog/app --secret-string file://<caminho fora do repositório>/segredo.json
```

**4. Publicar a primeira imagem de cada serviço, da sua máquina** (arm64, `--provenance=false` para uma imagem por push):

```bash
terraform -chdir=infra output ecr_repository_urls
# API: publica no host para arm64 (o SDK do .NET aborta sob emulação QEMU: dotnet restore sai com 134) e só
# empacota a imagem em arm64; use um clone limpo do commit, não a árvore de trabalho
dotnet publish backend/src/EmpregaNet.Api/EmpregaNet.Api.csproj -c Release -r linux-musl-arm64 --no-self-contained -p:UseAppHost=false -o backend/publish
docker buildx build --platform linux/arm64 --provenance=false --target prebuilt -t <url api>:<sha> backend && docker push <url api>:<sha>
# Web: o build roda emulado sem problema
docker buildx build --platform linux/arm64 --provenance=false -t <url web>:<sha> frontend && docker push <url web>:<sha>
```

No Windows, o `docker login` pode falhar com *The stub received bad data* (o token do ECR é maior que o limite do credential helper). Contorno: use um `DOCKER_CONFIG` temporário com `{"auths": {"<registro>": {"auth": "<base64 de AWS:token>"}}}` e apague-o depois.

**5. Migrar o banco.** Cria só o necessário para rodar a tarefa de migração e a executa:

```bash
terraform -chdir=infra apply -target=module.network -target=module.ecs.aws_ecs_cluster.this -target='module.ecs.aws_ecs_task_definition.this["migrate"]' -target=module.ecs.aws_vpc_security_group_ingress_rule.database_from_api -target=module.ecs.aws_vpc_security_group_egress_rule.database -target=module.ecs.aws_vpc_security_group_egress_rule.https
terraform -chdir=infra output -json migration_task
aws ecs run-task --cluster <cluster> --task-definition <task_definition> --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[<subnets>],securityGroups=[<security_groups>],assignPublicIp=ENABLED}"
```

Acompanhe os logs em `/ecs/empreganet-<env>/migrate`; a tarefa termina com código 0.

**6. Aplicar o restante** (ALB, serviços, Auto Scaling, agenda, alarmes e orçamento) e conferir que não sobra diferença:

```bash
terraform -chdir=infra plan -out=homolog.tfplan
terraform -chdir=infra apply homolog.tfplan
terraform -chdir=infra plan -detailed-exitcode
```

Aponte o hostname para a saída `alb_dns_name` (ou informe `route53_zone_id`) e confira `https://<domínio>/healthz` (o frontend). O `/health/live` da API só responde dentro do target group; por fora, `/api/*` é o que chega nela.

**7. Daqui em diante, a CI publica e implanta** (seção "Deploy pela CI"). Os serviços ignoram `task_definition` e `desired_count` (`ignore_changes`): a CI é dona da revisão e o Auto Scaling é dono da contagem. O `image_tag` só vale para o primeiro apply.

- No workspace `default` o plan falha de propósito: *"Selecione o workspace do ambiente antes do plan/apply"*.
- O ambiente tem no máximo 17 caracteres (limite de 32 do nome dos target groups).
- A guarda do workspace `default` é um recurso, então não roda nos passos com `-target`: rode sempre `terraform workspace select <env>` antes.
- Mude a infraestrutura dentro da janela 07h-00h: um `apply` que altera o RDS parado (00:15-06:25) falha com `InvalidDBInstanceState`.
- `Trust Server Certificate=true` cifra a conexão sem validar o certificado do RDS; em VPC o risco é baixo, e `VerifyFull` exigiria o bundle da RDS na imagem.
- O papel OIDC da CI só aceita a `master` (`ref:refs/heads/master`). Um job com `environment:` na chave do job gera outro `sub` (`...:environment:<Nome>`) e seria negado: o workflow de CD para ECS não deve usá-lo, ou a trust policy precisa aceitar esse formato.
- Para o primeiro `destroy` e o seguinte do mesmo ambiente, o snapshot final tem nome fixo (`empreganet-<env>-final`); apague o anterior antes.

## Deploy pela CI

O `main-deploy.yml` roda a cada push na `master` e no disparo manual. Dois jobs ficam depois dos CIs, só fora de pull request e **só quando a variável de repositório `AWS_DEPLOY_ROLE_ARN` existe** (sem ela o CI segue normal e nada é publicado):

| Job | O que faz |
| --- | --- |
| `CD - Imagens` (`cd-image.yml`) | Constrói em runner arm64 e publica `api` (a partir do publish do CI) e `web` no ECR, com o SHA completo do commit como tag. Só o serviço cuja área mudou e cujo CI passou. Tag imutável: re-executar o mesmo commit reaproveita a imagem |
| `CD - Deploy ECS` (`cd-ecs.yml`) | Roda `.github/scripts/ecs_deploy.py`: registra a revisão nova de cada serviço, **migra o banco antes** (tarefa `--migrate-only`, só quando a API mudou), atualiza os serviços e espera o rollout. Migração com código diferente de 0 aborta sem tocar nos serviços; um rollout que não fica saudável é revertido pelo circuit breaker |

Para ligar: `gh variable set AWS_DEPLOY_ROLE_ARN --body "$(terraform -chdir=infra output -raw github_actions_role_arn)"`. Variáveis opcionais: `AWS_REGION` (padrão `sa-east-1`) e `NEXT_PUBLIC_GOOGLE_CLIENT_ID` (de build do frontend).

- O deploy **recusa** serviços com 0 tarefas e o RDS que não esteja `available` (de 00:15 a 06:25 ele está parado). Rode dentro da janela 07h-00h; o disparo manual tem `allow_off_window` para os casos deliberados.
- Os jobs não usam `environment:`, pelo motivo do `sub` do OIDC descrito acima.
- Os testes do script rodam sem AWS: `python -m unittest discover -s .github/scripts/tests` (a CI de infra também os roda).
- Só existe o ambiente `homolog`. Um `prod` pede o seu próprio papel (outra variável) e outra conta ou outro workspace.

## Janela de operação e custo

Às 06:25 o RDS liga; às 06:50 os serviços voltam a `min`/`max` (1/2 por padrão); às 00:00 vão a 0/0; às 00:15 o RDS desliga. O Scheduler usa `America/Sao_Paulo`. **O limite rígido de custo é o `max` do Auto Scaling e a cota de vCPU do Fargate da conta**; o orçamento só avisa. Com `schedule_enabled = false` tudo fica 24 horas.

## Verificações

Não há testes automatizados em `infra/`. As verificações estáticas, que a CI também roda (`CI - Infra`, só quando `infra/` muda) e não precisam de credenciais:

```bash
terraform fmt -check -recursive infra
bash infra/scripts/check-open-ingress.sh infra
terraform -chdir=infra/modules/network init -backend=false && terraform -chdir=infra/modules/network validate
```

Rode o `validate` do mesmo jeito em cada pasta com `.tf` (cada módulo e `infra/bootstrap`). Cada pasta tem seu `.terraform.lock.hcl` versionado com o provider `6.66.0`; ao criar um módulo, copie o lock de outro antes do `init`.

## O que não está aqui

Deliberadamente fora, com gatilho de retorno no ADR 0018: a Lambda de conferência do RDS às 06:45; a ação do Budgets que nega aumento de capacidade em 90 %; o `plan` agendado com papel somente leitura (tarefa 4.8); Sentry; login com Google (`GoogleAuth__ClientIds`; o client id do frontend é de build, na CI de imagem); WAF; Redis; NAT; Multi-AZ. Dependem de decisões ou recursos que ainda não existem: conta, domínio, certificado, identidade do SES.
