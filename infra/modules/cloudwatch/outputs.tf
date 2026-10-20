output "running_task_alarm_names" {
  description = "Nomes dos alarmes de tasks rodando, que a agenda desliga de madrugada"
  value       = [for alarm in aws_cloudwatch_metric_alarm.ecs_running_tasks : alarm.alarm_name]
}

output "running_task_alarm_arns" {
  description = "ARN dos alarmes de tasks rodando, escopo da permissão da agenda"
  value       = [for alarm in aws_cloudwatch_metric_alarm.ecs_running_tasks : alarm.arn]
}
