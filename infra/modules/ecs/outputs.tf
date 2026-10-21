output "cluster_name" {
  description = "Nome do cluster ECS"
  value       = aws_ecs_cluster.this.name
}

output "service_names" {
  description = "Nome do serviço ECS de cada serviço lógico (api, web)"
  value       = { for name, service in aws_ecs_service.this : name => service.name }
}

output "migration_task" {
  description = "Dados do ecs run-task da migração: a CI a executa antes de cada deploy e espera o código de saída"
  value = {
    cluster         = aws_ecs_cluster.this.name
    task_definition = aws_ecs_task_definition.this["migrate"].family
    container_name  = "migrate"
    subnets         = var.subnet_ids
    security_groups = [aws_security_group.task["api"].id]
  }
}
