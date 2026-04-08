using FluentValidation;
using System.Text.RegularExpressions;
using EmpregaNet.Application.Utils.Helpers;

namespace EmpregaNet.Application.Utils
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> IsBrazilianCellPhone<T>(
        this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(BeAValidBrazilianCellPhone)
                .WithMessage("Celular inválido.");
        }

        private static bool BeAValidBrazilianCellPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var numbers = phone.OnlyNumbers();

            // Adiciona DDI se não tiver
            if (!numbers.StartsWith("55"))
                numbers = "55" + numbers;

            // Deve ter exatamente 13 dígitos: 55 + DDD + 9 dígitos
            if (numbers.Length != 13)
                return false;

            // Regex: 55 + DDD + 9 + 8 dígitos
            var regex = new Regex(@"^55\d{2}9\d{8}$");

            // Evita números tipo 99999999999
            if (AllDigitsEqual(numbers))
                return false;

            return regex.IsMatch(numbers);
        }

        private static bool AllDigitsEqual(string value)
        {
            return value.All(c => c == value[0]);
        }
    }
}