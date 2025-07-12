using EmpregaNet.Application.User;
using EmpregaNet.Domain.Entities;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace EmpregaNet.Application.Service;

/// <summary>
/// Contexto do usuário em uma requisição HTTP.
/// Responsável por extrair informações do usuário autenticado a partir dos claims do token JWT
/// e disponibilizá-las para o restante do pipeline da aplicação.
/// </summary>
public class HttpUserContext
{
    /// <summary>
    /// Acessor para o contexto HTTP, permitindo acessar dados da requisição atual.
    /// </summary>
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Instância cacheada do usuário autenticado, para evitar múltiplas leituras dos claims.
    /// </summary>
    private UserLoggedViewModel? _userLogged;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="HttpUserContext"/>.
    /// </summary>
    /// <param name="httpContextAccessor">Acessor para o contexto HTTP, injetado pelo ASP.NET Core.</param>
    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Extrai os claims do usuário autenticado e os adiciona como cabeçalhos na requisição HTTP.
    /// Isso facilita o acesso a informações do usuário em outros pontos do pipeline, como middlewares e controllers.
    /// </summary>
    /// <param name="context">Contexto HTTP da requisição.</param>
    public static void SetHeader(HttpContext context)
    {
        var userClaims = context?.User?.Identities?.FirstOrDefault()?.Claims;

        if (userClaims != null)
        {
            var userId = userClaims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var userName = userClaims.FirstOrDefault(c => c.Type == "userName")?.Value;
            var key = userClaims.FirstOrDefault(c => c.Type == "key")?.Value;

            // Adiciona os valores como cabeçalhos para serem utilizados no resto do pipeline de middleware
            if (!string.IsNullOrEmpty(userId))
            {
                if (context!.Request.Headers.ContainsKey("UserId"))
                {
                    context.Request.Headers.Remove("UserId");
                }
                context.Request.Headers["UserId"] = userId;
            }

            if (!string.IsNullOrEmpty(userName))
            {
                if (context!.Request.Headers.ContainsKey("UserName"))
                {
                    context.Request.Headers.Remove("UserName");
                }
                context.Request.Headers["UserName"] = userName;
            }

            if (!string.IsNullOrEmpty(key))
            {
                if (context!.Request.Headers.ContainsKey("Key"))
                {
                    context.Request.Headers.Remove("Key");
                }
                // Decodifica a chave de Base64 antes de adicionar ao header
                context.Request.Headers["Key"] = Encoding.UTF8.GetString(Convert.FromBase64String(key));
            }

            // Adiciona o token de acesso como header "AccessToken" se o header "Authorization" estiver presente
            if (!string.IsNullOrEmpty(context?.Request.Headers["Authorization"]))
            {
                if (context.Request.Headers.ContainsKey("AccessToken"))
                {
                    context.Request.Headers.Remove("AccessToken");
                }
                context.Request.Headers["AccessToken"] = context.Request.Headers["Authorization"];
            }
        }
    }

    /// <summary>
    /// Obtém o usuário autenticado a partir dos claims presentes no contexto HTTP.
    /// Os dados são extraídos apenas uma vez e cacheados para reutilização durante o ciclo da requisição.
    /// </summary>
    /// <returns>Instância de <see cref="User"/> representando o usuário autenticado.</returns>
    /// <exception cref="Exception">
    /// Lançada se não for possível obter os dados do usuário a partir dos claims ou se o usuário não estiver autenticado.
    /// Em modo DEBUG, a exceção original é propagada.
    /// </exception>
    public UserLoggedViewModel? GetContextuser()
    {
        try
        {
            if (_userLogged != null)
            {
                return _userLogged;
            }

            HttpContext context = _httpContextAccessor.HttpContext!;

            var claims = context?.User?.Identities?.FirstOrDefault()?.Claims;

            var userIdClaim = claims!.FirstOrDefault(s => s.Type == "userId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var userId))
            {
                _userLogged = new UserLoggedViewModel
                {
                    UserToken = new UserToken
                    {
                        Id = userId,
                        Username = claims?.FirstOrDefault(s => s.Type == "userName")?.Value ?? string.Empty,
                        Email = claims?.FirstOrDefault(s => s.Type == "email")?.Value ?? string.Empty,
                        Claims = claims?.Select(c => new UserClaim { Type = c.Type, Value = c.Value }).ToList() ?? new List<UserClaim>()
                    },
                    AccessToken = context!.Request.Headers["AccessToken"].ToString(),
                    ExpiresIn = TimeSpan.FromHours(1).TotalSeconds,
                    Permissions = claims?
                   .Where(c => c.Type == "permission")
                   .Select(c => new UserPermissionVieModel
                   {
                       Resource = Enum.Parse<PermissionResourceEnum>(c.Value.Split(':')[0]),
                       Type = Enum.Parse<PermissionTypeEnum>(c.Value.Split(':')[1])
                   })
                   .ToList() ?? new List<UserPermissionVieModel>()
                };
            }


            return _userLogged;
        }
        catch (Exception ex)
        {
            var erro = ex;

#if DEBUG
            // Em ambiente de desenvolvimento, propaga a exceção original para facilitar o diagnóstico
            throw new Exception(ex.Message, ex);
#endif
            // Em produção, retorna uma mensagem genérica
            throw new Exception("Usuário não autenticado!");
        }
    }
}
