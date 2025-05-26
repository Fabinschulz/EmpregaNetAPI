using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Enumeração que representa o estado civil de um usuário.
    /// </summary>
    public enum CivilStatusEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Casado")] Married,
        [Description("Viúvo")] Widowed,
        [Description("Separado")] Separated,
        [Description("Divorciado")] Divorced,
        [Description("Solteiro")] Single
    }
}