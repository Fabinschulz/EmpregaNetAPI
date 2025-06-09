using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    public enum PermissionTypeEnum
    {
        [Description("Permissão de Leitura")] Read,
        [Description("Permissão de Escrita")] Create,
        [Description("Permissão de Atualização")] Update,
        [Description("Permissão de Exclusão")] Delete,
    }
}