using System.ComponentModel.DataAnnotations;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    /// <summary>
    /// Validation para data de nascimento.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class BirthDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success!;

            string dateString = Convert.ToDateTime(value.ToString()).ToString("yyyy-MM-ddTHH:mm:sszzz");
            DateTimeOffset.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:sszzz",
                                         System.Globalization.CultureInfo.InvariantCulture,
                                         System.Globalization.DateTimeStyles.None,
                                         out DateTimeOffset birthDate);

            if (birthDate.Date >= DateTimeOffset.Now.Date)
                return new ValidationResult("Data de nascimento não pode ser maior ou igual a data atual.", new string[] { validationContext.MemberName ?? string.Empty });
            if (birthDate.Date <= DateTimeOffset.Now.AddYears(-150).Date)
                return new ValidationResult("Data de nascimento inválida.", new string[] { validationContext.MemberName ?? string.Empty });

            return ValidationResult.Success!;
        }
    }
}
