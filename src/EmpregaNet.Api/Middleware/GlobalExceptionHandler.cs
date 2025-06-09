using System.Net;
using EmpregaNet.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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

            var (statusCode, title, errors) = MapException(exception);

            // Log.Error(exception, "Erro ao processar a requisição: {Message}", exception.Message); - ELK
            // Captura a exceção no Sentry
            SentrySdk.CaptureException(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Extensions = { ["errors"] = errors }
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static (int statusCode, string title, string[] errors) MapException(Exception exception)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var title = "Erro no Servidor";
            var errors = Array.Empty<string>();

            switch (exception)
            {
                case BadRequestException badRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Requisição inválida";
                    errors = badRequestException.Errors;
                    break;

                case NotFoundException notFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    title = "Recurso não encontrado";
                    errors = new[] { notFoundException.Message };
                    break;

                case InvalidOperationException invalidOperationException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    title = "Problema de concorrência";
                    errors = new[] { invalidOperationException.Message };
                    break;

                case KeyNotFoundException keyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    title = "Chave não encontrada";
                    errors = new[] { keyNotFoundException.Message };
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    title = "Acesso não autorizado";
                    errors = new[] { unauthorizedAccessException.Message };
                    break;

                case DatabaseNotFoundException databaseNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    title = "Banco de dados não encontrado";
                    errors = new[] { databaseNotFoundException.Message };
                    break;

                case ValidationException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Validação falhou";
                    errors = validationException.Errors.Select(e => e.ErrorMessage).ToArray();
                    break;

                default:
                    errors = new[] { exception?.StackTrace ?? "Erro interno no servidor." };
                    break;
            }

            return (statusCode, title, errors);
        }
    }
}