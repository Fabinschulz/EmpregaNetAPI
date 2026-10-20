variable "aws_region" {
  description = "Região da AWS onde o bucket de state é criado"
  type        = string
  default     = "sa-east-1"
}

variable "aws_profile" {
  description = "Profile da AWS CLI usado para autenticar; null usa as credenciais do ambiente"
  type        = string
  default     = null
}

variable "aws_account_id" {
  description = "Conta AWS dedicada; o provider recusa qualquer outra"
  type        = string

  validation {
    condition     = can(regex("^[0-9]{12}$", var.aws_account_id))
    error_message = "O ID da conta deve ter 12 dígitos."
  }
}
