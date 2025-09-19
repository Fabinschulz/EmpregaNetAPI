using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    /// <summary>
    /// Validation para CEP.
    /// </summary>
    public class IsZipCodeAttribute : ValidationAttribute
    {

        public override bool IsValid(object? value)
        {
            var zipCode = value as string;

            if (string.IsNullOrEmpty(zipCode))
                return false;

            var soNumero = Regex.Replace(zipCode, "[^0-9]", string.Empty);

            if (soNumero.Length != 8)
                return false;
            if (soNumero == "00000000"){
                return false;
            }
            return true;
        }
    }
}
