using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uganda.AdministrativeUnits.Application.Exceptions;
using Uganda.AdministrativeUnits.Contracts.Responses;

namespace Uganda.AdministrativeUnits.Application.Extensions;

/// <summary>
/// Executes service operations and maps unexpected failures to domain exceptions.
/// Domain exceptions are rethrown so <c>BadRequestExceptionMiddleware</c> can map status codes.
/// </summary>
public static partial class ExceptionHandlingExtensions
{
    public static Task<ApiResponse<T>> ExecuteWithExceptionHandlingAsync<T>(
        Func<Task<T>> func,
        ILogger logger,
        string errorMessage) =>
        RunAsync(async () =>
        {
            T result = await func().ConfigureAwait(false);
            return ApiResponse.Ok(result);
        }, logger, errorMessage);

    private static async Task<T> RunAsync<T>(
        Func<Task<T>> func,
        ILogger logger,
        string errorMessage)
    {
        try
        {
            return await func().ConfigureAwait(false);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (InternalServerException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            LogDatabaseFailure(logger, ex, errorMessage);
            throw new InternalServerException($"A database error occurred: {ex.ExtractInnerExceptionMessage()}");
        }
        catch (InvalidOperationException ex)
        {
            LogInvalidOperationFailure(logger, ex, errorMessage);
            throw new InternalServerException($"Operation error: {ex.ExtractInnerExceptionMessage()}");
        }
        catch (Exception ex)
        {
            LogUnexpectedFailure(logger, ex, errorMessage);
            throw new InternalServerException(ex.ExtractInnerExceptionMessage());
        }
    }

    public static string ExtractInnerExceptionMessage(this Exception ex)
    {
        Exception innerException = ex;
        while (innerException.InnerException is not null)
        {
            innerException = innerException.InnerException;
        }

        return string.IsNullOrWhiteSpace(innerException.Message)
            ? "An error occurred"
            : innerException.Message;
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Error, Message = "A database error occurred while executing a service operation. {ErrorMessage}")]
    private static partial void LogDatabaseFailure(ILogger logger, Exception exception, string errorMessage);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "An invalid operation occurred while executing a service operation. {ErrorMessage}")]
    private static partial void LogInvalidOperationFailure(ILogger logger, Exception exception, string errorMessage);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "An unexpected error occurred while executing a service operation. {ErrorMessage}")]
    private static partial void LogUnexpectedFailure(ILogger logger, Exception exception, string errorMessage);
}
