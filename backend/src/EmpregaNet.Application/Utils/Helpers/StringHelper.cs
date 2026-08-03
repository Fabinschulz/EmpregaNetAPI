using System.Text.RegularExpressions;

namespace EmpregaNet.Application.Utils.Helpers
{
    public static partial class StringHelper
    {
        /// <summary>
        /// Mantém apenas os dígitos da string, descartando máscara, espaços e separadores.
        /// </summary>
        /// <remarks>
        /// Para CPF/CNPJ use <see cref="CustomValidation.BrazilianDocument"/>: o CNPJ alfanumérico
        /// tem letras na forma canónica e seria mutilado aqui.
        /// </remarks>
        public static string OnlyNumbers(this string stIn)
            => string.IsNullOrEmpty(stIn) ? string.Empty : NonDigits().Replace(stIn, string.Empty);

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NonDigits();
    }
}
