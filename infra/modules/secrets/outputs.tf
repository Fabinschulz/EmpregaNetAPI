output "secret_arn" {
  description = "ARN do segredo de runtime, de onde as task definitions leem cada chave"
  value       = aws_secretsmanager_secret.this.arn
}
