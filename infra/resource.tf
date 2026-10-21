resource "terraform_data" "environment_guard" {
  input = terraform.workspace

  lifecycle {
    precondition {
      condition     = terraform.workspace != "default"
      error_message = "Selecione o workspace do ambiente antes do plan/apply (ex.: terraform workspace select homolog)."
    }
  }
}

locals {
  ses_from_email = var.ses_from_email != null ? var.ses_from_email : (try(strcontains(var.ses_identity, "@"), false) ? var.ses_identity : "")

  tags = {
    Project     = "empreganet"
    Environment = terraform.workspace
    ManagedBy   = "terraform"
    Repository  = "EmpregaNetAPI"
  }
}

# Em Production a API recusa subir sem o SES configurado (e sem o certificado do Data Protection, no segredo).
resource "terraform_data" "production_guard" {
  input = var.aspnetcore_environment

  lifecycle {
    precondition {
      condition     = var.aspnetcore_environment != "Production" || (var.ses_identity != null && local.ses_from_email != "")
      error_message = "Production exige ses_identity e um endereço remetente (ses_from_email, ou ses_identity sendo um e-mail)."
    }
  }
}

module "network" {
  source = "./modules/network"

  environment = terraform.workspace
  vpc_cidr    = var.vpc_cidr
  tags        = local.tags
}

module "sns" {
  source = "./modules/sns"

  environment = terraform.workspace
  alarm_email = var.alarm_email
  tags        = local.tags
}

module "ecr" {
  source = "./modules/ecr"

  environment = terraform.workspace
  tags        = local.tags
}

module "secrets" {
  source = "./modules/secrets"

  environment = terraform.workspace
  tags        = local.tags
}

module "rds" {
  source = "./modules/rds"

  environment         = terraform.workspace
  vpc_id              = module.network.vpc_id
  private_subnet_ids  = module.network.private_subnet_ids
  instance_class      = var.db_instance_class
  allocated_storage   = var.db_allocated_storage
  deletion_protection = var.db_deletion_protection

  master_password         = var.db_master_password
  master_password_version = var.db_master_password_version

  tags = local.tags
}

# Vem antes do ecs: deriva os ARNs de log groups, serviços e task definitions pelo nome.
module "iam" {
  source = "./modules/iam"

  environment              = terraform.workspace
  repository_arns          = module.ecr.repository_arns
  secret_arn               = module.secrets.secret_arn
  ses_identity             = var.ses_identity
  github_repository        = var.github_repository
  github_repository_ids    = var.github_repository_ids
  github_oidc_provider_arn = var.github_oidc_provider_arn
  tags                     = local.tags
}

module "alb" {
  source = "./modules/alb"

  environment       = terraform.workspace
  vpc_id            = module.network.vpc_id
  public_subnet_ids = module.network.public_subnet_ids
  certificate_arn   = var.certificate_arn
  domain_name       = var.domain_name
  route53_zone_id   = var.route53_zone_id
  tags              = local.tags
}

module "ecs" {
  source = "./modules/ecs"

  environment             = terraform.workspace
  vpc_id                  = module.network.vpc_id
  subnet_ids              = module.network.public_subnet_ids
  alb_subnet_cidrs        = module.network.public_subnet_cidrs
  alb_security_group_id   = module.alb.security_group_id
  target_group_arns       = module.alb.target_group_arns
  database_security_group = module.rds.security_group_id
  repository_urls         = module.ecr.repository_urls
  image_tag               = var.image_tag
  execution_role_arn      = module.iam.execution_role_arn
  task_role_arns          = module.iam.task_role_arns
  secret_arn              = module.secrets.secret_arn
  capacity                = var.service_capacity
  aspnetcore_environment  = var.aspnetcore_environment
  domain_name             = var.domain_name
  ses_identity            = var.ses_identity
  ses_from_email          = local.ses_from_email
  tags                    = local.tags
}

module "autoscaling" {
  source = "./modules/autoscaling"

  environment      = terraform.workspace
  ecs_cluster_name = module.ecs.cluster_name
  service_names    = module.ecs.service_names
  capacity         = var.service_capacity
  schedule_enabled = var.schedule_enabled
}

module "cloudwatch" {
  source = "./modules/cloudwatch"

  environment        = terraform.workspace
  alarm_topic_arn    = module.sns.topic_arn
  ecs_cluster_name   = module.ecs.cluster_name
  ecs_service_names  = module.ecs.service_names
  load_balancer_name = module.alb.load_balancer_arn_suffix
  db_instance_id     = module.rds.instance_id
  tags               = local.tags
}

module "scheduler" {
  source = "./modules/scheduler"

  environment              = terraform.workspace
  enabled                  = var.schedule_enabled
  db_instance_id           = module.rds.instance_id
  db_instance_arn          = module.rds.instance_arn
  running_task_alarm_names = module.cloudwatch.running_task_alarm_names
  running_task_alarm_arns  = module.cloudwatch.running_task_alarm_arns
  tags                     = local.tags
}

module "budgets" {
  source = "./modules/budgets"

  environment = terraform.workspace
  limit_usd   = var.budget_limit_usd
  email       = var.alarm_email
}
