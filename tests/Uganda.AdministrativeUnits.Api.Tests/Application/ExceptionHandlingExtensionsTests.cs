using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Uganda.AdministrativeUnits.Application.Exceptions;
using Uganda.AdministrativeUnits.Application.Extensions;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Application;

public sealed class ExceptionHandlingExtensionsTests
{
    private readonly Mock<ILogger> _logger = new();

    [Fact]
    public async Task ExecuteWithExceptionHandlingAsync_Should_ReturnOkEnvelope_When_OperationSucceeds()
    {
        // Act
        ApiResponse<int> response = await ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync(
            () => Task.FromResult(42),
            _logger.Object,
            "failed");

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(42);
        response.StatusCode.Should().Be(200);
    }

    [Theory]
    [InlineData(typeof(NotFoundException))]
    [InlineData(typeof(BadRequestException))]
    [InlineData(typeof(ForbiddenException))]
    [InlineData(typeof(UnauthorizedException))]
    [InlineData(typeof(InternalServerException))]
    public async Task ExecuteWithExceptionHandlingAsync_Should_Rethrow_When_DomainExceptionIsThrown(Type exceptionType)
    {
        // Arrange
        Exception exception = (Exception)Activator.CreateInstance(exceptionType, "domain")!;

        // Act
        Func<Task> act = async () => await ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync<int>(
            () => Task.FromException<int>(exception),
            _logger.Object,
            "failed");

        // Assert
        await act.Should().ThrowAsync<Exception>().Where(ex => ex.GetType() == exceptionType);
    }

    [Fact]
    public async Task ExecuteWithExceptionHandlingAsync_Should_WrapAsInternalServerException_When_DbUpdateExceptionIsThrown()
    {
        // Arrange
        DbUpdateException dbException = new("db failed", new InvalidOperationException("inner"));

        // Act
        Func<Task> act = async () => await ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync<int>(
            () => Task.FromException<int>(dbException),
            _logger.Object,
            "failed for district '06'");

        // Assert
        (await act.Should().ThrowAsync<InternalServerException>())
            .Which.Message.Should().Contain("database error");
    }

    [Fact]
    public async Task ExecuteWithExceptionHandlingAsync_Should_WrapAsInternalServerException_When_InvalidOperationExceptionIsThrown()
    {
        // Act
        Func<Task> act = async () => await ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync<int>(
            () => Task.FromException<int>(new InvalidOperationException("invalid")),
            _logger.Object,
            "failed");

        // Assert
        (await act.Should().ThrowAsync<InternalServerException>())
            .Which.Message.Should().Contain("Operation error");
    }

    [Fact]
    public async Task ExecuteWithExceptionHandlingAsync_Should_WrapAsInternalServerException_When_UnexpectedExceptionIsThrown()
    {
        // Act
        Func<Task> act = async () => await ExceptionHandlingExtensions.ExecuteWithExceptionHandlingAsync<int>(
            () => Task.FromException<int>(new NotSupportedException("boom")),
            _logger.Object,
            "failed");

        // Assert
        (await act.Should().ThrowAsync<InternalServerException>())
            .Which.Message.Should().Be("boom");
    }

    [Fact]
    public void ExtractInnerExceptionMessage_Should_ReturnInnermostMessage()
    {
        // Arrange
        Exception exception = new InvalidOperationException(
            "outer",
            new InvalidOperationException("mid", new InvalidOperationException("inner")));

        // Act
        string message = exception.ExtractInnerExceptionMessage();

        // Assert
        message.Should().Be("inner");
    }

    [Fact]
    public void ExtractInnerExceptionMessage_Should_ReturnFallback_When_MessageIsWhitespace()
    {
        // Arrange
        Exception exception = new InvalidOperationException("   ");

        // Act
        string message = exception.ExtractInnerExceptionMessage();

        // Assert
        message.Should().Be("An error occurred");
    }
}
