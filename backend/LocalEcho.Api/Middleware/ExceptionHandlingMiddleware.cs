using System.Net;
using System.Text.Json;
using LocalEcho.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LocalEcho.API.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var statusCode = (int)HttpStatusCode.InternalServerError;
        var title = "Internal Server Error";
        var detail = "Произошла внутренняя ошибка сервера.";

        switch (exception)
        {
            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound; 
                title = "Not Found";
                detail = exception.Message;
                break;
            case SecurityTokenException:
                statusCode = (int)HttpStatusCode.Unauthorized; // 401
                title = "Unauthorized";
                detail = "Недействительный или поврежденный токен авторизации.";
                break;
            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Forbidden; 
                title = "Forbidden";
                detail = exception.Message;
                break;

            case ArgumentException: 
            case InvalidOperationException: 
            case BadRequestException: 
                statusCode = (int)HttpStatusCode.BadRequest; 
                title = "Bad Request";
                detail = exception.Message;
                break;

            default:
                _logger.LogError(exception, "Необработанное исключение: {Message}", exception.Message);
                
                break;
        }

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions = 
            {
                ["traceId"] = context.TraceIdentifier 
            }
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        
        await context.Response.WriteAsync(json);
    }
}