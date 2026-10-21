output "vpc_id" {
  description = "ID da VPC"
  value       = aws_vpc.this.id
}

output "public_subnet_ids" {
  description = "Subnets públicas (ALB e tarefas), uma por zona"
  value       = aws_subnet.public[*].id
}

output "public_subnet_cidrs" {
  description = "CIDRs das subnets públicas: a rede do balanceador que a API confia em ForwardedHeaders:KnownNetworks"
  value       = aws_subnet.public[*].cidr_block
}

output "private_subnet_ids" {
  description = "Subnets privadas isoladas (RDS), uma por zona"
  value       = aws_subnet.private[*].id
}
