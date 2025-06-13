namespace Mediator.Interfaces;


/// <summary>
/// Interface que define um manipulador (Handler) para uma requisição do tipo <typeparamref name="TRequest"/>
/// que retorna uma resposta do tipo <typeparamref name="TResponse"/>.
/// 
/// Esta interface é utilizada no padrão CQRS (Command Query Responsibility Segregation) 
/// para separar claramente as operações de leitura (queries) e escrita (commands).
/// 
/// O manipulador processa a lógica associada à requisição e retorna o resultado esperado.
/// 
/// Exemplo de uso:
/// 
/// public class GetUserByIdQuery : IRequest<UserDto>
/// {
///     public Guid UserId { get; set; }
/// }
/// 
/// public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
/// {
///     public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
///     {
///         // Lógica para buscar usuário e retornar DTO.
///     }
/// }
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição que implementa <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">Tipo da resposta que será retornada após o processamento.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///  Método responsável por manipular uma requisição e retornar uma resposta.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Uma Task contendo a resposta processada do tipo <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

}