output "alb_dns_name" {
  description = "Nome DNS do load balancer, para apontar o hostname quando o DNS é gerenciado fora desta conta"
  value       = module.alb.dns_name
}

output "ecr_repository_urls" {
  description = "URL dos repositórios ECR por serviço (api, web)"
  value       = module.ecr.repository_urls
}

output "github_actions_role_arn" {
  description = "Papel que o workflow de deploy assume por OIDC; vai para a variável AWS_DEPLOY_ROLE_ARN do GitHub"
  value       = module.iam.github_actions_role_arn
}

output "app_secret_arn" {
  description = "Segredo de runtime, criado sem valor: preencha as chaves listadas no README antes do primeiro deploy"
  value       = module.secrets.secret_arn
}

output "database_endpoint" {
  description = "Endereço do RDS, para montar a connection string"
  value       = module.rds.endpoint
}

output "cluster_name" {
  description = "Cluster ECS"
  value       = module.ecs.cluster_name
}

output "service_names" {
  description = "Serviços ECS por nome lógico (api, web)"
  value       = module.ecs.service_names
}

output "migration_task" {
  description = "Dados para executar a tarefa de migração (dotnet EmpregaNet.Api.dll --migrate-only) antes de cada deploy"
  value       = module.ecs.migration_task
}
