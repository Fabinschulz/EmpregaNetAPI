data "aws_caller_identity" "current" {}
data "aws_region" "current" {}

locals {
  # As chaves são endereços de recurso: renomear uma destrói a role. Para renomear, use um bloco moved.
  services = toset(["api", "web"])

  name_prefix = "empreganet-${var.environment}"

  account_id = data.aws_caller_identity.current.account_id
  region     = data.aws_region.current.region

  # Os módulos ecs criam log groups, serviços e task definitions com estes nomes; os ARNs são derivados do nome
  # (e não lidos de uma saída) para o iam vir antes do ecs no grafo. O sufixo :* dos log groups é exigido pela AWS.
  log_group_arns = [
    for name in concat(tolist(local.services), ["migrate"]) :
    "arn:aws:logs:${local.region}:${local.account_id}:log-group:/ecs/${local.name_prefix}/${name}:*"
  ]
  ecs_service_arns = [
    for name in local.services :
    "arn:aws:ecs:${local.region}:${local.account_id}:service/${local.name_prefix}/${local.name_prefix}-${name}"
  ]
  migrate_task_definition_arn = "arn:aws:ecs:${local.region}:${local.account_id}:task-definition/${local.name_prefix}-migrate:*"
  cluster_task_arn            = "arn:aws:ecs:${local.region}:${local.account_id}:task/${local.name_prefix}/*"
  cluster_arn                 = "arn:aws:ecs:${local.region}:${local.account_id}:cluster/${local.name_prefix}"

  # Só o ECS assume estas roles, e só em nome desta conta (proteção contra confused deputy).
  ecs_tasks_trust = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect    = "Allow"
        Principal = { Service = "ecs-tasks.amazonaws.com" }
        Action    = "sts:AssumeRole"
        Condition = {
          StringEquals = { "aws:SourceAccount" = local.account_id }
        }
      }
    ]
  })
}

# ------------------------- Task execution role -----------------------
# Usada pelo ECS (não pela aplicação) para puxar a imagem, gravar logs e ler o segredo antes de a task iniciar.
resource "aws_iam_role" "execution" {
  name               = "${local.name_prefix}-ecs-execution"
  assume_role_policy = local.ecs_tasks_trust
  tags               = var.tags
}

resource "aws_iam_role_policy" "execution" {
  name = "${local.name_prefix}-ecs-execution"
  role = aws_iam_role.execution.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        # A AWS não permite restringir este token por recurso.
        Sid      = "EcrAuthorizationToken"
        Effect   = "Allow"
        Action   = ["ecr:GetAuthorizationToken"]
        Resource = "*"
      },
      {
        Sid      = "EcrPull"
        Effect   = "Allow"
        Action   = ["ecr:BatchGetImage", "ecr:GetDownloadUrlForLayer"]
        Resource = values(var.repository_arns)
      },
      {
        Sid      = "Logs"
        Effect   = "Allow"
        Action   = ["logs:CreateLogStream", "logs:PutLogEvents"]
        Resource = local.log_group_arns
      },
      {
        Sid      = "RuntimeSecret"
        Effect   = "Allow"
        Action   = ["secretsmanager:GetSecretValue"]
        Resource = var.secret_arn
      }
    ]
  })
}

# ------------------------- Task roles --------------------------------
# A role que a aplicação assume. A API só envia e-mail (SES); o frontend não chama nenhum serviço da AWS.
resource "aws_iam_role" "task" {
  for_each = local.services

  name               = "${local.name_prefix}-${each.key}-task"
  assume_role_policy = local.ecs_tasks_trust
  tags               = var.tags
}

resource "aws_iam_role_policy" "api_ses" {
  count = var.ses_identity == null ? 0 : 1

  name = "${local.name_prefix}-api-ses"
  role = aws_iam_role.task["api"].id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = ["ses:SendEmail"]
        # O ARN da identidade vale para e-mail e para domínio verificado.
        Resource = "arn:aws:ses:${local.region}:${local.account_id}:identity/${var.ses_identity}"
      }
    ]
  })
}

# ------------------------- GitHub OIDC -------------------------------
# O provedor é único por conta: se outro projeto já o criou, informe o ARN em github_oidc_provider_arn.
resource "aws_iam_openid_connect_provider" "github" {
  count = var.github_oidc_provider_arn == null ? 1 : 0

  url            = "https://token.actions.githubusercontent.com"
  client_id_list = ["sts.amazonaws.com"]
  tags           = var.tags
}

locals {
  github_oidc_provider_arn = coalesce(var.github_oidc_provider_arn, one(aws_iam_openid_connect_provider.github[*].arn))

  # Subject imutável do OIDC do GitHub: o token traz os IDs numéricos do dono e do repositório, então um repositório
  # apagado e recriado com o mesmo nome não assume esta role.
  github_oidc_subject = format(
    "repo:%s@%d/%s@%d:ref:refs/heads/master",
    split("/", var.github_repository)[0],
    var.github_repository_ids.owner_id,
    split("/", var.github_repository)[1],
    var.github_repository_ids.repository_id,
  )
}

# ------------------------- CI role -----------------------------------
# Assumida pelo workflow de deploy sem chave de acesso. Só a branch master deste repositório entra.
resource "aws_iam_role" "github_actions" {
  name = "${local.name_prefix}-github-actions"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect    = "Allow"
        Principal = { Federated = local.github_oidc_provider_arn }
        Action    = "sts:AssumeRoleWithWebIdentity"
        Condition = {
          StringEquals = {
            "token.actions.githubusercontent.com:aud" = "sts.amazonaws.com"
            "token.actions.githubusercontent.com:sub" = local.github_oidc_subject
          }
        }
      }
    ]
  })

  tags = var.tags
}

resource "aws_iam_role_policy" "github_actions" {
  name = "${local.name_prefix}-github-actions"
  role = aws_iam_role.github_actions.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        # A AWS não permite restringir este token por recurso.
        Sid      = "EcrAuthorizationToken"
        Effect   = "Allow"
        Action   = ["ecr:GetAuthorizationToken"]
        Resource = "*"
      },
      {
        Sid    = "EcrPush"
        Effect = "Allow"
        Action = [
          "ecr:BatchCheckLayerAvailability",
          "ecr:InitiateLayerUpload",
          "ecr:UploadLayerPart",
          "ecr:CompleteLayerUpload",
          "ecr:PutImage",
        ]
        Resource = values(var.repository_arns)
      },
      {
        # O cd-image consulta a tag antes do build (re-execução do mesmo commit) e lê o digest depois do push.
        Sid      = "EcrRead"
        Effect   = "Allow"
        Action   = ["ecr:DescribeImages", "ecr:BatchGetImage"]
        Resource = values(var.repository_arns)
      },
      {
        # A AWS não permite restringir estas duas ações por recurso.
        Sid      = "EcsTaskDefinition"
        Effect   = "Allow"
        Action   = ["ecs:RegisterTaskDefinition", "ecs:DescribeTaskDefinition"]
        Resource = "*"
      },
      {
        Sid      = "EcsServices"
        Effect   = "Allow"
        Action   = ["ecs:UpdateService", "ecs:DescribeServices"]
        Resource = local.ecs_service_arns
      },
      {
        # A tarefa de migração roda antes de cada deploy; o CI só a dispara e acompanha.
        Sid      = "EcsMigrationRun"
        Effect   = "Allow"
        Action   = ["ecs:RunTask"]
        Resource = local.migrate_task_definition_arn
        Condition = {
          ArnEquals = { "ecs:cluster" = local.cluster_arn }
        }
      },
      {
        Sid      = "EcsMigrationWatch"
        Effect   = "Allow"
        Action   = ["ecs:DescribeTasks"]
        Resource = local.cluster_task_arn
      },
      {
        # Registrar a task definition entrega as roles ao ECS; só ao ECS e só estas.
        Sid      = "PassEcsRoles"
        Effect   = "Allow"
        Action   = ["iam:PassRole"]
        Resource = concat([aws_iam_role.execution.arn], [for role in aws_iam_role.task : role.arn])
        Condition = {
          StringEquals = { "iam:PassedToService" = "ecs-tasks.amazonaws.com" }
        }
      }
    ]
  })
}
