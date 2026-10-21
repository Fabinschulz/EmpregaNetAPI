variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado no nome"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "limit_usd" {
  description = "Teto mensal em dólares; os alertas disparam em 70% e 90% do gasto real e em 80% e 100% do previsto"
  type        = number

  validation {
    condition     = var.limit_usd > 0
    error_message = "O teto deve ser maior que zero."
  }
}

variable "email" {
  description = "E-mail que recebe os alertas"
  type        = string
}
