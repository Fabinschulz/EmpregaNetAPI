output "topic_arn" {
  description = "ARN do tópico de alarmes"
  value       = aws_sns_topic.this.arn
}
