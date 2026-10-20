variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz)"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "ecs_cluster_name" {
  description = "Cluster ECS dos serviços"
  type        = string
}

variable "service_names" {
  description = "Nome do serviço ECS de cada serviço lógico (api, web)"
  type        = map(string)
}

variable "capacity" {
  description = "Limites min e max do Auto Scaling por serviço lógico, usados na abertura da janela de operação"
  type = map(object({
    cpu    = number
    memory = number
    min    = number
    max    = number
  }))
}

variable "schedule_enabled" {
  description = "Cria as ações agendadas de abertura (06:50) e fechamento (00:00) da janela de operação"
  type        = bool
}
