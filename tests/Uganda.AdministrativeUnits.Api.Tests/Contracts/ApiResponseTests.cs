using System.Net;
using FluentAssertions;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Contracts;

public sealed class ApiResponseTests
{
    [Fact]
    public void Ok_Should_SetSuccessEnvelope_When_DataIsProvided()
    {
        // Arrange / Act
        ApiResponse<string> response = ApiResponse.Ok("payload", "Done", HttpStatusCode.Created);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Done");
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        response.Data.Should().Be("payload");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Fail_Should_SetFailureEnvelope_When_MessageIsProvided()
    {
        // Arrange / Act
        ApiResponse<object> response = ApiResponse.Fail<object>("broken", HttpStatusCode.BadRequest);

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("broken");
        response.StatusCode.Should().Be(400);
        response.Data.Should().BeNull();
        response.Errors.Should().ContainSingle().Which.Should().Be("broken");
    }

    [Fact]
    public void Fail_Should_UseProvidedErrors_When_ErrorsAreSupplied()
    {
        // Arrange / Act
        ApiResponse<object> response = ApiResponse.Fail<object>(
            "Multiple errors",
            HttpStatusCode.BadRequest,
            ["a", "b"]);

        // Assert
        response.Errors.Should().Equal("a", "b");
    }
}
