variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }

  # O nome dos target groups (empreganet-<env>-<serviço>) tem no máximo 32 caracteres.
  validation {
    condition     = length(var.environment) <= 17
    error_message = "O ambiente deve ter no máximo 17 caracteres para o nome dos target groups caber no limite de 32 da AWS."
  }
}

variable "vpc_id" {
  description = "VPC onde ficam o load balancer e os target groups"
  type        = string
}

variable "public_subnet_ids" {
  description = "Subnets públicas, em ao menos duas zonas, do load balancer"
  type        = list(string)

  validation {
    condition     = length(var.public_subnet_ids) >= 2
    error_message = "O load balancer exige ao menos duas zonas de disponibilidade."
  }
}

variable "certificate_arn" {
  description = "Certificado ACM do listener HTTPS"
  type        = string
}

variable "domain_name" {
  description = "Hostname público, alvo do alias do Route 53"
  type        = string
}

variable "route53_zone_id" {
  description = "Zona do Route 53 do alias; null não cria o registro"
  type        = string
  default     = null
}

variable "tags" {
  description = "Tags aplicadas ao load balancer, aos target groups, aos listeners e ao security group"
  type        = map(string)
  default     = {}
}
