data "aws_region" "current" {}

locals {
  # As chaves são endereços de recurso: renomear uma destrói o recurso. Para renomear, use um bloco moved.
  services = toset(["api", "web"])

  name_prefix = "empreganet-${var.environment}"

  container_ports = { api = 8080, web = 3000 }

  public_url = "https://${var.domain_name}"
}

# ------------------------- Cluster -----------------------------------
resource "aws_ecs_cluster" "this" {
  name = local.name_prefix

  # Container Insights publica a métrica RunningTaskCount usada pelo alarme de serviço sem task rodando.
  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = var.tags
}

# ------------------------- Log groups --------------------------------
# O módulo iam deriva o ARN destes grupos do nome, então o nome não pode mudar sem mudar lá.
resource "aws_cloudwatch_log_group" "this" {
  for_each = toset(concat(tolist(local.services), ["migrate"]))

  name              = "/ecs/${local.name_prefix}/${each.key}"
  retention_in_days = 30
  tags              = var.tags
}

# ------------------------- Task security groups ----------------------
# Um grupo por serviço; a tarefa de migração usa o da API. As regras são recursos próprios (sem blocos inline).
resource "aws_security_group" "task" {
  for_each = local.services

  name        = "${local.name_prefix}-${each.key}-task"
  description = "Tasks ${each.key} da EmpregaNet"
  vpc_id      = var.vpc_id
  tags        = var.tags
}

# Entrada só do load balancer: nenhuma tarefa aceita tráfego direto da internet, mesmo com IP público.
resource "aws_vpc_security_group_ingress_rule" "from_alb" {
  for_each = local.services

  security_group_id            = aws_security_group.task[each.key].id
  description                  = "HTTP ${local.container_ports[each.key]} vindo do load balancer"
  referenced_security_group_id = var.alb_security_group_id
  ip_protocol                  = "tcp"
  from_port                    = local.container_ports[each.key]
  to_port                      = local.container_ports[each.key]
}

# Um security group criado pelo Terraform não tem saída liberada: sem esta regra o load balancer não alcança as tasks.
resource "aws_vpc_security_group_egress_rule" "alb_to_task" {
  for_each = local.services

  security_group_id            = var.alb_security_group_id
  description                  = "HTTP ${local.container_ports[each.key]} para as tasks ${each.key}"
  referenced_security_group_id = aws_security_group.task[each.key].id
  ip_protocol                  = "tcp"
  from_port                    = local.container_ports[each.key]
  to_port                      = local.container_ports[each.key]
}

# Saída 443 para a internet: ECR, Secrets Manager, CloudWatch Logs, SES, Google, Sentry e, no frontend, a API pelo
# hostname público. Sem NAT, é o que a tarefa precisa para funcionar.
resource "aws_vpc_security_group_egress_rule" "https" {
  for_each = local.services

  security_group_id = aws_security_group.task[each.key].id
  description       = "HTTPS para a internet"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 443
  to_port           = 443
}

# Só a API (e a migração) fala com o banco.
resource "aws_vpc_security_group_egress_rule" "database" {
  security_group_id            = aws_security_group.task["api"].id
  description                  = "PostgreSQL 5432 para o RDS"
  referenced_security_group_id = var.database_security_group
  ip_protocol                  = "tcp"
  from_port                    = 5432
  to_port                      = 5432
}

# Única escrita em um recurso que este módulo não possui: a entrada 5432 no security group do RDS, só da API.
resource "aws_vpc_security_group_ingress_rule" "database_from_api" {
  security_group_id            = var.database_security_group
  description                  = "PostgreSQL 5432 vindo das tasks da API"
  referenced_security_group_id = aws_security_group.task["api"].id
  ip_protocol                  = "tcp"
  from_port                    = 5432
  to_port                      = 5432
  tags                         = var.tags
}

# ------------------------- Configuração da aplicação -------------------
locals {
  # Convenção __ do .NET: JwtSettings__SecretKey vira JwtSettings:SecretKey. Nenhum segredo aqui.
  api_environment = [
    { name = "ASPNETCORE_ENVIRONMENT", value = var.aspnetcore_environment },
    { name = "Redis__Enabled", value = "false" },
    { name = "Database__ExpectedMaxReplicas", value = tostring(var.capacity["api"].max + 1) },
    { name = "JwtSettings__Issuer", value = "EmpregaNet.Api" },
    { name = "JwtSettings__Audience", value = "EmpregaNet.App" },
    { name = "JwtSettings__ExpirationHours", value = "8" },
    { name = "JwtSettings__RefreshTokenExpirationDays", value = "14" },
    { name = "AppUrls__PublicAppBaseUrl", value = local.public_url },
    { name = "AppUrls__CorsAllowedOrigins__0", value = local.public_url },
    { name = "AppUrls__EmailConfirmationPath", value = "/confirm-email" },
    { name = "AppUrls__PasswordResetPath", value = "/reset-password" },
    { name = "Ses__Enabled", value = tostring(var.ses_identity != null) },
    { name = "Ses__Region", value = data.aws_region.current.region },
    { name = "Ses__FromEmail", value = var.ses_from_email },
    { name = "Ses__FromName", value = "EmpregaNet" },
    { name = "OutputCache__DefaultExpirationMinutes", value = "2" },
    { name = "RateLimiting__Enabled", value = "true" },
    { name = "RateLimiting__BurstCapacity", value = "600" },
    { name = "RateLimiting__SustainedPerPeriod", value = "300" },
    { name = "RateLimiting__ReplenishmentPeriodInSeconds", value = "10" },
    { name = "EmailThrottle__MaxPerDay", value = "30" },
  ]

  # O ALB fica nas subnets públicas: é a única rede de onde a API confia em X-Forwarded-*.
  forwarded_networks = [
    for i, cidr in var.alb_subnet_cidrs : { name = "ForwardedHeaders__KnownNetworks__${i}", value = cidr }
  ]

  # Réplicas não migram nem fazem seed: quem faz isso é a tarefa de migração, uma vez por deploy.
  startup_replica = [
    { name = "Startup__RunMigrations", value = "false" },
    { name = "Startup__RunSeed", value = "false" },
  ]

  secret_keys = {
    connection_string = "ConnectionStrings__PostgreSQLConnection"
    jwt               = "JwtSettings__SecretKey"
    dp_certificate    = "DataProtection__CertificatePfxBase64"
    dp_password       = "DataProtection__CertificatePassword"
    internal_caller   = "INTERNAL_API_SECRET"
    seed_password     = "Seed__AdminPassword"
  }

  # Chaves que precisam existir no segredo antes do deploy da revisão que as referencia, ou o ECS não inicia a task.
  # Com Staging, as duas do certificado podem ser strings vazias.
  api_secrets = [
    { name = "ConnectionStrings__PostgreSQLConnection", key = local.secret_keys.connection_string },
    { name = "JwtSettings__SecretKey", key = local.secret_keys.jwt },
    { name = "DataProtection__CertificatePfxBase64", key = local.secret_keys.dp_certificate },
    { name = "DataProtection__CertificatePassword", key = local.secret_keys.dp_password },
    { name = "RateLimiting__InternalCaller__Secret", key = local.secret_keys.internal_caller },
  ]

  migrate_secrets = [
    { name = "ConnectionStrings__PostgreSQLConnection", key = local.secret_keys.connection_string },
    { name = "JwtSettings__SecretKey", key = local.secret_keys.jwt },
    { name = "DataProtection__CertificatePfxBase64", key = local.secret_keys.dp_certificate },
    { name = "DataProtection__CertificatePassword", key = local.secret_keys.dp_password },
    { name = "Seed__AdminPassword", key = local.secret_keys.seed_password },
  ]

  web_secrets = [
    { name = "INTERNAL_API_SECRET", key = local.secret_keys.internal_caller },
  ]

  # Definição de cada contêiner. A tarefa de migração roda a mesma imagem da API com --migrate-only.
  containers = {
    api = {
      image       = "${var.repository_urls["api"]}:${var.image_tag}"
      port        = 8080
      command     = null
      environment = concat(local.api_environment, local.forwarded_networks, local.startup_replica)
      secrets     = local.api_secrets
      role        = "api"
      cpu_memory  = "api"
    }
    web = {
      image       = "${var.repository_urls["web"]}:${var.image_tag}"
      port        = 3000
      command     = null
      environment = [{ name = "API_INTERNAL_BASE_URL", value = local.public_url }]
      secrets     = local.web_secrets
      role        = "web"
      cpu_memory  = "web"
    }
    migrate = {
      image   = "${var.repository_urls["api"]}:${var.image_tag}"
      port    = null
      command = ["--migrate-only"]
      environment = concat(local.api_environment, [
        { name = "Seed__AdminEmail", value = "admin@empreganet.com" },
        { name = "Seed__AdminUserName", value = "admin" },
      ])
      secrets    = local.migrate_secrets
      role       = "api"
      cpu_memory = "api"
    }
  }
}

# ------------------------- Task definitions --------------------------
resource "aws_ecs_task_definition" "this" {
  for_each = local.containers

  family                   = "${local.name_prefix}-${each.key}"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = tostring(var.capacity[each.value.cpu_memory].cpu)
  memory                   = tostring(var.capacity[each.value.cpu_memory].memory)
  execution_role_arn       = var.execution_role_arn
  task_role_arn            = var.task_role_arns[each.value.role]

  # As imagens são construídas em runner arm64; o Fargate ARM é mais barato.
  runtime_platform {
    operating_system_family = "LINUX"
    cpu_architecture        = "ARM64"
  }

  container_definitions = jsonencode([
    {
      name      = each.key
      image     = each.value.image
      essential = true
      command   = each.value.command

      # Tempo para a API e o Next encerrarem as requisições em andamento depois do SIGTERM.
      stopTimeout = 30

      portMappings = each.value.port == null ? [] : [{ containerPort = each.value.port, protocol = "tcp" }]

      environment = each.value.environment

      secrets = [
        for s in each.value.secrets : { name = s.name, valueFrom = "${var.secret_arn}:${s.key}::" }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.this[each.key].name
          "awslogs-region"        = data.aws_region.current.region
          "awslogs-stream-prefix" = each.key
        }
      }
    }
  ])

  tags = var.tags
}

# ------------------------- Services ------------------------------------
# O nome do serviço é <cluster>-<serviço>: o módulo iam deriva o ARN dele deste padrão para o papel do CI.
resource "aws_ecs_service" "this" {
  for_each = local.services

  name            = "${local.name_prefix}-${each.key}"
  cluster         = aws_ecs_cluster.this.arn
  task_definition = aws_ecs_task_definition.this[each.key].arn
  desired_count   = var.capacity[each.key].min
  launch_type     = "FARGATE"

  # Sem NAT gateway, a task precisa de IP público para puxar a imagem e ler o segredo. A entrada continua fechada
  # pelos security groups: só o load balancer chega nela.
  network_configuration {
    subnets          = var.subnet_ids
    security_groups  = [aws_security_group.task[each.key].id]
    assign_public_ip = true
  }

  # Uma imagem que nunca fica saudável é revertida para a revisão anterior, e a única task saudável nunca é trocada
  # antes de a nova estar de pé (mínimo 100%, máximo 200%).
  deployment_circuit_breaker {
    enable   = true
    rollback = true
  }
  deployment_minimum_healthy_percent = 100
  deployment_maximum_percent         = 200

  # A API só abre a porta depois de esperar o banco (até 180 s, ADR 0019); a carência tem de ser maior que essa espera,
  # senão o ECS mata a tarefa em ciclo quando o RDS ainda está religando.
  health_check_grace_period_seconds = 240

  load_balancer {
    target_group_arn = var.target_group_arns[each.key]
    container_name   = each.key
    container_port   = local.container_ports[each.key]
  }

  # A CI é dona da revisão da task definition (registra uma por deploy) e o Application Auto Scaling é dono da
  # contagem de tarefas (inclusive o zero da madrugada). Sem isto, o próximo apply desfaria os dois.
  lifecycle {
    ignore_changes = [task_definition, desired_count]
  }

  tags = var.tags
}
