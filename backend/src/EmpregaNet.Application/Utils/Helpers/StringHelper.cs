using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace EmpregaNet.Application.Utils.Helpers
{
    public static class StringHelper
    {

        public static string OnlyNumbers(this string stIn)
        {
            return string.IsNullOrEmpty(stIn) ? string.Empty : Regex.Replace(stIn, "[^0-9]", string.Empty);
        }

        public static string RemoveDiacritics(this string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string RemoveSpecialChars(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            var r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            return r.Replace(value, string.Empty);
        }

        public static string RemoveSpecialCharsExceptAccents(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            var r = new Regex("(?:[^a-z0-9 \\p{L}~^]|(?<=['\"])s)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            return r.Replace(value, string.Empty);
        }

        public static string NormalizeString(this string value)
        {
            string normalizedString = value.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        stringBuilder.Append(c);
                        break;
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                        stringBuilder.Append('-');
                        break;
                }
            }
            string result = stringBuilder.ToString();
            return string.Join("-", result.Split(new char[] { '-' }
                , StringSplitOptions.RemoveEmptyEntries));
        }

        public static string NullSafeToLower(this string s)
        {
            if (s == null)
            {
                s = string.Empty;
            }
            return s.ToLower();
        }

        public static string ToJson(this object s) => JsonConvert.SerializeObject(s, Formatting.Indented);

        public static string ToXml(this object obj)
        {
            if (obj == null)
            {
                return "";
            }
                

            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static string FormatCPF(this string sender)
        {
            string response = sender.Trim();
            if (response.Length == 11)
            {
                response = response.Insert(9, "-");
                response = response.Insert(6, ".");
                response = response.Insert(3, ".");
            }
            return response;
        }

        public static string FormatCNPJ(this string sender)
        {
            string response = sender.Trim();
            if (response.Length == 14)
            {
                response = response.Insert(12, "-");
                response = response.Insert(8, "/");
                response = response.Insert(5, ".");
                response = response.Insert(2, ".");
            }
            return response;
        }

        public static string RemoveSpecialCharsAndSpacesPhoneNumber(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (!value.StartsWith("+55"))
                    value = "+55" + value.RemoveSpecialChars();

                return value.Replace(" ", "");
            }
            return value;
        }
    }
}