/// <summary>
/// Interface que representa uma requisição no padrão CQRS (Command Query Responsibility Segregation).
/// 
/// Este contrato é implementado por todos os "Commands" ou "Queries" que desejam obter uma resposta do tipo <typeparamref name="TResponse"/>.
/// 
/// Ao implementar esta interface, a requisição pode ser processada por um handler que implementa <see cref="IRequestHandler{TRequest, TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">Tipo da resposta esperada ao processar a requisição.</typeparam>
public interface IRequest<out TResponse> { }