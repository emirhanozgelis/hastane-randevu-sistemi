using System.Net;
using System.Text.Json;
using HospitalAppointment.Domain.Exceptions;

namespace HospitalAppointment.API.Middleware;

// Global hata yakalama middleware — tüm kontrolsüz istisnaları yakalar ve JSON formatında döner
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
            _logger.LogError(ex, "İşlenmeyen istisna: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = exception switch
        {
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                domainEx.ErrorCode,
                domainEx.Message),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "NOT_FOUND",
                exception.Message),

            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                "FORBIDDEN",
                exception.Message),

            ArgumentException => (
                HttpStatusCode.BadRequest,
                "BAD_REQUEST",
                exception.Message),

            _ => (
                HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR",
                "Sunucu tarafında beklenmeyen bir hata oluştu.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = message,
            errorCode = errorCode,
            timestamp = DateTime.UtcNow.ToString("O")
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
