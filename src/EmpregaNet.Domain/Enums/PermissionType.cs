using System.ComponentModel;

namespace ESM0028.src.Domain.Enum
{
    public enum PermissionTypeEnum
    {
        [Description("Permissão de Leitura")] Read,
        [Description("Permissão de Escrita")] Create,
        [Description("Permissão de Atualização")] Update,
        [Description("Permissão de Exclusão")] Delete,
    }
}