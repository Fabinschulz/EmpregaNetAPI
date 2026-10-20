locals {
  bucket_state = "empreganet-terraform-state-${var.aws_account_id}-${var.aws_region}"
}

# ------------------------- State Bucket -----------------------------
resource "aws_s3_bucket" "tf_state" {
  bucket = local.bucket_state

  tags = {
    Project     = "empreganet"
    Environment = "shared"
    ManagedBy   = "terraform"
    Repository  = "EmpregaNetAPI"
  }

  lifecycle {
    # Apagar este bucket apaga o state de todos os ambientes.
    prevent_destroy = true
  }
}

# Mantém versões anteriores do state para recuperar de uma escrita errada.
resource "aws_s3_bucket_versioning" "tf_state" {
  bucket = aws_s3_bucket.tf_state.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "tf_state" {
  bucket = aws_s3_bucket.tf_state.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "tf_state" {
  bucket = aws_s3_bucket.tf_state.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}
