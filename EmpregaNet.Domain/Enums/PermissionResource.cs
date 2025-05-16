using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    public enum PermissionResourceEnum
    {
        [Description("Usuario")] User,
        [Description("Vaga")] Job,
        [Description("Candidatura")] Application,
        [Description("Curriculo")] Resume,
        [Description("Empresa")] Company,
        [Description("Relatorio")] Report,
        [Description("Configuracao")] Configuration,
        [Description("Dashboard")] Dashboard,
        [Description("Notificacao")] Notification,
        [Description("Permissao")] Permission,
        
    }
}