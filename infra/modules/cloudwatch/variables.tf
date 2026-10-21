variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "alarm_topic_arn" {
  description = "Tópico SNS que recebe as mudanças de estado dos alarmes"
  type        = string
}

variable "ecs_cluster_name" {
  description = "Cluster ECS, dimensão ClusterName dos alarmes de tasks rodando"
  type        = string
}

variable "ecs_service_names" {
  description = "Nome do serviço ECS de cada serviço lógico (api, web)"
  type        = map(string)
}

variable "load_balancer_name" {
  description = "Sufixo do ARN do load balancer (dimensão LoadBalancer das métricas)"
  type        = string
}

variable "db_instance_id" {
  description = "Identificador da instância RDS"
  type        = string
}

variable "tags" {
  description = "Tags aplicadas aos alarmes"
  type        = map(string)
  default     = {}
}
