output "repository_urls" {
  description = "URL dos repositórios ECR por serviço (api, web)"
  value       = { for name, repo in aws_ecr_repository.this : name => repo.repository_url }
}

output "repository_arns" {
  description = "ARN dos repositórios ECR por serviço, escopo das permissões de pull e push"
  value       = { for name, repo in aws_ecr_repository.this : name => repo.arn }
}
