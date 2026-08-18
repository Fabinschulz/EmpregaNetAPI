using System.Net;
using System.Text.Json;
using EmpregaNet.Application.Common.Exceptions;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Enums;
using Microsoft.AspNetCore.Diagnostics;

namespace EmpregaNet.Api.Middleware
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private static readonly string[] EnvelopeSegments = ["entity", "request", "command", "model", "dto"];

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

                if (domainError.Errors.Count > 0)
                {
                    scope.SetExtra(
                        "Validation_Errors",
                        domainError.Errors.Select(e => e.Field is null ? e.Message : $"{e.Field}: {e.Message}").ToArray());
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
        /// Mapeia a exceção para o corpo <see cref="DomainError"/> e o status HTTP correspondente.
        /// </summary>
        private (DomainError domainError, int httpStatusCode) MapExceptionToDomainError(Exception exception, string correlationId)
        {
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            var code = DomainErrorEnum.UNEXPECTED_EXCEPTION;
            var headline = "Erro inesperado. Tente novamente mais tarde.";
            var statusCode = (int)HttpStatusCode.InternalServerError;
            IReadOnlyList<DomainErrorItem> errors = [];

            switch (exception)
            {
                case BadRequestException badRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = DomainErrorEnum.INVALID_PARAMS;
                    headline = "Requisição inválida.";
                    errors = FormLevel(badRequestException.Errors, code);
                    break;

                case NotFoundException notFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    code = DomainErrorEnum.RESOURCE_ID_NOT_FOUND;
                    headline = "Recurso não encontrado.";
                    errors = ClientErrorDetails(isDevelopment, notFoundException.Message, code);
                    break;

                case InvalidOperationException invalidOperationException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    code = DomainErrorEnum.INVALID_ACTION_FOR_RECORD;
                    headline = "Operação inválida.";
                    errors = ClientErrorDetails(isDevelopment, invalidOperationException.Message, code);
                    break;

                case KeyNotFoundException keyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    code = DomainErrorEnum.RESOURCE_ID_NOT_FOUND;
                    headline = "Chave não encontrada.";
                    errors = ClientErrorDetails(isDevelopment, keyNotFoundException.Message, code);
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    code = DomainErrorEnum.MISSING_RESOURCE_PERMISSION;
                    headline = "Acesso não autorizado.";
                    errors = ClientErrorDetails(isDevelopment, unauthorizedAccessException.Message, code);
                    break;

                case DatabaseNotFoundException databaseNotFoundException:
                    statusCode = (int)HttpStatusCode.ServiceUnavailable;
                    code = DomainErrorEnum.UNEXPECTED_EXCEPTION;
                    headline = "Banco de dados não encontrado.";
                    errors = ClientErrorDetails(isDevelopment, databaseNotFoundException.Message, code);
                    break;

                case ValidationAppException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = validationException.Code ?? DomainErrorEnum.INVALID_PARAMS;
                    headline = "Corrija os campos destacados.";
                    errors = ToErrorItems(validationException, code);
                    break;

                case ForbiddenAccessException forbiddenAccessException:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    code = DomainErrorEnum.MISSING_RESOURCE_PERMISSION;
                    headline = "Acesso negado. Você não possui o nível de permissão necessário para acessar este recurso.";
                    errors = ClientErrorDetails(isDevelopment, forbiddenAccessException.Message, code);
                    break;

                case NotSupportedException notSupportedException:
                    statusCode = (int)HttpStatusCode.NotImplemented;
                    code = DomainErrorEnum.UNSUPPORTED_OPERATION;
                    headline = notSupportedException.Message ?? "Operação não suportada.";
                    errors = ClientErrorDetails(isDevelopment, notSupportedException.Message, code);
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    code = DomainErrorEnum.UNEXPECTED_EXCEPTION;
                    headline = "Erro interno no servidor.";
                    errors = ClientErrorDetails(isDevelopment, exception.Message, code);
                    break;
            }

            var domainError = new DomainError
            {
                StatusCode = statusCode,
                Code = code,
                Message = ResolveMessage(headline, errors),
                Errors = errors,
                CorrelationId = correlationId,
                StackTrace = isDevelopment ? exception.StackTrace : null
            };

            return (domainError, statusCode);
        }

        private static string ResolveMessage(string headline, IReadOnlyList<DomainErrorItem> errors) =>
            errors.Count == 1 ? errors[0].Message : headline;

        private static List<DomainErrorItem> ToErrorItems(ValidationAppException exception, DomainErrorEnum code) =>
            exception.Errors
                .SelectMany(entry => entry.Value.Select(message => new DomainErrorItem
                {
                    Field = NormalizeFieldPath(entry.Key),
                    Message = message,
                    Code = code
                }))
                .ToList();

        private static List<DomainErrorItem> FormLevel(IEnumerable<string> messages, DomainErrorEnum code) =>
            messages
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Select(message => new DomainErrorItem { Field = null, Message = message, Code = code })
                .ToList();

        private static List<DomainErrorItem> ClientErrorDetails(bool isDevelopment, string? detail, DomainErrorEnum code) =>
            isDevelopment && !string.IsNullOrWhiteSpace(detail)
                ? [new DomainErrorItem { Field = null, Message = detail, Code = code }]
                : [];

        /// <summary>
        /// Converte o caminho vindo do FluentValidation ou de <c>nameof</c> no caminho que o cliente espera.
        /// Exemplo: <c>entity.Address.Street</c> -> <c>address.street</c>
        /// </summary>
        private static string? NormalizeFieldPath(string? propertyPath)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                return null;
            }

            var segments = propertyPath
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .SkipWhile(segment => EnvelopeSegments.Contains(segment, StringComparer.OrdinalIgnoreCase))
                .Select(ToCamelCaseSegment)
                .ToArray();

            return segments.Length == 0 ? null : string.Join('.', segments);
        }

        private static string ToCamelCaseSegment(string segment)
        {
            var bracket = segment.IndexOf('[');
            if (bracket < 0)
            {
                return JsonNamingPolicy.CamelCase.ConvertName(segment);
            }

            var name = segment[..bracket];
            var indexer = segment[bracket..];
            return JsonNamingPolicy.CamelCase.ConvertName(name) + indexer;
        }
    }
}
