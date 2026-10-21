variable "environment" {
  description = "Ambiente dos recursos (o workspace do Terraform na raiz), usado nos nomes e nas tags"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.environment))
    error_message = "O ambiente deve conter apenas letras minúsculas, dígitos e hífens (ex.: homolog)."
  }
}

variable "vpc_id" {
  description = "VPC onde ficam os security groups das tasks"
  type        = string
}

variable "subnet_ids" {
  description = "Subnets onde as tasks rodam (as públicas, com IP público em cada task, porque a VPC não tem NAT)"
  type        = list(string)
}

variable "alb_subnet_cidrs" {
  description = "CIDRs das subnets do load balancer: a única rede de onde a API confia em X-Forwarded-*"
  type        = list(string)
}

variable "alb_security_group_id" {
  description = "Security group do load balancer: origem da entrada das tasks, que recebe aqui a saída para elas"
  type        = string
}

variable "target_group_arns" {
  description = "ARN do target group de cada serviço (api, web)"
  type        = map(string)
}

variable "database_security_group" {
  description = "Security group do RDS: destino da saída 5432 da API, que recebe aqui a entrada"
  type        = string
}

variable "repository_urls" {
  description = "URL dos repositórios ECR por serviço (api, web)"
  type        = map(string)
}

variable "image_tag" {
  description = "Tag (SHA do commit) das imagens da task definition inicial; depois do primeiro deploy a CI é dona da revisão"
  type        = string
}

variable "execution_role_arn" {
  description = "Task execution role (puxa a imagem, grava logs e lê o segredo)"
  type        = string
}

variable "task_role_arns" {
  description = "Task role de cada serviço (api, web)"
  type        = map(string)
}

variable "secret_arn" {
  description = "ARN do segredo de runtime, de onde vêm as chaves injetadas como secrets"
  type        = string
}

variable "capacity" {
  description = "Tamanho de cada tarefa e limites do Auto Scaling, por serviço (api, web)"
  type = map(object({
    cpu    = number
    memory = number
    min    = number
    max    = number
  }))
}

variable "aspnetcore_environment" {
  description = "Valor de ASPNETCORE_ENVIRONMENT (Staging ou Production)"
  type        = string
}

variable "domain_name" {
  description = "Hostname público: base dos links de e-mail, origem CORS e URL que o frontend usa para chamar a API"
  type        = string
}

variable "ses_identity" {
  description = "Identidade verificada no SES; null desliga o envio de e-mail"
  type        = string
  default     = null
}

variable "ses_from_email" {
  description = "Endereço remetente (Ses__FromEmail); vazio quando o envio está desligado"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags aplicadas ao cluster, aos serviços, às task definitions, aos log groups e aos security groups"
  type        = map(string)
  default     = {}
}
