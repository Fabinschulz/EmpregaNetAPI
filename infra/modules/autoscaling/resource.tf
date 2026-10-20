locals {
  timezone = "America/Sao_Paulo"
}

# ------------------------- Scalable targets ---------------------------
# Os limites mudam duas vezes por dia pelas ações agendadas abaixo; depois do primeiro apply o Terraform não os
# reescreve (ignore_changes), senão cada apply religaria ou desligaria os serviços fora de hora.
resource "aws_appautoscaling_target" "this" {
  for_each = var.service_names

  service_namespace  = "ecs"
  scalable_dimension = "ecs:service:DesiredCount"
  resource_id        = "service/${var.ecs_cluster_name}/${each.value}"
  min_capacity       = var.capacity[each.key].min
  max_capacity       = var.capacity[each.key].max

  lifecycle {
    ignore_changes = [min_capacity, max_capacity]
  }
}

# ------------------------- Target tracking ----------------------------
resource "aws_appautoscaling_policy" "cpu" {
  for_each = aws_appautoscaling_target.this

  name               = "${each.value.resource_id}-cpu"
  policy_type        = "TargetTrackingScaling"
  service_namespace  = each.value.service_namespace
  scalable_dimension = each.value.scalable_dimension
  resource_id        = each.value.resource_id

  target_tracking_scaling_policy_configuration {
    target_value       = 60
    scale_out_cooldown = 60
    scale_in_cooldown  = 600

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
  }
}

# A memória só expande: reduzir por memória oscilaria com o uso do cache em memória da aplicação.
resource "aws_appautoscaling_policy" "memory" {
  for_each = aws_appautoscaling_target.this

  name               = "${each.value.resource_id}-memory"
  policy_type        = "TargetTrackingScaling"
  service_namespace  = each.value.service_namespace
  scalable_dimension = each.value.scalable_dimension
  resource_id        = each.value.resource_id

  target_tracking_scaling_policy_configuration {
    target_value       = 70
    scale_out_cooldown = 60
    scale_in_cooldown  = 600
    disable_scale_in   = true

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageMemoryUtilization"
    }
  }
}

# ------------------------- Agenda -------------------------------------
# Janela de operação 07h-00h (America/Sao_Paulo). O serviço volta antes das 07h para a tarefa já estar de pé; o RDS
# é ligado às 06:25 pelo módulo scheduler.
resource "aws_appautoscaling_scheduled_action" "open" {
  for_each = { for name, target in aws_appautoscaling_target.this : name => target if var.schedule_enabled }

  name               = "${each.value.resource_id}-open"
  service_namespace  = each.value.service_namespace
  scalable_dimension = each.value.scalable_dimension
  resource_id        = each.value.resource_id
  schedule           = "cron(50 6 * * ? *)"
  timezone           = local.timezone

  scalable_target_action {
    min_capacity = var.capacity[each.key].min
    max_capacity = var.capacity[each.key].max
  }
}

resource "aws_appautoscaling_scheduled_action" "close" {
  for_each = { for name, target in aws_appautoscaling_target.this : name => target if var.schedule_enabled }

  name               = "${each.value.resource_id}-close"
  service_namespace  = each.value.service_namespace
  scalable_dimension = each.value.scalable_dimension
  resource_id        = each.value.resource_id
  schedule           = "cron(0 0 * * ? *)"
  timezone           = local.timezone

  scalable_target_action {
    min_capacity = 0
    max_capacity = 0
  }
}
