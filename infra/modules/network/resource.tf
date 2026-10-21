locals {
  name_prefix = "empreganet-${var.environment}"

  # Duas zonas: o ALB exige duas e o grupo de subnets do RDS também.
  azs = slice(data.aws_availability_zones.available.names, 0, 2)

  public_cidrs  = [for i, _ in local.azs : cidrsubnet(var.vpc_cidr, 8, i)]
  private_cidrs = [for i, _ in local.azs : cidrsubnet(var.vpc_cidr, 8, 10 + i)]
}

data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_region" "current" {}

resource "aws_vpc" "this" {
  cidr_block           = var.vpc_cidr
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = merge(var.tags, { Name = local.name_prefix })
}

resource "aws_internet_gateway" "this" {
  vpc_id = aws_vpc.this.id
  tags   = merge(var.tags, { Name = local.name_prefix })
}

# ALB e tarefas. Sem NAT Gateway: as tarefas recebem IP público e a entrada é fechada pelos security groups.
resource "aws_subnet" "public" {
  count = length(local.azs)

  vpc_id                  = aws_vpc.this.id
  cidr_block              = local.public_cidrs[count.index]
  availability_zone       = local.azs[count.index]
  map_public_ip_on_launch = false

  tags = merge(var.tags, { Name = "${local.name_prefix}-public-${local.azs[count.index]}" })
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.this.id
  tags   = merge(var.tags, { Name = "${local.name_prefix}-public" })
}

resource "aws_route" "public_internet" {
  route_table_id         = aws_route_table.public.id
  destination_cidr_block = "0.0.0.0/0"
  gateway_id             = aws_internet_gateway.this.id
}

resource "aws_route_table_association" "public" {
  count = length(aws_subnet.public)

  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

# Só o RDS. A tabela de rotas não tem rota para o internet gateway (nem NAT): o banco é inalcançável de fora.
resource "aws_subnet" "private" {
  count = length(local.azs)

  vpc_id            = aws_vpc.this.id
  cidr_block        = local.private_cidrs[count.index]
  availability_zone = local.azs[count.index]

  tags = merge(var.tags, { Name = "${local.name_prefix}-private-${local.azs[count.index]}" })
}

resource "aws_route_table" "private" {
  vpc_id = aws_vpc.this.id
  tags   = merge(var.tags, { Name = "${local.name_prefix}-private" })
}

resource "aws_route_table_association" "private" {
  count = length(aws_subnet.private)

  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = aws_route_table.private.id
}

# Gateway endpoint, sem custo: o tráfego das tarefas para o S3 (camadas das imagens do ECR) não sai pela internet.
resource "aws_vpc_endpoint" "s3" {
  vpc_id            = aws_vpc.this.id
  service_name      = "com.amazonaws.${data.aws_region.current.region}.s3"
  vpc_endpoint_type = "Gateway"
  route_table_ids   = [aws_route_table.public.id, aws_route_table.private.id]

  tags = merge(var.tags, { Name = "${local.name_prefix}-s3" })
}
