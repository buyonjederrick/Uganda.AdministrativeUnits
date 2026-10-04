using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Uganda.AdministrativeUnits.Api.Authentication;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Api;

public sealed class ApiKeyAuthenticationHandlerTests
{
    [Fact]
    public async Task HandleAuthenticateAsync_Should_Succeed_When_ValidApiKeyIsProvided()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Headers[ApiKeyAuthenticationOptions.HeaderName] = "valid-key";
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context, ["valid-key"]);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Principal!.FindFirstValue(ClaimTypes.Name).Should().Be("ApiKeyClient");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_Fail_When_HeaderIsMissing()
    {
        // Arrange
        DefaultHttpContext context = new();
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context, ["valid-key"]);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Contain("missing");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_Fail_When_HeaderIsEmpty()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Headers[ApiKeyAuthenticationOptions.HeaderName] = "   ";
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context, ["valid-key"]);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Contain("empty");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_Fail_When_NoKeysAreConfigured()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Headers[ApiKeyAuthenticationOptions.HeaderName] = "any";
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context, []);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Contain("No API keys");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Should_Fail_When_ApiKeyIsInvalid()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Headers[ApiKeyAuthenticationOptions.HeaderName] = "wrong";
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context, ["valid-key"]);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure!.Message.Should().Contain("Invalid");
    }

    private static async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(
        HttpContext context,
        string[] apiKeys)
    {
        Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>> optionsMonitor = new();
        optionsMonitor.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions
        {
            ApiKeys = apiKeys,
        });
        optionsMonitor.Setup(x => x.CurrentValue).Returns(new ApiKeyAuthenticationOptions
        {
            ApiKeys = apiKeys,
        });

        ApiKeyAuthenticationHandler handler = new(
            optionsMonitor.Object,
            NullLoggerFactory.Instance,
            UrlEncoder.Default);

        await handler.InitializeAsync(
            new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)),
            context);

        return handler;
    }
}
