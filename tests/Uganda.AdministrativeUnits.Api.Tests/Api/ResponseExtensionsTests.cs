using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Uganda.AdministrativeUnits.Api.Extensions;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Api;

public sealed class ResponseExtensionsTests
{
    [Fact]
    public void ToActionResult_Should_UseEnvelopeStatusCode_When_ResponseIsSuccessful()
    {
        // Arrange
        ApiResponse<string> response = ApiResponse.Ok("ok", statusCode: System.Net.HttpStatusCode.Created);

        // Act
        IActionResult result = response.ToActionResult();

        // Assert
        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
        objectResult.Value.Should().BeSameAs(response);
    }

    [Fact]
    public void ToActionResult_Should_UseEnvelopeStatusCode_When_ResponseIsFailure()
    {
        // Arrange
        ApiResponse<object> response = ApiResponse.Fail<object>("nope", System.Net.HttpStatusCode.NotFound);

        // Act
        IActionResult result = response.ToActionResult();

        // Assert
        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(404);
    }
}
