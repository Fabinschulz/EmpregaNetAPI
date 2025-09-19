using System.Text;

namespace EmpregaNet.Application.Utils.Helpers
{
    public class Conversor
    {
        static public string EncodeToBase64(string texto)
        {
            try
            {
                byte[] textoAsBytes = Encoding.ASCII.GetBytes(texto);
                string resultado = Convert.ToBase64String(textoAsBytes);
                return resultado;
            }
            catch (Exception)
            {
                throw;
            }
        }
        static public string DecodeFrom64(string dados)
        {
            try
            {
                byte[] dadosAsBytes = Convert.FromBase64String(dados);
                string resultado = Encoding.ASCII.GetString(dadosAsBytes);
                return resultado;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
