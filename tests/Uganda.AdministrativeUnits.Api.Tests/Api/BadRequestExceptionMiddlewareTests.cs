using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Uganda.AdministrativeUnits.Api.Middleware;
using Uganda.AdministrativeUnits.Application.Exceptions;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Api;

public sealed class BadRequestExceptionMiddlewareTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Theory]
    [InlineData(typeof(BadRequestException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(NotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(UnauthorizedException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(ForbiddenException), HttpStatusCode.Forbidden)]
    [InlineData(typeof(InternalServerException), HttpStatusCode.InternalServerError)]
    public async Task Invoke_Should_MapDomainException_When_HandlerThrows(Type exceptionType, HttpStatusCode expectedStatus)
    {
        // Arrange
        Exception exception = (Exception)Activator.CreateInstance(exceptionType, "problem")!;
        BadRequestExceptionMiddleware middleware = new(
            _ => throw exception,
            NullLogger<BadRequestExceptionMiddleware>.Instance);

        DefaultHttpContext context = CreateContext();

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)expectedStatus);
        ApiResponse<object?> payload = await ReadBodyAsync(context);
        payload.Success.Should().BeFalse();
        payload.StatusCode.Should().Be((int)expectedStatus);
        payload.Message.Should().Be("problem");
    }

    [Fact]
    public async Task Invoke_Should_ReturnInternalServerError_When_UnexpectedExceptionIsThrown()
    {
        // Arrange
        BadRequestExceptionMiddleware middleware = new(
            _ => throw new NotSupportedException("secret"),
            NullLogger<BadRequestExceptionMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        ApiResponse<object?> payload = await ReadBodyAsync(context);
        payload.Message.Should().Contain("unexpected error");
        payload.Message.Should().NotContain("secret");
    }

    [Fact]
    public async Task Invoke_Should_PassThrough_When_NoExceptionIsThrown()
    {
        // Arrange
        bool called = false;
        BadRequestExceptionMiddleware middleware = new(
            async context =>
            {
                called = true;
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                await Task.CompletedTask;
            },
            NullLogger<BadRequestExceptionMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        // Act
        await middleware.Invoke(context);

        // Assert
        called.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    private static DefaultHttpContext CreateContext()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ApiResponse<object?>> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return (await JsonSerializer.DeserializeAsync<ApiResponse<object?>>(
            context.Response.Body,
            SerializerOptions))!;
    }
}
