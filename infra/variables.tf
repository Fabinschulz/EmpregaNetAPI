variable "aws_region" {
  description = "Região da AWS onde os recursos são criados"
  type        = string
  default     = "sa-east-1"
}

variable "aws_profile" {
  description = "Profile da AWS CLI usado para autenticar; null usa as credenciais do ambiente (por exemplo, o papel OIDC da CI)"
  type        = string
  default     = null
}

variable "aws_account_id" {
  description = "Conta AWS dedicada ao ambiente; o provider recusa qualquer outra"
  type        = string

  validation {
    condition     = can(regex("^[0-9]{12}$", var.aws_account_id))
    error_message = "O ID da conta deve ter 12 dígitos."
  }
}

variable "vpc_cidr" {
  description = "CIDR da VPC dedicada (/16); as subnets são derivadas dele em /24"
  type        = string
  default     = "10.20.0.0/16"

  validation {
    condition     = can(cidrhost(var.vpc_cidr, 0)) && endswith(var.vpc_cidr, "/16")
    error_message = "Informe um CIDR /16 válido (ex.: 10.20.0.0/16)."
  }
}

variable "domain_name" {
  description = "Hostname público único do frontend e da API (mesma origem: o cookie de autenticação é host-only)"
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9]([a-z0-9.-]*[a-z0-9])?$", var.domain_name)) && !can(regex("^https?://", var.domain_name))
    error_message = "Informe só o hostname, em minúsculas e sem esquema (ex.: homolog.exemplo.gov.br)."
  }
}

variable "certificate_arn" {
  description = "ARN do certificado ACM do hostname, emitido na mesma região do load balancer"
  type        = string

  validation {
    condition     = can(regex("^arn:aws:acm:[a-z0-9-]+:[0-9]{12}:certificate/", var.certificate_arn))
    error_message = "O certificado deve ser um ARN do ACM (arn:aws:acm:<região>:<conta>:certificate/<id>)."
  }
}

variable "route53_zone_id" {
  description = "Zona do Route 53 onde criar o alias do hostname para o load balancer; null deixa o DNS para quem o gerencia fora desta conta"
  type        = string
  default     = null
}

variable "image_tag" {
  description = "SHA do commit (40 caracteres hexadecimais) das imagens das task definitions iniciais; só vale para o primeiro apply, porque depois a CI é dona da revisão"
  type        = string

  validation {
    condition     = can(regex("^[0-9a-f]{40}$", var.image_tag))
    error_message = "A tag deve ser o SHA completo do commit: 40 caracteres hexadecimais minúsculos (não use latest)."
  }
}

variable "aspnetcore_environment" {
  description = "Valor de ASPNETCORE_ENVIRONMENT. Production exige o certificado do Data Protection (ADR 0019); Staging só avisa"
  type        = string
  default     = "Staging"

  validation {
    condition     = contains(["Staging", "Production"], var.aspnetcore_environment)
    error_message = "Use Staging ou Production."
  }
}

variable "service_capacity" {
  description = "Tamanho de cada tarefa (cpu em unidades, memória em MiB) e limites do Auto Scaling na janela de operação, por serviço"
  type = map(object({
    cpu    = number
    memory = number
    min    = number
    max    = number
  }))
  default = {
    api = { cpu = 512, memory = 1024, min = 1, max = 2 }
    web = { cpu = 512, memory = 1024, min = 1, max = 2 }
  }

  validation {
    condition     = alltrue([for name in ["api", "web"] : contains(keys(var.service_capacity), name)])
    error_message = "Defina api e web."
  }

  validation {
    condition     = alltrue([for s in values(var.service_capacity) : s.min >= 1 && s.max >= s.min])
    error_message = "Em cada serviço, min >= 1 e max >= min (o zero da madrugada vem da agenda)."
  }
}

variable "ses_identity" {
  description = "Identidade verificada no SES (e-mail ou domínio) usada como remetente; null desliga o envio e a permissão ses:SendEmail"
  type        = string
  default     = null
}

variable "ses_from_email" {
  description = "Endereço remetente dos e-mails; null usa ses_identity quando ela é um e-mail. Com uma identidade de domínio, informe o endereço"
  type        = string
  default     = null
}

variable "db_master_password" {
  description = "Senha do usuário mestre do RDS (a mesma que vai na connection string do segredo da aplicação). Efêmera e write-only: não entra no state nem no plan; só é preciso informá-la na criação e quando db_master_password_version aumenta. Use TF_VAR_db_master_password"
  type        = string
  default     = null
  ephemeral   = true
  sensitive   = true
}

variable "db_master_password_version" {
  description = "Aumente para enviar de novo a senha ao RDS (troca de senha)"
  type        = number
  default     = 1
}

variable "db_instance_class" {
  description = "Classe da instância RDS"
  type        = string
  default     = "db.t4g.micro"
}

variable "db_allocated_storage" {
  description = "Armazenamento do RDS em GB (gp3)"
  type        = number
  default     = 20
}

variable "db_deletion_protection" {
  description = "Protege a instância contra exclusão; desligue só para desmontar um ambiente descartável"
  type        = bool
  default     = true
}

variable "schedule_enabled" {
  description = "Liga a agenda de operação (07h-00h): sem ela as tarefas e o RDS ficam 24 horas"
  type        = bool
  default     = true
}

variable "alarm_email" {
  description = "E-mail que recebe os alarmes e os avisos do orçamento; a AWS pede confirmação da inscrição"
  type        = string
}

variable "budget_limit_usd" {
  description = "Teto mensal do orçamento em dólares (o AWS Budgets não aceita real). Padrão: R$ 1.000 pela PTAX de 15/10/2026 (5,0521)"
  type        = number
  default     = 198
}

variable "github_repository" {
  description = "Repositório do GitHub (dono/nome) cujo workflow na branch master publica as imagens e faz o deploy"
  type        = string
  default     = "Fabinschulz/EmpregaNetAPI"
}

variable "github_repository_ids" {
  description = "IDs numéricos do dono e do repositório (gh api repos/<dono>/<nome> --jq '.owner.id, .id'), exigidos pelo subject imutável do token OIDC"
  type = object({
    owner_id      = number
    repository_id = number
  })
  default = {
    owner_id      = 77129076
    repository_id = 942977763
  }
}

variable "github_oidc_provider_arn" {
  description = "ARN do provedor OIDC do GitHub já existente na conta; com null o Terraform cria o provedor (só existe um por conta)"
  type        = string
  default     = null
}
