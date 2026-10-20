locals {
  name_prefix = "empreganet-${var.environment}"
}

# ------------------------- ECS running tasks -------------------------
# Um serviço que cai (processo em loop de falha, segredo ausente, imagem que não puxa) fica sem task rodando.
# Se o serviço some, a métrica some junto: por isso dado ausente conta como violação. De madrugada o zero é esperado;
# o módulo scheduler desliga as ações destes alarmes às 00:00 e as religa às 07:10.
resource "aws_cloudwatch_metric_alarm" "ecs_running_tasks" {
  for_each = var.ecs_service_names

  alarm_name        = "${each.value}-running-tasks"
  alarm_description = "O serviço ECS ${each.value} está sem nenhuma task rodando há 10 minutos: veja o motivo da última task parada no ECS e os logs do serviço no CloudWatch."

  namespace           = "ECS/ContainerInsights"
  metric_name         = "RunningTaskCount"
  dimensions          = { ClusterName = var.ecs_cluster_name, ServiceName = each.value }
  statistic           = "Minimum"
  period              = 300
  evaluation_periods  = 2
  threshold           = 1
  comparison_operator = "LessThanThreshold"
  treat_missing_data  = "breaching"

  alarm_actions = [var.alarm_topic_arn]
  ok_actions    = [var.alarm_topic_arn]

  tags = var.tags
}

# ------------------------- Load balancer ------------------------------
resource "aws_cloudwatch_metric_alarm" "alb_target_5xx" {
  alarm_name        = "${local.name_prefix}-alb-target-5xx"
  alarm_description = "As aplicações responderam 5xx dez vezes ou mais em 5 minutos: veja os logs da API e do frontend."

  namespace           = "AWS/ApplicationELB"
  metric_name         = "HTTPCode_Target_5XX_Count"
  dimensions          = { LoadBalancer = var.load_balancer_name }
  statistic           = "Sum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 10
  comparison_operator = "GreaterThanOrEqualToThreshold"
  treat_missing_data  = "notBreaching"

  alarm_actions = [var.alarm_topic_arn]
  ok_actions    = [var.alarm_topic_arn]

  tags = var.tags
}

# ------------------------- RDS ----------------------------------------
# Com o RDS parado de madrugada as métricas somem; dado ausente não conta como violação.
resource "aws_cloudwatch_metric_alarm" "rds_cpu" {
  alarm_name        = "${local.name_prefix}-rds-cpu"
  alarm_description = "A CPU do RDS ficou acima de 80% por 15 minutos: a instância micro pode estar pequena para a carga."

  namespace           = "AWS/RDS"
  metric_name         = "CPUUtilization"
  dimensions          = { DBInstanceIdentifier = var.db_instance_id }
  statistic           = "Average"
  period              = 300
  evaluation_periods  = 3
  threshold           = 80
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "notBreaching"

  alarm_actions = [var.alarm_topic_arn]
  ok_actions    = [var.alarm_topic_arn]

  tags = var.tags
}

resource "aws_cloudwatch_metric_alarm" "rds_free_storage" {
  alarm_name        = "${local.name_prefix}-rds-free-storage"
  alarm_description = "Restam menos de 2 GB livres no RDS."

  namespace           = "AWS/RDS"
  metric_name         = "FreeStorageSpace"
  dimensions          = { DBInstanceIdentifier = var.db_instance_id }
  statistic           = "Minimum"
  period              = 300
  evaluation_periods  = 1
  threshold           = 2147483648
  comparison_operator = "LessThanThreshold"
  treat_missing_data  = "notBreaching"

  alarm_actions = [var.alarm_topic_arn]
  ok_actions    = [var.alarm_topic_arn]

  tags = var.tags
}
