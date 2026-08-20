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

        /// <summary>Colapsa qualquer sequência de espaços em branco num único espaço e apara as bordas.</summary>
        public static string CollapseWhitespace(this string? stIn)
            => string.IsNullOrWhiteSpace(stIn)
                ? string.Empty
                : string.Join(' ', stIn.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Recorta até <paramref name="maxLength"/> caracteres na última palavra inteira, marcando o
        /// corte com reticências.
        /// </summary>
        /// <param name="alreadyTruncated">
        /// Indica que o texto recebido já vinha cortado. Marca o corte mesmo quando o texto cabe no
        /// limite, para a última palavra não ficar partida sem aviso.
        /// </param>
        public static string ToExcerpt(this string? stIn, int maxLength, bool alreadyTruncated = false)
        {
            var text = stIn.CollapseWhitespace();

            if (text.Length == 0)
            {
                return string.Empty;
            }

            if (text.Length <= maxLength)
            {
                return alreadyTruncated ? CutAtWord(text, text.Length) : text;
            }

            return CutAtWord(text, maxLength);
        }

        private static string CutAtWord(string text, int limit)
        {
            var lastSpace = text.LastIndexOf(' ', limit - 1);
            var keepWholeWords = lastSpace > limit / 2;
            var head = keepWholeWords ? text[..lastSpace] : text[..(limit - 1)];

            return head.TrimEnd(' ', ',', ';', '.', '-') + Ellipsis;
        }

        private const string Ellipsis = "…";

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NonDigits();
    }
}
