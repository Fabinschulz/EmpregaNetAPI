using System.Net;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Diagnostics;

namespace EmpregaNet.Api.Middleware
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var correlationId = httpContext.Items["Correlation-ID"]?.ToString() ?? Guid.NewGuid().ToString();
            exception = ResolveHandleableException(exception);
            var (domainError, httpStatusCode) = MapExceptionToDomainError(exception, correlationId);

            _logger.LogError(exception, "Erro ao processar a requisição: {Message}. CorrelationId: {CorrelationId}", exception.Message, correlationId);
            SentrySdk.ConfigureScope(scope =>
             {
                 scope.SetExtra("DomainError_Code", domainError.Code.ToString());
                 scope.SetExtra("DomainError_Message", domainError.Message);
                 scope.SetExtra("DomainError_StatusCode", domainError.StatusCode);

                 if (domainError.Details is IDictionary<string, object> detailsDictionary && detailsDictionary.TryGetValue("Errors", out var errorsValue))
                 {
                     if (errorsValue is string[] stringErrors)
                     {
                         scope.SetExtra("Validation_Errors", stringErrors);
                     }
                     else if (errorsValue is IEnumerable<string> enumerableErrors)
                     {
                         scope.SetExtra("Validation_Errors", enumerableErrors.ToArray());
                     }
                     else if (errorsValue is object[] objectErrors)
                     {
                         scope.SetExtra("Validation_Errors", objectErrors.Select(e => e?.ToString() ?? "N/A").ToArray());
                     }
                 }

                 SentrySdk.CaptureException(exception);
             });

            httpContext.Response.StatusCode = httpStatusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(domainError, cancellationToken);

            return true;
        }
    
        private Exception ResolveHandleableException(Exception exception)
        {
            var current = exception;

            while (current is not null)
            {
                if (current is BadRequestException
                    or NotFoundException
                    or InvalidOperationException
                    or KeyNotFoundException
                    or UnauthorizedAccessException
                    or DatabaseNotFoundException
                    or ValidationAppException
                    or ForbiddenAccessException
                    or NotSupportedException)
                {
                    return current;
                }

                current = current.InnerException;
            }

            return exception;
        }


        /// <summary>
        /// Mapeia diferentes tipos de exceção para uma estrutura DomainError e um StatusCode HTTP.
        /// </summary>
        /// <param name="exception">A exceção a ser mapeada.</param>
        /// <param name="correlationId">O ID de correlação da requisição.</param>
        /// <returns>Uma tupla contendo o DomainError mapeado e o StatusCode HTTP apropriado.</returns>
        private (DomainError domainError, int httpStatusCode) MapExceptionToDomainError(Exception exception, string correlationId)
        {
            var code = DomainErrorEnum.UNEXPECTED_EXCEPTION;
            var message = "Erro inesperado. Tente novamente mais tarde.";
            object details = new { };
            string[] errors = Array.Empty<string>();
            var statusCode = (int)HttpStatusCode.InternalServerError;

            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            switch (exception)
            {
                case BadRequestException badRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = DomainErrorEnum.INVALID_PARAMS;
                    message = "Requisição inválida.";
                    errors = badRequestException.Errors;
                    break;

                case NotFoundException notFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    code = DomainErrorEnum.RESOURCE_ID_NOT_FOUND;
                    message = "Recurso não encontrado.";
                    errors = ClientErrorDetails(isDevelopment, notFoundException.Message);
                    break;

                case InvalidOperationException invalidOperationException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    code = DomainErrorEnum.INVALID_ACTION_FOR_RECORD;
                    message = "Operação inválida.";
                    errors = ClientErrorDetails(isDevelopment, invalidOperationException.Message);
                    break;

                case KeyNotFoundException keyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    code = DomainErrorEnum.RESOURCE_ID_NOT_FOUND;
                    message = "Chave não encontrada.";
                    errors = ClientErrorDetails(isDevelopment, keyNotFoundException.Message);
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    code = DomainErrorEnum.MISSING_RESOURCE_PERMISSION;
                    message = "Acesso não autorizado.";
                    errors = ClientErrorDetails(isDevelopment, unauthorizedAccessException.Message);
                    break;

                case DatabaseNotFoundException databaseNotFoundException:
                    statusCode = (int)HttpStatusCode.ServiceUnavailable;
                    code = DomainErrorEnum.UNEXPECTED_EXCEPTION;
                    message = "Banco de dados não encontrado.";
                    errors = ClientErrorDetails(isDevelopment, databaseNotFoundException.Message);
                    break;

                case ValidationAppException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = validationException.Code ?? DomainErrorEnum.INVALID_PARAMS;
                    message = validationException.Message ?? "Erro de validação.";
                    errors = validationException.Errors.SelectMany(e => e.Value).ToArray();
                    break;

                case ForbiddenAccessException forbiddenAccessException:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    code = DomainErrorEnum.MISSING_RESOURCE_PERMISSION;
                    message = "Acesso negado. Você não possui o nível de permissão necessário para acessar este recurso.";
                    errors = ClientErrorDetails(isDevelopment, forbiddenAccessException.Message);
                    break;

                case NotSupportedException notSupportedException:
                    statusCode = (int)HttpStatusCode.NotImplemented;
                    code = DomainErrorEnum.UNSUPPORTED_OPERATION;
                    message = notSupportedException.Message ?? "Operação não suportada.";
                    errors = ClientErrorDetails(isDevelopment, notSupportedException.Message ?? "Mensagem não fornecida.");
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    code = DomainErrorEnum.UNEXPECTED_EXCEPTION;
                    message = "Erro interno no servidor.";
                    errors = ClientErrorDetails(isDevelopment, exception.Message);
                    break;
            }

            details = new Dictionary<string, object>();

            if (errors.Any())
            {
                ((Dictionary<string, object>)details).Add("Errors", errors);
            }
            if (exception.StackTrace != null && isDevelopment)
            {
                ((Dictionary<string, object>)details).Add("StackTrace", exception.StackTrace);
            }

            var domainError = new DomainError
            {
                StatusCode = statusCode,
                Code = code,
                Message = message,
                Details = details,
                CorrelationId = correlationId
            };

            return (domainError, statusCode);
        }

        private static string[] ClientErrorDetails(bool isDevelopment, string? detail) =>
            isDevelopment && !string.IsNullOrWhiteSpace(detail)
                ? [detail]
                : Array.Empty<string>();
    }
}