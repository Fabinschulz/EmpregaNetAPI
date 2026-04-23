using System.Text;
using EmpregaNet.Application.Auth.ViewModel;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Auth;

/// <summary>
/// Informações do usuário autenticado e permissões derivadas do JWT no contexto HTTP.
/// </summary>
public class HttpCurrentUser : IHttpCurrentUser
{
    /// <summary>
    /// Contexto do usuário na requisição HTTP, responsável por recuperar os dados do usuário autenticado.
    /// </summary>
    private readonly HttpUserContext _userContext;

    public HttpCurrentUser(HttpUserContext userContext)
    {
        _userContext = userContext;
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
    public Task<List<UserPermissionVieModel>?> GetAllPermissions()
    {
        var permissions = _userContext.GetContextuser()?.Permissions;
        return Task.FromResult(permissions);
    }

    /// <summary>
    /// Verifica se o usuário autenticado possui uma permissão específica para um recurso e tipo de ação.
    /// </summary>
    /// <param name="resource">Recurso a ser verificado (<see cref="PermissionResourceEnum"/>).</param>
    /// <param name="type">Tipo de permissão (<see cref="PermissionTypeEnum"/>).</param>
    /// <returns><c>true</c> se o usuário possui a permissão; caso contrário, <c>false</c>.</returns>
    public async Task<bool> HasPermission(PermissionResourceEnum resource, PermissionTypeEnum type)
    {
        var permissions = await GetAllPermissions();
        if (permissions is null || permissions.Count == 0)
            return false;

        return permissions.Any(p => p.Resource == resource && p.Type == type);
    }

    /// <summary>
    /// Gera uma chave composta a partir da chave do usuário e um tipo informado.
    /// </summary>
    /// <param name="type">Tipo a ser concatenado à chave.</param>
    /// <returns>Chave composta no formato "Key:Type".</returns>
    public string GetKeyType(string type) => $"{Key}:{type}";
}
