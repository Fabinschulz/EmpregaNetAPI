namespace EmpregaNet.Domain.Components.Mediator.Interfaces;

/// <summary>
/// Interface que representa uma requisição no padrão CQRS (Command Query Responsibility Segregation).
/// 
/// Este contrato é implementado por todos os "Commands" ou "Queries" que desejam obter uma resposta do tipo <typeparamref name="TResponse"/>.
/// 
/// Ao implementar esta interface, a requisição pode ser processada por um handler que implementa <see cref="IRequestHandler{TRequest, TResponse}"/>.
/// 
/// Exemplo de uso:
/// 
/// public class GetUserByIdQuery : IRequest<UserDto>
/// {
///     public Guid UserId { get; set; }
/// }
/// 
/// O handler correspondente seria:
/// 
/// public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
/// {
///     // Lógica de manipulação
/// }
/// </summary>
/// <typeparam name="TResponse">Tipo da resposta esperada ao processar a requisição.</typeparam>
public interface IRequest<TResponse> { }