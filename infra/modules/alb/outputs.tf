output "security_group_id" {
  description = "Security group do load balancer, origem permitida na entrada das tarefas"
  value       = aws_security_group.this.id
}

output "target_group_arns" {
  description = "ARN do target group de cada serviço (api, web)"
  value = {
    api = aws_lb_target_group.api.arn
    web = aws_lb_target_group.web.arn
  }

  # O ECS só registra um serviço em um target group já ligado a um listener; quem consome esta saída espera as regras.
  depends_on = [aws_lb_listener.https, aws_lb_listener_rule.api]
}

output "dns_name" {
  description = "Nome DNS do load balancer"
  value       = aws_lb.this.dns_name
}

output "load_balancer_arn_suffix" {
  description = "Sufixo do ARN do load balancer, dimensão LoadBalancer das métricas do CloudWatch"
  value       = aws_lb.this.arn_suffix
}
