output "execution_role_arn" {
  description = "Task execution role, compartilhada pelos serviços"
  value       = aws_iam_role.execution.arn
}

output "task_role_arns" {
  description = "Task role de cada serviço (api, web)"
  value       = { for name, role in aws_iam_role.task : name => role.arn }
}

output "github_actions_role_arn" {
  description = "Papel que o workflow de deploy assume por OIDC"
  value       = aws_iam_role.github_actions.arn
}
