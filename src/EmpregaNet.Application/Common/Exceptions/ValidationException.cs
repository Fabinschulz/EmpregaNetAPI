using EmpregaNet.Domain.Enums;
using FluentValidation.Results;

namespace Common.Exceptions;

/// <summary>
/// Exceção lançada quando uma ou mais validações de dados falham durante a execução de uma operação.
/// Permite informar um código de erro de domínio (<see cref="DomainErrorEnum"/>) e um dicionário detalhado
/// com os erros de validação agrupados por propriedade.
/// Pode ser utilizada em regras de negócio, serviços de aplicação ou middlewares para sinalizar erros de validação (ex: HTTP 400).
/// </summary>
public class ValidationAppException : Exception
{
    /// <summary>
    /// Código de erro de domínio relacionado à validação, conforme o enum <see cref="DomainErrorEnum"/>.
    /// </summary>
    public DomainErrorEnum? Code { get; set; }

    /// <summary>
    /// Dicionário contendo os erros de validação agrupados por nome da propriedade.
    /// A chave é o nome da propriedade e o valor é um array de mensagens de erro.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ValidationAppException"/> com uma mensagem padrão.
    /// </summary>
    /// <param name="message">Mensagem de erro (opcional).</param>
    public ValidationAppException(string message = "Ocorreram uma ou mais falhas de validação.")
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ValidationAppException"/> para um erro de validação em uma propriedade específica.
    /// </summary>
    /// <param name="propertyName">Nome da propriedade com erro.</param>
    /// <param name="errorMessage">Mensagem de erro de validação.</param>
    /// <param name="code">Código de erro de domínio (<see cref="DomainErrorEnum"/>).</param>
    public ValidationAppException(string propertyName, string errorMessage, DomainErrorEnum code)
        : this()
    {
        Code = code;
        Errors =
            new List<ValidationFailure> { new ValidationFailure(propertyName, errorMessage) }
            .GroupBy(g => g.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }


    /// <summary>
    /// Inicializa uma nova instância de <see cref="ValidationAppException"/> a partir de uma lista de falhas de validação.
    /// </summary>
    /// <param name="failures">Coleção de falhas de validação (<see cref="ValidationFailure"/>).</param>
    public ValidationAppException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(g => g.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}
