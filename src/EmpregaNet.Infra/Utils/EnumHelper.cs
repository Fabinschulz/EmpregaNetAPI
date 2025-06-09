using System.ComponentModel;
using System.Reflection;

namespace EmpregaNet.Infra.Utils
{
    public static class EnumHelper
    {
        public static string GetEnumDescription<T>(this T enumValue) where T : Enum
        {
            try
            {
                FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString())!;
                if (fieldInfo != null)
                {
                    var attr = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
                    if (attr != null && attr.Any())
                    {
                        return attr.First().Description;
                    }
                }

                return enumValue.ToString();
            }
            catch
            {
                return string.Empty;
            }

        }
    }
}