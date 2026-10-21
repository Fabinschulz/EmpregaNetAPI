locals {
  name_prefix = "empreganet-${var.environment}"
}

resource "aws_security_group" "this" {
  name        = "${local.name_prefix}-alb"
  description = "Load balancer publico da EmpregaNet"
  vpc_id      = var.vpc_id
  tags        = var.tags
}

# A única entrada da internet na conta. A porta 80 só existe para redirecionar para HTTPS.
resource "aws_vpc_security_group_ingress_rule" "https" {
  security_group_id = aws_security_group.this.id
  description       = "HTTPS vindo da internet"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 443
  to_port           = 443
}

resource "aws_vpc_security_group_ingress_rule" "http_redirect" {
  security_group_id = aws_security_group.this.id
  description       = "HTTP vindo da internet, so para redirecionar para HTTPS"
  cidr_ipv4         = "0.0.0.0/0"
  ip_protocol       = "tcp"
  from_port         = 80
  to_port           = 80
}

# A saída para as tarefas (8080 e 3000) é criada no módulo ecs, que conhece os security groups delas.

resource "aws_lb" "this" {
  name               = local.name_prefix
  load_balancer_type = "application"
  internal           = false
  subnets            = var.public_subnet_ids
  security_groups    = [aws_security_group.this.id]

  drop_invalid_header_fields = true

  tags = var.tags
}

# Health check na liveness: com /health/ready o ECS substituiria tarefas em laço se o banco estivesse fora.
resource "aws_lb_target_group" "api" {
  name        = "${local.name_prefix}-api"
  vpc_id      = var.vpc_id
  target_type = "ip"
  protocol    = "HTTP"
  port        = 8080

  # 30 s em vez dos 300 s padrão: o rollout não demora.
  deregistration_delay = 30

  health_check {
    path                = "/health/live"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 3
  }

  tags = var.tags
}

resource "aws_lb_target_group" "web" {
  name        = "${local.name_prefix}-web"
  vpc_id      = var.vpc_id
  target_type = "ip"
  protocol    = "HTTP"
  port        = 3000

  deregistration_delay = 30

  # /healthz não depende da API: uma falha dela não tira o frontend de rotação.
  health_check {
    path                = "/healthz"
    matcher             = "200"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 2
    unhealthy_threshold = 3
  }

  tags = var.tags
}

resource "aws_lb_listener" "https" {
  load_balancer_arn = aws_lb.this.arn
  port              = 443
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = var.certificate_arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.web.arn
  }

  tags = var.tags
}

# /api/* vai para a API e o resto para o Next: mesma origem, o cookie host-only continua valendo.
resource "aws_lb_listener_rule" "api" {
  listener_arn = aws_lb_listener.https.arn
  priority     = 10

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }

  condition {
    path_pattern {
      values = ["/api/*"]
    }
  }

  tags = var.tags
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.this.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = "redirect"

    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }

  tags = var.tags
}

resource "aws_route53_record" "this" {
  count = var.route53_zone_id == null ? 0 : 1

  zone_id = var.route53_zone_id
  name    = var.domain_name
  type    = "A"

  alias {
    name                   = aws_lb.this.dns_name
    zone_id                = aws_lb.this.zone_id
    evaluate_target_health = true
  }
}
