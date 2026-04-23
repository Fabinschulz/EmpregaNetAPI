using System.ComponentModel.DataAnnotations;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    /// <summary>
    /// Validation para telefone.
    /// </summary>
    public class IsPhoneAttribute : ValidationAttribute
    {

        public override bool IsValid(object? value)
        {
            if (value is string phone && !string.IsNullOrEmpty(phone))
            {
                int phoneLength = phone.Length;
                return phoneLength >= 13 && phoneLength <= 14;
            }
            return false;
        }
    }
}
