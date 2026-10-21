variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "repository_arns" {
  description = "ARN dos repositórios ECR por serviço (api, web), escopo das permissões de pull e push"
  type        = map(string)
}

variable "secret_arn" {
  description = "ARN do segredo de runtime que a execution role lê ao iniciar a task"
  type        = string
}

variable "ses_identity" {
  description = "Identidade verificada no SES (e-mail ou domínio); null não concede ses:SendEmail"
  type        = string
  default     = null
}

variable "github_repository" {
  description = "Repositório do GitHub (dono/nome) cujo workflow na branch master assume o papel do CI"
  type        = string
}

variable "github_repository_ids" {
  description = "IDs numéricos do dono e do repositório no GitHub, exigidos pelo subject imutável do token OIDC"
  type = object({
    owner_id      = number
    repository_id = number
  })
}

variable "github_oidc_provider_arn" {
  description = "ARN do provedor OIDC do GitHub já existente na conta; null cria o provedor"
  type        = string
  default     = null
}

variable "tags" {
  description = "Tags aplicadas às roles e ao provedor"
  type        = map(string)
  default     = {}
}
