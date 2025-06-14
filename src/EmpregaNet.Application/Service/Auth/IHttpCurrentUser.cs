using EmpregaNet.Application.ViewModel;
using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Service;

/// <summary>
/// Interface que abstrai o acesso ao usuário autenticado no contexto da requisição HTTP.
/// Permite obter informações básicas do usuário, token de acesso, permissões e realizar verificações de autorização.
/// Implementações típicas extraem os dados de claims do token JWT ou de um contexto de usuário.
/// </summary>
public interface IHttpCurrentUser
{
    /// <summary>
    /// Obtém o objeto <see cref="UserLoggedViewModel"/> representando o usuário autenticado no contexto atual.
    /// </summary>
    /// <returns>Instância de <see cref="UserLoggedViewModel"/> com os dados do usuário autenticado.</returns>
    UserLoggedViewModel? GetContextUser();

    /// <summary>
    /// Identificador único do usuário autenticado.
    /// </summary>
    long UserId { get; }

    /// <summary>
    /// Nome do usuário autenticado.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Chave única do usuário autenticado, geralmente utilizada para identificação e cache.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Token de acesso (JWT) do usuário autenticado.
    /// </summary>
    string AccessToken { get; }

    /// <summary>
    /// Obtém todas as permissões do usuário autenticado.
    /// </summary>
    /// <returns>Lista de permissões (<see cref="UserPermissionVieModel"/>) ou <c>null</c> se não houver permissões.</returns>
    Task<List<UserPermissionVieModel>?> GetAllPermissions();

    /// <summary>
    /// Verifica se o usuário autenticado possui uma permissão específica para um recurso e tipo de ação.
    /// </summary>
    /// <param name="resource">Recurso a ser verificado (<see cref="PermissionResourceEnum"/>).</param>
    /// <param name="type">Tipo de permissão exigida (<see cref="PermissionTypeEnum"/>).</param>
    /// <returns><c>true</c> se o usuário possui a permissão; caso contrário, <c>false</c>.</returns>
    Task<bool> HasPermission(PermissionResourceEnum resource, PermissionTypeEnum type);

    /// <summary>
    /// Gera uma chave composta a partir da chave do usuário e um tipo informado.
    /// Útil para operações de cache ou identificação de permissões específicas.
    /// </summary>
    /// <param name="type">Tipo a ser concatenado à chave.</param>
    /// <returns>Chave composta no formato "Key:Type".</returns>
    string GetKeyType(string type);
}
