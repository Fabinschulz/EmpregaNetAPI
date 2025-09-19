using EmpregaNet.Domain.Enums;

namespace EmpregaNet.Application.Common.Exceptions;

/// <summary>
/// Exceção lançada quando um usuário tenta acessar um recurso ou executar uma ação para a qual não possui permissão.
/// Pode ser utilizada em filtros de autorização, middlewares ou regras de negócio para sinalizar acesso proibido (HTTP 403).
/// Permite informar o recurso, o tipo de permissão e identificadores adicionais relacionados à tentativa de acesso negado.
/// </summary>
public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// Recurso protegido ao qual o acesso foi negado (ex: Pessoa Física).
    /// </summary>
    public PermissionResourceEnum? Resource { get; set; }

    /// <summary>
    /// Tipo de permissão exigida que foi negada (ex: Leitura, Criação, Atualização, etc).
    /// </summary>
    public PermissionTypeEnum? PermissionType { get; set; }

    /// <summary>
    /// Identificadores adicionais relacionados ao contexto da exceção (ex: Id do registro, usuário, etc).
    /// </summary>
    public object? Identifiers { get; set; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ForbiddenAccessException"/> sem detalhes adicionais.
    /// </summary>
    public ForbiddenAccessException() : base() { }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ForbiddenAccessException"/> especificando o recurso, tipo de permissão e identificadores.
    /// </summary>
    /// <param name="resource">Recurso protegido ao qual o acesso foi negado.</param>
    /// <param name="permissionType">Tipo de permissão exigida que foi negada.</param>
    /// <param name="identifiers">Identificadores adicionais relacionados ao contexto da exceção.</param>
    public ForbiddenAccessException(PermissionResourceEnum? resource, PermissionTypeEnum? permissionType, object identifiers) : base()
    {
        Resource = resource;
        PermissionType = permissionType;
        Identifiers = identifiers;
    }
}
