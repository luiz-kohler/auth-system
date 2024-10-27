using Auth_API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Auth_API.Common
{
    public static class GlobalExceptionHandler
    {
        public static async Task Handle(HttpContext httpContext)
        {
            var exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature == null)
                return;

            var (httpStatusCode, message) = exceptionHandlerFeature.Error switch
            {
                ValidationException exception => (HttpStatusCode.BadRequest, MontarMensagemErro(exception)),
                NotFoundException exception => (HttpStatusCode.BadRequest, exception.Message),
                BadRequestException exception => (HttpStatusCode.BadRequest, exception.Message),
                InvalidOperationException exception => (HttpStatusCode.Unauthorized, "you do not have access for this action"),
                UnauthorizedAccessException exception => (HttpStatusCode.Unauthorized, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "unexpected error")
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)httpStatusCode;

            var jsonResponse = new
            {
                httpContext.Response.StatusCode,
                Message = message,
            };

            var jsonSerialised = JsonSerializer.Serialize(jsonResponse);
            await httpContext.Response.WriteAsync(jsonSerialised);
        }

        private static string MontarMensagemErro(ValidationException erro)
        {
            var listaDeErros = erro.Failures.Values.Select(failures => failures.First());
            return string.Join(", ", listaDeErros);
        }
    }
}
