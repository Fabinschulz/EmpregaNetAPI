using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EmpregaNet.Application.Utils.CustomValidation
{
    /// <summary>
    /// Validation para linha digitável.
    /// </summary>
    public class IsBarCodeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var _barcode = value as string; 
            if (string.IsNullOrEmpty(_barcode))
            {
                return true;
            }
            string barcode = new Regex(@"[^\d]").Replace(_barcode, "");

            if (barcode.Length != 44 && barcode.Length != 47)
            {
                return false;
            }

            return true;
        }

        public int module_bank(string num)
        {
            num = new Regex(@"[^\d]").Replace(num, "");
            int sum = 0;
            int weight = 2;
            int base1 = 9;
            int count = num.Length - 1;
            for (int i = count; i >= 0; i--)
            {
                sum = sum + int.Parse(num.Substring(i, 1)) * weight;
                if (weight < base1)
                {
                    weight++;
                }
                else
                {
                    weight = 2;
                }
            }
            int digit = 11 - sum % 11;
            if (digit > 9) digit = 0;
            if (digit == 0) digit = 1;
            return digit;
        }
    }
}

