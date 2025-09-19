using System.ComponentModel.DataAnnotations;
using EmpregaNet.Application.Utils.Helpers;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    /// <summary>
    /// Validation para número de endereço.
    /// </summary>
    public class IsInt32StringAttribute : ValidationAttribute
    {

        public override bool IsValid(object? value)
        {
            var stringNumber = (value as string)?.OnlyNumbers() ?? string.Empty;
            int number;

            if (string.IsNullOrEmpty(stringNumber))
                stringNumber = "0";

            if (int.TryParse(stringNumber, out number))
            {
                if (number >= 0 && number <= int.MaxValue)
                    return true;
            }

            return false;
        }
    }
}
