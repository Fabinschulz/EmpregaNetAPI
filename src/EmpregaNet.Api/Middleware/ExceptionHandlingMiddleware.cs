using EmpregaNet.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace EmpregaNet.Api.Middleware
{
    // deprecated
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = HttpStatusCode.InternalServerError; // 500 por padrão
            var message = "Ocorreu um erro interno no servidor.";
            var errors = new string[] { };

            // // Captura a exceção no Sentry
            SentrySdk.CaptureException(exception);

            // Tratar exceções específicas
            switch (exception)
            {
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    message = "Requisição inválida.";
                    errors = badRequestException.Errors;
                    break;

                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    message = "Recurso não encontrado.";
                    errors = new string[] { notFoundException.Message };
                    break;

                case InvalidOperationException invalidOperationException:
                    statusCode = HttpStatusCode.Conflict; // 409
                    message = "Ocorreu um problema de concorrência ao acessar o banco de dados. Tente novamente.";
                    errors = new string[] { invalidOperationException.Message };
                    break;

                case KeyNotFoundException keyNotFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    message = "Recurso não encontrado.";
                    errors = new string[] { keyNotFoundException.Message };
                    break;

                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized; // 401
                    message = "Acesso não autorizado.";
                    errors = new string[] { unauthorizedAccessException.Message };
                    break;
                
                case DatabaseNotFoundException databaseNotFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    message = "Banco de dados não encontrado.";
                    errors = new string[] { databaseNotFoundException.Message };
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError; // 500
                    message = exception.Message;
                    errors = new string[] { exception?.StackTrace ?? "Erro interno no servidor." };
                    break;
            }

            // Configurar a resposta
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                StatusCode = (int)statusCode,
                Message = message,
                Errors = errors
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
