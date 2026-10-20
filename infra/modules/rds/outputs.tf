output "security_group_id" {
  description = "Security group do banco, que recebe a entrada 5432 das tarefas"
  value       = aws_security_group.this.id
}

output "instance_id" {
  description = "Identificador da instância, usado pela agenda de início e parada"
  value       = aws_db_instance.this.identifier
}

output "instance_arn" {
  description = "ARN da instância, escopo da permissão de início e parada"
  value       = aws_db_instance.this.arn
}

output "endpoint" {
  description = "Endereço do banco (host:porta)"
  value       = aws_db_instance.this.endpoint
}
