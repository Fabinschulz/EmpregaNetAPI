variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "alarm_email" {
  description = "E-mail inscrito no tópico; a AWS envia um pedido de confirmação e nada é entregue até o destinatário confirmar"
  type        = string
}

variable "tags" {
  description = "Tags aplicadas ao tópico"
  type        = map(string)
  default     = {}
}
