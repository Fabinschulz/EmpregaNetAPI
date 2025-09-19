using System.ComponentModel.DataAnnotations;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    /// <summary>
    /// Validation para email.
    /// </summary>
    public class IsEmailAttribute : ValidationAttribute
    {

        public override bool IsValid(object? value)
        {
            try
            {
                if (value == null) return false;
                string strValue = (string)value;
                var addr = new System.Net.Mail.MailAddress(strValue);
                return addr.Address == strValue;
            }
            catch
            {
                return false;
            }
        }
    }
}
