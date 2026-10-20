# Alertas informativos: o AWS Budgets atualiza com atraso e não é um teto. O limite rígido de custo é o max do Auto
# Scaling e a cota de vCPU do Fargate da conta.
locals {
  notifications = {
    actual_70    = { threshold = 70, type = "ACTUAL" }
    forecast_80  = { threshold = 80, type = "FORECASTED" }
    actual_90    = { threshold = 90, type = "ACTUAL" }
    forecast_100 = { threshold = 100, type = "FORECASTED" }
  }
}

resource "aws_budgets_budget" "this" {
  name         = "empreganet-${var.environment}-monthly"
  budget_type  = "COST"
  limit_amount = tostring(var.limit_usd)
  limit_unit   = "USD"
  time_unit    = "MONTHLY"

  dynamic "notification" {
    for_each = local.notifications

    content {
      comparison_operator        = "GREATER_THAN"
      threshold                  = notification.value.threshold
      threshold_type             = "PERCENTAGE"
      notification_type          = notification.value.type
      subscriber_email_addresses = [var.email]
    }
  }
}
