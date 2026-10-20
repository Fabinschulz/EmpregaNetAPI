terraform {
  required_version = ">= 1.11"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "6.66.0"
    }
  }

  # Backend parcial: o bucket e a região dependem da conta, que ainda não existe. Informe-os com
  # terraform init -backend-config=backend.hcl (modelo em backend.hcl.example).
  # O state de cada workspace fica em empreganet/<workspace>/terraform.tfstate.
  backend "s3" {
    key                  = "terraform.tfstate"
    workspace_key_prefix = "empreganet"
    encrypt              = true
    use_lockfile         = true
  }
}

provider "aws" {
  region              = var.aws_region
  profile             = var.aws_profile
  allowed_account_ids = [var.aws_account_id]
}
