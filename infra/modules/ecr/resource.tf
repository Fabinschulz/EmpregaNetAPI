locals {
  services = toset(["api", "web"])

  name_prefix = "empreganet-${var.environment}"
}

resource "aws_ecr_repository" "this" {
  for_each = local.services

  name = "${local.name_prefix}-${each.key}"
  # Tag imutável: um deploy aponta sempre para o mesmo conteúdo e o rollback nomeia uma tag que não mudou.
  image_tag_mutability = "IMMUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = var.tags
}

resource "aws_ecr_lifecycle_policy" "this" {
  for_each = aws_ecr_repository.this

  repository = each.value.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Mantém as 20 imagens mais recentes e expira as demais"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 20
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}
