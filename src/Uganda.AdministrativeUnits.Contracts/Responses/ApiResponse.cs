using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace Uganda.AdministrativeUnits.Contracts.Responses;

/// <summary>Standard API envelope for successful and failed responses.</summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public int StatusCode { get; init; }

    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Errors { get; init; }
}

/// <summary>Factory helpers for <see cref="ApiResponse{T}"/> (kept off the generic type for CA1000).</summary>
public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string message = "Success", HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new()
        {
            Success = true,
            Message = message,
            StatusCode = (int)statusCode,
            Data = data,
            Errors = null,
        };

    public static ApiResponse<T> Fail<T>(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        IReadOnlyList<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = (int)statusCode,
            Data = default,
            Errors = errors ?? [message],
        };
}
