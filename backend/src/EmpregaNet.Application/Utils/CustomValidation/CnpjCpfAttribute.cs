using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    /// <summary>
    /// Validation para cnpj.
    /// </summary>
    public class IsCnpjAttribute : ValidationAttribute
    {

        public override bool IsValid(object? value)
        {
            var cnpj = value as string;

            if (string.IsNullOrEmpty(cnpj)) return true;
            var d = new int[14];
            var v = new int[2];
            int j, i, soma;

            var soNumero = Regex.Replace(cnpj, "[^0-9]", string.Empty);
            if (new string(soNumero[0], soNumero.Length) == soNumero) return false;

            if (soNumero.Length == 14)
            {
                const string sequencia = "6543298765432";
                for (i = 0; i <= 13; i++) d[i] = Convert.ToInt32(soNumero.Substring(i, 1));
                for (i = 0; i <= 1; i++)
                {
                    soma = 0;
                    for (j = 0; j <= 11 + i; j++)
                        soma += d[j] * Convert.ToInt32(sequencia.Substring(j + 1 - i, 1));

                    v[i] = soma * 10 % 11;
                    if (v[i] == 10) v[i] = 0;
                }
                return v[0] == d[12] & v[1] == d[13];
            }
            return false;
        }
    }

    /// <summary>
    /// Validation para cpf.
    /// </summary>
    public class IsCpfAttribute : ValidationAttribute
    {

        public override bool IsValid(object? value)
        {
            var cpf = value as string;

            if (string.IsNullOrEmpty(cpf))
                return true;
            var d = new int[14];
            var v = new int[2];
            int j, i, soma;

            var soNumero = Regex.Replace(cpf, "[^0-9]", string.Empty);
            if (new string(soNumero[0], soNumero.Length) == soNumero) return false;

            if (soNumero.Length == 11)
            {
                for (i = 0; i <= 10; i++) d[i] = Convert.ToInt32(soNumero.Substring(i, 1));
                for (i = 0; i <= 1; i++)
                {
                    soma = 0;
                    for (j = 0; j <= 8 + i; j++) soma += d[j] * (10 + i - j);

                    v[i] = soma * 10 % 11;
                    if (v[i] == 10) v[i] = 0;
                }
                return v[0] == d[9] & v[1] == d[10];
            }
            return false;
        }
    }
}
