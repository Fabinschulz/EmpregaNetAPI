variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "vpc_cidr" {
  description = "CIDR /16 da VPC; as subnets são derivadas dele"
  type        = string
}

variable "tags" {
  description = "Tags aplicadas aos recursos de rede"
  type        = map(string)
  default     = {}
}
