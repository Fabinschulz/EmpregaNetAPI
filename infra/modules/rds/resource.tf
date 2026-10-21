locals {
  name_prefix = "empreganet-${var.environment}"
}

# A entrada 5432 vem do security group das tarefas e é criada no módulo ecs, que o conhece. Sem saída: o banco não inicia conexões.
resource "aws_security_group" "this" {
  name        = "${local.name_prefix}-db"
  description = "RDS PostgreSQL da EmpregaNet"
  vpc_id      = var.vpc_id
  tags        = var.tags
}

resource "aws_db_subnet_group" "this" {
  name       = local.name_prefix
  subnet_ids = var.private_subnet_ids
  tags       = var.tags
}

resource "aws_db_parameter_group" "this" {
  name_prefix = "${local.name_prefix}-"
  family      = "postgres${var.engine_major_version}"

  parameter {
    name  = "rds.force_ssl"
    value = "1"
  }

  tags = var.tags

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_db_instance" "this" {
  identifier = local.name_prefix

  engine         = "postgres"
  engine_version = var.engine_major_version
  instance_class = var.instance_class

  allocated_storage = var.allocated_storage
  storage_type      = "gp3"
  storage_encrypted = true

  db_name  = "empreganet"
  username = "empreganet_admin"
  # Senha estática, write-only: não vai para o state nem para o plan. Não se usa manage_master_user_password porque o
  # RDS rotaciona essa senha a cada 7 dias e a connection string da aplicação ficaria inválida (ADR 0020).
  password_wo         = var.master_password
  password_wo_version = var.master_password_version

  db_subnet_group_name   = aws_db_subnet_group.this.name
  vpc_security_group_ids = [aws_security_group.this.id]
  parameter_group_name   = aws_db_parameter_group.this.name
  publicly_accessible    = false
  multi_az               = false

  backup_retention_period = var.backup_retention_days
  copy_tags_to_snapshot   = true

  auto_minor_version_upgrade = true
  deletion_protection        = var.deletion_protection
  skip_final_snapshot        = false
  final_snapshot_identifier  = "${local.name_prefix}-final"

  tags = var.tags
}
