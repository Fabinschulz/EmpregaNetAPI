data "aws_caller_identity" "current" {}

locals {
  name_prefix = "empreganet-${var.environment}"
  timezone    = "America/Sao_Paulo"

  # Janela de operação 07h-00h. O RDS liga 06:25 (leva minutos) e as tarefas voltam às 06:50, pelo módulo autoscaling.
  # As ações dos alarmes de "sem task rodando" ficam desligadas de 00:00 às 07:10, quando o zero é esperado.
  schedules = {
    rds_start = {
      expression = "cron(25 6 * * ? *)"
      target     = "arn:aws:scheduler:::aws-sdk:rds:startDBInstance"
      input      = { DbInstanceIdentifier = var.db_instance_id }
    }
    rds_stop = {
      expression = "cron(15 0 * * ? *)"
      target     = "arn:aws:scheduler:::aws-sdk:rds:stopDBInstance"
      input      = { DbInstanceIdentifier = var.db_instance_id }
    }
    alarms_enable = {
      expression = "cron(10 7 * * ? *)"
      target     = "arn:aws:scheduler:::aws-sdk:cloudwatch:enableAlarmActions"
      input      = { AlarmNames = var.running_task_alarm_names }
    }
    alarms_disable = {
      expression = "cron(0 0 * * ? *)"
      target     = "arn:aws:scheduler:::aws-sdk:cloudwatch:disableAlarmActions"
      input      = { AlarmNames = var.running_task_alarm_names }
    }
  }
}

# ------------------------- Dead-letter queue --------------------------
# Execução que esgota as tentativas (por exemplo, o RDS não responde ao início) fica aqui em vez de se perder.
resource "aws_sqs_queue" "dlq" {
  count = var.enabled ? 1 : 0

  name                      = "${local.name_prefix}-scheduler-dlq"
  message_retention_seconds = 1209600
  sqs_managed_sse_enabled   = true
  tags                      = var.tags
}

# ------------------------- Role do scheduler --------------------------
resource "aws_iam_role" "scheduler" {
  count = var.enabled ? 1 : 0

  name = "${local.name_prefix}-scheduler"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect    = "Allow"
        Principal = { Service = "scheduler.amazonaws.com" }
        Action    = "sts:AssumeRole"
        Condition = {
          StringEquals = { "aws:SourceAccount" = data.aws_caller_identity.current.account_id }
        }
      }
    ]
  })

  tags = var.tags
}

resource "aws_iam_role_policy" "scheduler" {
  count = var.enabled ? 1 : 0

  name = "${local.name_prefix}-scheduler"
  role = aws_iam_role.scheduler[0].id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid      = "RdsPower"
        Effect   = "Allow"
        Action   = ["rds:StartDBInstance", "rds:StopDBInstance"]
        Resource = var.db_instance_arn
      },
      {
        Sid      = "AlarmActions"
        Effect   = "Allow"
        Action   = ["cloudwatch:EnableAlarmActions", "cloudwatch:DisableAlarmActions"]
        Resource = var.running_task_alarm_arns
      },
      {
        Sid      = "DeadLetter"
        Effect   = "Allow"
        Action   = ["sqs:SendMessage"]
        Resource = aws_sqs_queue.dlq[0].arn
      }
    ]
  })
}

# ------------------------- Agendas ------------------------------------
resource "aws_scheduler_schedule" "this" {
  for_each = { for name, schedule in local.schedules : name => schedule if var.enabled }

  name                         = "${local.name_prefix}-${replace(each.key, "_", "-")}"
  schedule_expression          = each.value.expression
  schedule_expression_timezone = local.timezone

  flexible_time_window {
    mode = "OFF"
  }

  target {
    arn      = each.value.target
    role_arn = aws_iam_role.scheduler[0].arn
    input    = jsonencode(each.value.input)

    retry_policy {
      maximum_retry_attempts       = 5
      maximum_event_age_in_seconds = 3600
    }

    dead_letter_config {
      arn = aws_sqs_queue.dlq[0].arn
    }
  }
}
