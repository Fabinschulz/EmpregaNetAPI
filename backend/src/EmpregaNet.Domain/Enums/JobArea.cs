using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Área profissional da vaga.
    /// </summary>
    public enum JobAreaEnum
    {
        [Description("")] NaoSelecionado = 0,
        [Description("Produção")] Producao = 1,
        [Description("Logística")] Logistica = 2,
        [Description("Almoxarifado")] Almoxarifado = 3,
        [Description("Transporte")] Transporte = 4,
        [Description("Manutenção")] Manutencao = 5,
        [Description("Qualidade")] Qualidade = 6,
        [Description("Segurança do Trabalho")] SegurancaTrabalho = 7,
        [Description("Serviços Gerais")] ServicosGerais = 8,
        [Description("Administrativo")] Administrativo = 9,
        [Description("Comercial")] Comercial = 10,
        [Description("Recursos Humanos")] RecursosHumanos = 11,
        [Description("Financeiro")] Financeiro = 12,
        [Description("Tecnologia da Informação")] Ti = 13,
        [Description("Saúde")] Saude = 14,
        [Description("Educação")] Educacao = 15,
        [Description("Outras")] Outras = 16
    }
}
