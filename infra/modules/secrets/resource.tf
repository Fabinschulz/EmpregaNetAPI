resource "aws_secretsmanager_secret" "this" {
  name        = "empreganet/${var.environment}/app"
  description = "Segredos de runtime da API e do frontend da EmpregaNet (valores preenchidos fora do Terraform)"

  # Um segredo apagado por engano pode ser recuperado dentro da janela.
  recovery_window_in_days = 7

  tags = var.tags
}
