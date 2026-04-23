namespace EmpregaNet.Application.Utils.Helpers
{
    public class NumberToCurrencyTextHelper
    {
        public static string Transform(double valueInCents)
        {
            long rounded = (long)Math.Round(valueInCents);
            return Transform(rounded);
        }

        public static string Transform(long valueInCents)
        {
            var valor = (decimal)valueInCents / 100;

            if (valueInCents < 0){
                return $"Menos {Transform(-valueInCents)}";
            }

            if (valueInCents == 0)
            {
                return "Zero";
            }

            if (valor >= long.MaxValue)
                return "Valor não suportado pelo sistema.";
            else
            {
                string strValue = valor.ToString("000000000000000.00");
                string numberAsString = string.Empty;
                for (int i = 0; i <= 15; i += 3)
                {
                    numberAsString += NumberToTextHelper.WriteNumber(Convert.ToDecimal(strValue.Substring(i, 3)));
                    if (i == 0 & numberAsString != string.Empty)
                    {
                        if (Convert.ToInt32(strValue.Substring(0, 3)) == 1)
                            numberAsString += " trilhão" + (Convert.ToDecimal(strValue.Substring(3, 12)) > 0 ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValue.Substring(0, 3)) > 1)
                            numberAsString += " trilhões" + (Convert.ToDecimal(strValue.Substring(3, 12)) > 0 ? " e " : string.Empty);
                    }
                    else if (i == 3 & numberAsString != string.Empty)
                    {
                        if (Convert.ToInt32(strValue.Substring(3, 3)) == 1)
                            numberAsString += " bilhão" + (Convert.ToDecimal(strValue.Substring(6, 9)) > 0 ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValue.Substring(3, 3)) > 1)
                            numberAsString += " bilhões" + (Convert.ToDecimal(strValue.Substring(6, 9)) > 0 ? " e " : string.Empty);
                    }
                    else if (i == 6 & numberAsString != string.Empty)
                    {
                        if (Convert.ToInt32(strValue.Substring(6, 3)) == 1)
                            numberAsString += " milhão" + (Convert.ToDecimal(strValue.Substring(9, 6)) > 0 ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValue.Substring(6, 3)) > 1)
                            numberAsString += " milhões" + (Convert.ToDecimal(strValue.Substring(9, 6)) > 0 ? " e " : string.Empty);
                    }
                    else if (i == 9 & numberAsString != string.Empty)
                        if (Convert.ToInt32(strValue.Substring(9, 3)) > 0)
                            numberAsString += " mil" + (Convert.ToDecimal(strValue.Substring(12, 3)) > 0 ? " e " : string.Empty);
                    if (i == 12)
                    {
                        if (numberAsString.Length > 8)
                            if (numberAsString.Substring(numberAsString.Length - 6, 6) == "bilhão" | numberAsString.Substring(numberAsString.Length - 6, 6) == "milhão")
                                numberAsString += " de";
                            else
                                if (numberAsString.Substring(numberAsString.Length - 7, 7) == "bilhões" | numberAsString.Substring(numberAsString.Length - 7, 7) == "milhões"
                                    | numberAsString.Substring(numberAsString.Length - 8, 7) == "trilhões")
                                numberAsString += " de";
                            else
                                    if (numberAsString.Substring(numberAsString.Length - 8, 8) == "trilhões")
                                numberAsString += " de";
                        if (Convert.ToInt64(strValue.Substring(0, 15)) == 1)
                            numberAsString += " real";
                        else if (Convert.ToInt64(strValue.Substring(0, 15)) > 1)
                            numberAsString += " reais";
                        if (Convert.ToInt32(strValue.Substring(16, 2)) > 0 && numberAsString != string.Empty)
                            numberAsString += " e ";
                    }
                    if (i == 15)
                        if (Convert.ToInt32(strValue.Substring(16, 2)) == 1)
                            numberAsString += " centavo";
                        else if (Convert.ToInt32(strValue.Substring(16, 2)) > 1)
                            numberAsString += " centavos";
                }
                return numberAsString;
            }
        }
    }

    public class NumberToTextHelper
    {

        public static string Transform(decimal valor, bool percentage = false)
        {
            if (valor == 0)
            {
                return "Zero";
            }

            if (valor >= long.MaxValue)
                return "Valor não suportado pelo sistema.";
            else
            {
                string strValue = valor.ToString("000000000000000.000");
                string numberAsString = string.Empty;

                if (valor < 1)
                {
                    numberAsString += "zero virgula ";
                }


                for (int i = 0; i <= 15; i += 3)
                {

                    numberAsString += i < 15 ? WriteNumber(Convert.ToDecimal(strValue.Substring(i, 3))) : WriteNumber(Convert.ToDecimal(strValue.Substring(!percentage ? i : i + 1, 3)));


                    if (i == 0 & numberAsString != string.Empty)
                    {
                        if (Convert.ToInt32(strValue.Substring(0, 3)) == 1)
                            numberAsString += " trilhão" + (Convert.ToDecimal(strValue.Substring(3, 12)) > 0 ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValue.Substring(0, 3)) > 1)
                            numberAsString += " trilhões" + (Convert.ToDecimal(strValue.Substring(3, 12)) > 0 ? " e " : string.Empty);
                    }
                    else if (i == 3 & numberAsString != string.Empty)
                    {
                        if (Convert.ToInt32(strValue.Substring(3, 3)) == 1)
                            numberAsString += " bilhão" + (Convert.ToDecimal(strValue.Substring(6, 9)) > 0 ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValue.Substring(3, 3)) > 1)
                            numberAsString += " bilhões" + (Convert.ToDecimal(strValue.Substring(6, 9)) > 0 ? " e " : string.Empty);
                    }
                    else if (i == 6 & numberAsString != string.Empty)
                    {
                        if (Convert.ToInt32(strValue.Substring(6, 3)) == 1)
                            numberAsString += " milhão" + (Convert.ToDecimal(strValue.Substring(9, 6)) > 0 ? " e " : string.Empty);
                        else if (Convert.ToInt32(strValue.Substring(6, 3)) > 1)
                            numberAsString += " milhões" + (Convert.ToDecimal(strValue.Substring(9, 6)) > 0 ? " e " : string.Empty);
                    }
                    else if (i == 9 & numberAsString != string.Empty)
                        if (Convert.ToInt32(strValue.Substring(9, 3)) > 0)
                            numberAsString += " mil" + (Convert.ToDecimal(strValue.Substring(12, 3)) > 0 ? " e " : string.Empty);
                    if (i == 12)
                    {
                        if (numberAsString.Length > 8)
                            if (numberAsString.Substring(numberAsString.Length - 6, 6) == "bilhão" | numberAsString.Substring(numberAsString.Length - 6, 6) == "milhão")
                                numberAsString += " de";
                            else
                                if (numberAsString.Substring(numberAsString.Length - 7, 7) == "bilhões" | numberAsString.Substring(numberAsString.Length - 7, 7) == "milhões"
                                    | numberAsString.Substring(numberAsString.Length - 8, 7) == "trilhões")
                                numberAsString += " de";
                            else
                                    if (numberAsString.Substring(numberAsString.Length - 8, 8) == "trilhões")
                                numberAsString += " de";
                        if (Convert.ToInt64(strValue.Substring(0, 15)) == 1)
                            numberAsString += "";
                        else if (Convert.ToInt64(strValue.Substring(0, 15)) > 1)
                            numberAsString += "";
                        if (Convert.ToInt32(strValue.Substring(16, 2)) > 0 && numberAsString != string.Empty && Convert.ToInt64(strValue.Substring(0, 15)) >= 1)
                            numberAsString += " e ";
                    }
                    if (i == 15)
                        if (Convert.ToInt32(strValue.Substring(16, 2)) == 1)
                            numberAsString += "";
                        else if (Convert.ToInt32(strValue.Substring(16, 2)) > 1)
                            numberAsString += "";
                }
                return numberAsString;
            }
        }

        public static string WriteNumber(decimal valor)
        {
            if (valor <= 0)
                return string.Empty;
            else
            {
                string montagem = string.Empty;
                if (valor > 0 & valor < 1)
                {
                    valor *= 100;
                }
                string strValor = valor.ToString("000");
                int a = Convert.ToInt32(strValor.Substring(0, 1));
                int b = Convert.ToInt32(strValor.Substring(1, 1));
                int c = Convert.ToInt32(strValor.Substring(2, 1));
                if (a == 1) montagem += b + c == 0 ? "cem" : "cento";
                else if (a == 2) montagem += "duzentos";
                else if (a == 3) montagem += "trezentos";
                else if (a == 4) montagem += "quatrocentos";
                else if (a == 5) montagem += "quinhentos";
                else if (a == 6) montagem += "seiscentos";
                else if (a == 7) montagem += "setecentos";
                else if (a == 8) montagem += "oitocentos";
                else if (a == 9) montagem += "novecentos";
                if (b == 1)
                {
                    if (c == 0) montagem += (a > 0 ? " e " : string.Empty) + "dez";
                    else if (c == 1) montagem += (a > 0 ? " e " : string.Empty) + "onze";
                    else if (c == 2) montagem += (a > 0 ? " e " : string.Empty) + "doze";
                    else if (c == 3) montagem += (a > 0 ? " e " : string.Empty) + "treze";
                    else if (c == 4) montagem += (a > 0 ? " e " : string.Empty) + "quatorze";
                    else if (c == 5) montagem += (a > 0 ? " e " : string.Empty) + "quinze";
                    else if (c == 6) montagem += (a > 0 ? " e " : string.Empty) + "dezesseis";
                    else if (c == 7) montagem += (a > 0 ? " e " : string.Empty) + "dezessete";
                    else if (c == 8) montagem += (a > 0 ? " e " : string.Empty) + "dezoito";
                    else if (c == 9) montagem += (a > 0 ? " e " : string.Empty) + "dezenove";
                }
                else if (b == 2) montagem += (a > 0 ? " e " : string.Empty) + "vinte";
                else if (b == 3) montagem += (a > 0 ? " e " : string.Empty) + "trinta";
                else if (b == 4) montagem += (a > 0 ? " e " : string.Empty) + "quarenta";
                else if (b == 5) montagem += (a > 0 ? " e " : string.Empty) + "cinquenta";
                else if (b == 6) montagem += (a > 0 ? " e " : string.Empty) + "sessenta";
                else if (b == 7) montagem += (a > 0 ? " e " : string.Empty) + "setenta";
                else if (b == 8) montagem += (a > 0 ? " e " : string.Empty) + "oitenta";
                else if (b == 9) montagem += (a > 0 ? " e " : string.Empty) + "noventa";
                if (strValor.Substring(1, 1) != "1" & c != 0 & montagem != string.Empty) montagem += " e ";
                if (strValor.Substring(1, 1) != "1")
                    if (c == 1) montagem += "um";
                    else if (c == 2) montagem += "dois";
                    else if (c == 3) montagem += "três";
                    else if (c == 4) montagem += "quatro";
                    else if (c == 5) montagem += "cinco";
                    else if (c == 6) montagem += "seis";
                    else if (c == 7) montagem += "sete";
                    else if (c == 8) montagem += "oito";
                    else if (c == 9) montagem += "nove";
                return montagem;
            }
        }

        public static string WriteOrdinalNumber(int? valor, string gender)
        {
            string ordinalGender;
            ordinalGender = gender == "m" ? "o" : "a";
            if (valor <= 0 || valor == null)
                return string.Empty;
            else
            {
                string montagem = string.Empty;
                string strValor = valor.Value.ToString("0000");
                int a = Convert.ToInt32(strValor.Substring(0, 1));
                int b = Convert.ToInt32(strValor.Substring(1, 1));
                int c = Convert.ToInt32(strValor.Substring(2, 1));
                int d = Convert.ToInt32(strValor.Substring(3, 1));
                if (a == 1) montagem += "milésim" + ordinalGender;
                else if (a == 2) montagem += "segund" + ordinalGender + " milésim" + ordinalGender;
                else if (a == 3) montagem += "terceir" + ordinalGender + " milésim" + ordinalGender;
                else if (a == 4) montagem += "quart" + ordinalGender + " milésim" + ordinalGender;
                else if (a == 5) montagem += "quint" + ordinalGender + " milésim" + ordinalGender;
                else if (a == 6) montagem += "sext" + ordinalGender + " milésim" + ordinalGender;
                else if (a == 7) montagem += "sétim" + ordinalGender + " milésim" + ordinalGender;
                else if (a == 8) montagem += "oitav" + ordinalGender + " milésim" + ordinalGender;
                else if (a == 9) montagem += "non" + ordinalGender + " milésim" + ordinalGender;
                if (a > 0) montagem += "-";
                if (b == 1) montagem += "centésim" + ordinalGender;
                else if (b == 2) montagem += "ducentésim" + ordinalGender;
                else if (b == 3) montagem += "tricentésim" + ordinalGender;
                else if (b == 4) montagem += "quadringentésim" + ordinalGender;
                else if (b == 5) montagem += "quingentésim" + ordinalGender;
                else if (b == 6) montagem += "sexcentésim" + ordinalGender;
                else if (b == 7) montagem += "setingentésim" + ordinalGender;
                else if (b == 8) montagem += "octingentésim" + ordinalGender;
                else if (b == 9) montagem += "noningentésim" + ordinalGender;
                if (b > 0) montagem += "-";
                if (c == 1) montagem += "décim" + ordinalGender;
                else if (c == 2) montagem += "vigésim" + ordinalGender;
                else if (c == 3) montagem += "trigésim" + ordinalGender;
                else if (c == 4) montagem += "quadragésim" + ordinalGender;
                else if (c == 5) montagem += "quinquagésim" + ordinalGender;
                else if (c == 6) montagem += "sexagésim" + ordinalGender;
                else if (c == 7) montagem += "septuagésim" + ordinalGender;
                else if (c == 8) montagem += "octogésim" + ordinalGender;
                else if (c == 9) montagem += "nonagésim" + ordinalGender;
                if (c > 0) montagem += "-";
                if (d == 1) montagem += "primeir" + ordinalGender;
                else if (d == 2) montagem += "segund" + ordinalGender;
                else if (d == 3) montagem += "terceir" + ordinalGender;
                else if (d == 4) montagem += "quart" + ordinalGender;
                else if (d == 5) montagem += "quint" + ordinalGender;
                else if (d == 6) montagem += "sext" + ordinalGender;
                else if (d == 7) montagem += "sétim" + ordinalGender;
                else if (d == 8) montagem += "oitav" + ordinalGender;
                else if (d == 9) montagem += "non" + ordinalGender;
                return montagem;
            }
        }
    }

}
