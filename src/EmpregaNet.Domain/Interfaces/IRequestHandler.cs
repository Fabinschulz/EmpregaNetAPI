namespace EmpregaNet.Domain.Interfaces;

public interface IRequestHandler<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///  Método responsável por manipular uma requisição e retornar uma resposta.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    
}