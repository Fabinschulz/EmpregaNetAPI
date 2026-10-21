resource "aws_sns_topic" "this" {
  name = "empreganet-${var.environment}-alarms"
  tags = var.tags
}

resource "aws_sns_topic_subscription" "this" {
  topic_arn = aws_sns_topic.this.arn
  protocol  = "email"
  endpoint  = var.alarm_email
}
