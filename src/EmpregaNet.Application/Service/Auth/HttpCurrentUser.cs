using System.Text;
using EmpregaNet.Application.ViewModel;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Service;

/// <summary>
/// Implementação de <see cref="IHttpCurrentUser"/> responsável por fornecer informações do usuário autenticado
/// no contexto da requisição HTTP, bem como suas permissões e dados de autenticação.
/// Utiliza <see cref="HttpUserContext"/> para obter o usuário corrente e <see cref="JwtBuilder"/> para consultar permissões.
/// </summary>
public class HttpCurrentUser : IHttpCurrentUser
{
    /// <summary>
    /// Contexto do usuário na requisição HTTP, responsável por recuperar os dados do usuário autenticado.
    /// </summary>
    private readonly HttpUserContext _userContext;

    /// <summary>
    /// Serviço de autenticação utilizado para consultar permissões do usuário.
    /// </summary>
    private readonly IJwtBuilder _jwtBuilder;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="HttpCurrentUser"/>.
    /// </summary>
    /// <param name="userContext">Contexto do usuário HTTP injetado.</param>
    /// <param name="authService">Serviço de autenticação injetado.</param>
    public HttpCurrentUser(HttpUserContext userContext, IJwtBuilder authService)
    {
        _userContext = userContext;
        _jwtBuilder = authService;
    }

    /// <summary>
    /// Identificador único do usuário autenticado.
    /// </summary>
    /// <exception cref="Exception">Lançada se o UserId não estiver disponível no contexto.</exception>
    public long UserId => _userContext.GetContextuser()?.UserToken?.Id ?? throw new Exception("ContextUser UserToken or Id not found.");

    /// <summary>
    /// Nome do usuário autenticado.
    /// </summary>
    /// <exception cref="Exception">Lançada se o UserName não estiver disponível no contexto.</exception>
    public string Username => _userContext.GetContextuser()?.UserToken?.Username ?? throw new Exception("ContextUser Username not found.");

    /// <summary>
    /// Chave única do usuário autenticado, decodificada de Base64.
    /// </summary>
    /// <exception cref="Exception">Lançada se a chave não estiver disponível no contexto.</exception>
    public string Key => !string.IsNullOrEmpty(_userContext.GetContextuser()?.Key)
        ? Encoding.UTF8.GetString(Convert.FromBase64String(_userContext.GetContextuser()?.Key!)).ToString()
        : throw new Exception("ContextUser Key not found.");

    /// <summary>
    /// Token de acesso (JWT) do usuário autenticado.
    /// </summary>
    /// <exception cref="Exception">Lançada se o AccessToken não estiver disponível no contexto.</exception>
    public string AccessToken => _userContext.GetContextuser() is not null
        ? _userContext.GetContextuser()?.AccessToken ?? throw new Exception("ContextUser AccessToken not found.")
        : throw new Exception("ContextUser not found.");

    /// <summary>
    /// Obtém o objeto <see cref="UserLoggedViewModel"/> do contexto da requisição HTTP.
    /// </summary>
    /// <returns>Instância de <see cref="UserLoggedViewModel"/> representando o usuário autenticado.</returns>
    /// <exception cref="Exception">Lançada se o usuário não estiver disponível no contexto.</exception>
    public UserLoggedViewModel GetContextUser() => _userContext.GetContextuser() ?? throw new Exception("Contextuser not found.");

    /// <summary>
    /// Obtém todas as permissões do usuário autenticado consultando o serviço de autenticação.
    /// </summary>
    /// <returns>Lista de permissões (<see cref="UserPermissionVieModel"/>) ou <c>null</c> se não houver permissões.</returns>
    public async Task<List<UserPermissionVieModel>?> GetAllPermissions() => await _jwtBuilder.GetAllPermissions(Key);

    /// <summary>
    /// Verifica se o usuário autenticado possui uma permissão específica para um recurso e tipo de ação.
    /// </summary>
    /// <param name="resource">Recurso a ser verificado (<see cref="PermissionResourceEnum"/>).</param>
    /// <param name="type">Tipo de permissão (<see cref="PermissionTypeEnum"/>).</param>
    /// <returns><c>true</c> se o usuário possui a permissão; caso contrário, <c>false</c>.</returns>
    public async Task<bool> HasPermission(PermissionResourceEnum resource, PermissionTypeEnum type)
    {
        var permissions = await GetAllPermissions();

        if (permissions == null || !permissions.Any())
        {
            return false;
        }

        var permission = permissions.FirstOrDefault(p => p.Resource == resource && p.Type == type);
        return permission != null;
    }

    /// <summary>
    /// Gera uma chave composta a partir da chave do usuário e um tipo informado.
    /// </summary>
    /// <param name="type">Tipo a ser concatenado à chave.</param>
    /// <returns>Chave composta no formato "Key:Type".</returns>
    public string GetKeyType(string type) => $"{Key}:{type}";
}