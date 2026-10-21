variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "enabled" {
  description = "Cria as agendas; false deixa o RDS e os alarmes como estão"
  type        = bool
}

variable "db_instance_id" {
  description = "Identificador da instância RDS que a agenda liga e desliga"
  type        = string
}

variable "db_instance_arn" {
  description = "ARN da instância RDS, escopo da permissão de início e parada"
  type        = string
}

variable "running_task_alarm_names" {
  description = "Alarmes de tasks rodando cujas ações são desligadas de madrugada"
  type        = list(string)
}

variable "running_task_alarm_arns" {
  description = "ARN dos mesmos alarmes, escopo da permissão"
  type        = list(string)
}

variable "tags" {
  description = "Tags aplicadas à fila e à role"
  type        = map(string)
  default     = {}
}
