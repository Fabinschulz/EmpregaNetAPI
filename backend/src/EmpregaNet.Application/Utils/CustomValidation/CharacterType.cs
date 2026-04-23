using System.ComponentModel;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    public enum CharacterType
    {

        /// <summary>
        /// Campo do tipo Numérico
        /// </summary>
        [Description("Campo do tipo Numérico")]
        Numeric = 1,

        /// <summary>
        /// Campo do tipo Alfanumérico
        /// </summary>
        [Description("Campo do tipo Alfanumérico")]
        Alfanumeric = 2,

    }
}