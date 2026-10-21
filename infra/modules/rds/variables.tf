variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "vpc_id" {
  description = "VPC onde fica o security group do banco"
  type        = string
}

variable "private_subnet_ids" {
  description = "Subnets privadas isoladas, em duas zonas, do grupo de subnets do RDS"
  type        = list(string)

  validation {
    condition     = length(var.private_subnet_ids) >= 2
    error_message = "O RDS exige subnets em ao menos duas zonas de disponibilidade."
  }
}

variable "engine_major_version" {
  description = "Versão principal do PostgreSQL; a mesma dos testes de integração da CI"
  type        = string
  default     = "15"
}

variable "instance_class" {
  description = "Classe da instância"
  type        = string
}

variable "allocated_storage" {
  description = "Armazenamento em GB (gp3)"
  type        = number
}

variable "backup_retention_days" {
  description = "Dias de backup automático"
  type        = number
  default     = 7
}

variable "deletion_protection" {
  description = "Protege a instância contra exclusão"
  type        = bool
}

variable "master_password" {
  description = "Senha do usuário mestre. Efêmera e write-only: só é lida quando master_password_version muda (e na criação); informe-a por TF_VAR_db_master_password"
  type        = string
  default     = null
  ephemeral   = true
  sensitive   = true
}

variable "master_password_version" {
  description = "Aumente para o Terraform enviar de novo a senha ao RDS (troca de senha)"
  type        = number
  default     = 1
}

variable "tags" {
  description = "Tags aplicadas aos recursos do banco"
  type        = map(string)
  default     = {}
}
