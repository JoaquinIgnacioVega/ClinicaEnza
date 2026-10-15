using System.Net;
using System.Text.Json;
using ClinicaEnza.Domain.Exceptions;

namespace ClinicaEnza.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió una excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var mensaje = "Ocurrió un error interno en el servidor.";

        switch (exception)
        {
            case TurnoSuperpuestoException ex:
                statusCode = HttpStatusCode.Conflict;
                mensaje = ex.Message;
                break;

            case ReglaDeNegocioException ex:
                statusCode = HttpStatusCode.BadRequest;
                mensaje = ex.Message;
                break;

            case ArgumentException ex:
                statusCode = HttpStatusCode.BadRequest;
                mensaje = ex.Message;
                break;

            case InvalidOperationException ex:
                statusCode = HttpStatusCode.BadRequest;
                mensaje = ex.Message;
                break;

            case KeyNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                mensaje = ex.Message;
                break;
        }

        var response = new
        {
            status = (int)statusCode,
            error = statusCode.ToString(),
            message = mensaje,
            timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

