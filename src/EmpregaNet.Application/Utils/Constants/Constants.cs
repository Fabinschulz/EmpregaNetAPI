namespace EmpregaNet.Application.Utils;

public static partial class Constants
{
    /// <summary>
    /// Nomes de políticas (<c>AddPolicy</c> em <c>IdentityConfig</c>).
    /// </summary>
    public static class AuthPolicies
    {
        public const string Administrador = "Administrador";
        public const string Recrutamento = "Recrutamento";
    }

    /// <summary>
    /// Literais de documento/grupo Swagger (<see cref="Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute.GroupName"/>).
    /// </summary>
    public static class OpenApi
    {
        public const string V1 = "v1";
        public const string Admin = "admin";
        public const string Users = "users";
    }

    /// <summary>Valores de <see cref="Microsoft.AspNetCore.Identity.UserLoginInfo.LoginProvider"/>.</summary>
    public static class ExternalLogin
    {
        public const string GoogleProvider = "Google";
    }

    public static class Regex
    {
        public const string email = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-A-Za-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[A-Za-z0-9][\w\.-]*[A-Za-z0-9]\.[A-Za-z][A-Za-z\.]*[A-Za-z]$";
        public const string telefone = @"^(\+\d{2}\s?)?\(?\d{2}\)?\s?9\d{4,5}-?\d{4}$";
        public const string nomeUsuario = @"^(?!.*\.\.)(?!.*\.$)[^\W][\w.]{0,29}$";
        public const string nome = @"^[A-zÀ-ÿ']+\s([A-zÀ-ÿ']\s?)*[A-zÀ-ÿ']+$";
        public const string senha = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        public const string cnpj = @"^\d{2}\.?\d{3}\.?\d{3}/?\d{4}-?\d{2}$";
        public const string cpf = @"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$";
        public const string cep = @"^\d{5}-?\d{3}$";
    }
}
