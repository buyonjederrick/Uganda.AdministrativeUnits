using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Uganda.AdministrativeUnits.Application.Exceptions;
using Uganda.AdministrativeUnits.Contracts.Responses;

namespace Uganda.AdministrativeUnits.Api.Middleware;

/// <summary>
/// Maps each domain exception to its status code and writes the standard <see cref="ApiResponse{T}"/> envelope.
/// </summary>
public sealed partial class BadRequestExceptionMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<BadRequestExceptionMiddleware> _logger;

    public BadRequestExceptionMiddleware(RequestDelegate next, ILogger<BadRequestExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (BadRequestException ex)
        {
            LogBadRequest(_logger, ex, context.Request.Method, context.Request.Path.Value);
            await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message, ex.Errors).ConfigureAwait(false);
        }
        catch (NotFoundException ex)
        {
            LogNotFound(_logger, ex, context.Request.Method, context.Request.Path.Value);
            await WriteAsync(context, HttpStatusCode.NotFound, ex.Message, ex.Errors).ConfigureAwait(false);
        }
        catch (UnauthorizedException ex)
        {
            LogUnauthorized(_logger, ex, context.Request.Method, context.Request.Path.Value);
            await WriteAsync(context, HttpStatusCode.Unauthorized, ex.Message, ex.Errors).ConfigureAwait(false);
        }
        catch (ForbiddenException ex)
        {
            LogForbidden(_logger, ex, context.Request.Method, context.Request.Path.Value);
            await WriteAsync(context, HttpStatusCode.Forbidden, ex.Message, ex.Errors).ConfigureAwait(false);
        }
        catch (InternalServerException ex)
        {
            LogInternalServer(_logger, ex, context.Request.Method, context.Request.Path.Value);
            await WriteAsync(context, HttpStatusCode.InternalServerError, ex.Message, ex.Errors).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUnhandled(_logger, ex, context.Request.Method, context.Request.Path.Value);
            await WriteAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please contact support if the issue persists.").ConfigureAwait(false);
        }
    }

    private static async Task WriteAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        string[]? errors = null)
    {
        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException("The response has already started; cannot write error payload.");
        }

        List<string> errorMessages = [message];
        if (errors is { Length: > 0 })
        {
            errorMessages.AddRange(errors);
        }

        ApiResponse<object> response = ApiResponse.Fail<object>(message, statusCode, errorMessages);

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await JsonSerializer.SerializeAsync(context.Response.Body, response, SerializerOptions).ConfigureAwait(false);
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Warning, Message = "BadRequestException for {Method} {Path}")]
    private static partial void LogBadRequest(ILogger logger, Exception exception, string method, string? path);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Warning, Message = "NotFoundException for {Method} {Path}")]
    private static partial void LogNotFound(ILogger logger, Exception exception, string method, string? path);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Warning, Message = "UnauthorizedException for {Method} {Path}")]
    private static partial void LogUnauthorized(ILogger logger, Exception exception, string method, string? path);

    [LoggerMessage(EventId = 3004, Level = LogLevel.Warning, Message = "ForbiddenException for {Method} {Path}")]
    private static partial void LogForbidden(ILogger logger, Exception exception, string method, string? path);

    [LoggerMessage(EventId = 3005, Level = LogLevel.Error, Message = "InternalServerException for {Method} {Path}")]
    private static partial void LogInternalServer(ILogger logger, Exception exception, string method, string? path);

    [LoggerMessage(EventId = 3006, Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string method, string? path);
}
