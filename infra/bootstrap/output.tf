output "bucket_state" {
  description = "Bucket do state, para o backend.hcl da raiz"
  value       = aws_s3_bucket.tf_state.bucket
}
