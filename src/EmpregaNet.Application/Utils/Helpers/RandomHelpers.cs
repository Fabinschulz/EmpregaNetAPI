using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace EmpregaNet.Application.Utils.Helpers
{
    public static class RandomHelpers
    {
        static readonly HttpClient client = new HttpClient();

        public class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        /// <summary>
        /// Obtém a exceção mais interna de uma exceção aninhada.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>
        /// A exceção mais interna.
        /// </returns>
        public static Exception GetInnerException(Exception e)
        {
            while (e.InnerException != null) e = e.InnerException;
            return e;
        }

        /// <summary>
        /// Converte graus em radianos.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns>
        /// O valor em radianos.
        /// </returns>
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Valida se o número do CPF ou CNPJ é válido.
        /// </summary>
        /// <param name="documentNumber"></param>
        /// <returns>
        /// true se o número do CPF ou CNPJ for válido; caso contrário, false.
        /// </returns>
        public static bool ValidateCPFCNPJ(string documentNumber)
        {
            var cpfcnpj = documentNumber;

            if (string.IsNullOrEmpty(cpfcnpj))
                return false;
            var d = new int[14];
            var v = new int[2];
            int j, i, soma;

            var soNumero = Regex.Replace(cpfcnpj, "[^0-9]", string.Empty);
            if (new string(soNumero[0], soNumero.Length) == soNumero) return false;

            switch (soNumero.Length)
            {
                case 11:
                    for (i = 0; i <= 10; i++) d[i] = Convert.ToInt32(soNumero.Substring(i, 1));
                    for (i = 0; i <= 1; i++)
                    {
                        soma = 0;
                        for (j = 0; j <= 8 + i; j++) soma += d[j] * (10 + i - j);

                        v[i] = (soma * 10) % 11;
                        if (v[i] == 10) v[i] = 0;
                    }
                    return (v[0] == d[9] & v[1] == d[10]);
                case 14:
                    const string sequencia = "6543298765432";
                    for (i = 0; i <= 13; i++) d[i] = Convert.ToInt32(soNumero.Substring(i, 1));
                    for (i = 0; i <= 1; i++)
                    {
                        soma = 0;
                        for (j = 0; j <= 11 + i; j++)
                            soma += d[j] * Convert.ToInt32(sequencia.Substring(j + 1 - i, 1));

                        v[i] = (soma * 10) % 11;
                        if (v[i] == 10) v[i] = 0;
                    }
                    return (v[0] == d[12] & v[1] == d[13]);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Calcula a idade com base na data de nascimento fornecida.
        /// Retorna a idade em anos completos.
        /// </summary>
        /// <param name="bornDate"></param>
        /// <returns>
        /// A idade em anos completos.
        /// </returns>
        public static int GetAge(DateTime bornDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - bornDate.Year;
            if (bornDate > today.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// Obtém a data e hora atual no fuso horário de Brasília (America/Sao_Paulo).
        /// </summary>
        /// <returns>
        /// O valor DateTimeOffset atual no fuso horário de Brasília.
        /// </returns>
        public static DateTimeOffset GetBrasiliaDateTime()
        {
            TimeZoneInfo timeZone;
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timeZone);
        }

        /// <summary>
        /// Converte um DateTimeOffset (que deve ser UTC) para o fuso horário de Brasília (America/Sao_Paulo)
        /// e o retorna como uma string formatada (dd/MM/yyyy HH:mm:ss).
        /// </summary>
        /// <param name="utcDate">O DateTimeOffset a ser convertido (preferencialmente em UTC).</param>
        /// <param name="format">O formato de string desejado (padrão é "dd/MM/yyyy HH:mm:ss").</param>
        /// <returns>A string formatada com a data e hora local do Brasil.</returns>
        public static string FormatToBrasiliaTime(DateTimeOffset? utcDate, string format = "dd/MM/yyyy HH:mm:ss")
        {
            if (!utcDate.HasValue)
            {
                return string.Empty;
            }

            TimeZoneInfo timeZone;
            try
            {
                // Tenta o ID mais robusto para a maioria dos sistemas
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (TimeZoneNotFoundException)
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }

            DateTimeOffset brasiliaTime = TimeZoneInfo.ConvertTime(utcDate.Value, timeZone);
            return brasiliaTime.ToString(format, CultureInfo.GetCultureInfo("pt-BR"));
        }

        /// <summary>
        /// Converte um DateTimeOffset para o fuso horário de Brasília (America/Sao_Paulo).
        /// </summary>
        /// <param name="date"></param>
        /// <returns>
        /// O valor DateTimeOffset convertido para o fuso horário de Brasília.
        /// </returns>
        /// <remarks> 
        /// Se a conversão falhar, retorna o valor original.
        /// </remarks>
        public static DateTimeOffset ConvertBrasiliaDateTime(DateTimeOffset date)
        {
            try
            {
                TimeZoneInfo timeZone;
                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                }
                catch
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                }
                return TimeZoneInfo.ConvertTime(date, timeZone);
            }
            catch
            {
                return date;
            }
        }


        /// <summary>
        /// Converte um DateTimeOffset para um formato que pode ser comparado.
        /// </summary>
        /// <param name="date"></param>
        /// <returns>
        /// O valor DateTimeOffset convertido para UTC.
        /// </returns>
        public static DateTimeOffset ConvertToCompareDate(DateTimeOffset date)
        {
            try
            {
                TimeZoneInfo timeZone;
                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                }
                catch
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                }
                var brazilDate = new DateTimeOffset(date.DateTime, new TimeSpan(timeZone.BaseUtcOffset.Hours, 0, 0));
                return brazilDate.ToUniversalTime();
            }
            catch
            {
                return date;
            }
        }

        /// <summary>
        /// Converte uma string para DateTimeOffset, retornando null se a conversão falhar.
        /// </summary>
        /// <param name="date"></param>
        /// <returns>
        /// O valor DateTimeOffset convertido, ou null se a conversão falhar.
        /// </returns>
        public static DateTimeOffset? StringToDateTimeOffset(string date)
        {
            DateTimeOffset converted;
            if (DateTimeOffset.TryParse(date, out converted))
            {
                return converted;
            }
            return null;
        }

        /// <summary>
        /// Converte uma string para int, retornando null se a conversão falhar.
        /// </summary>
        /// <param name="integer"></param>
        /// <returns>
        /// O valor int convertido, ou null se a conversão falhar.
        /// </returns>
        public static int? StringToInt(string integer)
        {
            int converted;
            if (int.TryParse(integer, out converted))
            {
                return converted;
            }
            return null;
        }

        /// <summary>
        /// Converte uma string para long, retornando null se a conversão falhar.
        /// </summary>
        /// <param name="integer"></param>
        /// <returns>
        /// O valor long convertido, ou null se a conversão falhar.
        /// </returns>
        public static long? StringToLong(string integer)
        {
            long converted;
            if (long.TryParse(integer, out converted))
            {
                return converted;
            }
            return null;
        }

        /// <summary>
        /// Obtém a latitude e longitude de um endereço usando a API do Google Maps.
        /// </summary>
        /// <param name="street"></param>
        /// <param name="number"></param>
        /// <param name="cityName"></param>
        /// <param name="stateAbrev"></param>
        /// <param name="key"></param>
        /// <returns>
        /// Um objeto Location contendo a latitude e longitude, ou null se não for possível obter a localização.
        /// </returns>
        public static Location? GetLatLng(string street, string number, string cityName, string stateAbrev, string key)
        {

            try
            {
                var url =
                    $"https://maps.googleapis.com/maps/api/geocode/json?address={street}, {number}, {cityName} - {stateAbrev}&key={key}";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync();
                var contentResult = content.Result;
                if (string.IsNullOrEmpty(contentResult))
                    return null;

                dynamic? obj = string.IsNullOrEmpty(contentResult) ? null : JsonConvert.DeserializeObject(contentResult);
                if (obj == null)
                    return null;

                string test = obj.ToString();
                if (!test.Contains("UNKNOWN") && !test.Contains("ZERO_RESULTS"))
                {
                    dynamic location = obj.results[0].geometry.location;

                    return new Location
                    {
                        Latitude = location.lat,
                        Longitude = location.lng
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// Gera um arquivo CSV local a partir de uma coleção de itens do tipo T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="path"></param>
        public static void WriteCSVLocal<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine(string.Join("; ", props.Select(p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join("; ", props.Select(p => p.GetValue(item, null))));
                }
            }
        }

        /// <summary>
        /// Gera um arquivo CSV em memória a partir de uma coleção de itens do tipo T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="separator"></param>
        /// <returns>
        /// Um MemoryStream contendo o arquivo CSV gerado.
        /// </returns>
        public static MemoryStream WriteCSV<T>(IEnumerable<T> items, string separator)
        {
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            using (var writer = new StringWriter())
            {
                writer.WriteLine(string.Join("; ", props.Select(p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join("; ", props.Select(p => p.GetValue(item, null))));
                }

                var stream = new MemoryStream();
                using (StreamWriter writer2 = new StreamWriter(stream))
                {
                    writer2.Write(writer);
                    writer2.Flush();
                    stream.Position = 0;
                    return stream;
                }
            }

        }

        /// <summary>
        /// Remove acentos de uma string.
        /// </summary>
        /// <param name="withAccents"></param>
        /// <returns>A string sem acentos.</returns>
        public static string RemoveAccents(string withAccents)
        {
            if (!string.IsNullOrEmpty(withAccents))
                return new string(withAccents
                    .Normalize(NormalizationForm.FormD)
                    .Where(ch => char.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    .ToArray());
            return "";
        }

        /// <summary>
        /// Gera um número decimal aleatório dentro do intervalo especificado.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>
        /// O número decimal aleatório gerado.
        /// </returns>
        public static decimal RandomDecimal(decimal min, decimal max)
        {
            var rnd = new Random();
            decimal d = (decimal)(rnd.NextDouble()) * (max - min) + min;
            return decimal.Round(d, 2);
        }

        /// <summary>
        /// Gera uma string aleatória de 8 caracteres, composta por consoantes e dígitos.
        /// </summary>
        /// <returns>A string aleatória gerada.</returns>
        public static string RandomStringWithoutVowels()
        {
            const string chars = "BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        /// <summary>
        /// Gera uma string aleatória de tamanho especificado, composta por letras maiúsculas, minúsculas e dígitos.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>
        /// A string aleatória gerada.
        /// </returns>
        public static string RandomString(int size)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[size];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        /// <summary>
        /// Calcula a distância em metros entre dois pontos geográficos usando a fórmula de Haversine.
        /// </summary>
        /// <param name="initialLatitude"></param>
        /// <param name="initialLongitude"></param>
        /// <param name="finalLatitude"></param>
        /// <param name="finalLongitude"></param>
        /// <returns>
        /// A distância em metros entre os dois pontos.
        /// </returns>
        public static double CalculateDistance(double initialLatitude, double initialLongitude,
            double finalLatitude, double finalLongitude)
        {
            var d1 = initialLatitude * (Math.PI / 180.0);
            var num1 = initialLongitude * (Math.PI / 180.0);
            var d2 = finalLatitude * (Math.PI / 180.0);
            var num2 = finalLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                    Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}
