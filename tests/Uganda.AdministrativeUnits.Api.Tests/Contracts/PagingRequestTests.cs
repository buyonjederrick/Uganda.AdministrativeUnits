using FluentAssertions;
using Uganda.AdministrativeUnits.Contracts.Requests;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Contracts;

public sealed class PagingRequestTests
{
    [Fact]
    public void Normalize_Should_ResetPage_When_PageIsLessThanOne()
    {
        // Arrange
        PagingRequest request = new() { Page = 0, PageSize = 10 };

        // Act
        request.Normalize();

        // Assert
        request.Page.Should().Be(PagingRequest.DefaultPage);
        request.PageSize.Should().Be(10);
    }

    [Fact]
    public void Normalize_Should_ResetPageSize_When_PageSizeIsLessThanOne()
    {
        // Arrange
        PagingRequest request = new() { Page = 2, PageSize = 0 };

        // Act
        request.Normalize();

        // Assert
        request.Page.Should().Be(2);
        request.PageSize.Should().Be(PagingRequest.DefaultPageSize);
    }

    [Fact]
    public void Normalize_Should_CapPageSize_When_PageSizeExceedsMaximum()
    {
        // Arrange
        PagingRequest request = new() { Page = 1, PageSize = 500 };

        // Act
        request.Normalize();

        // Assert
        request.PageSize.Should().Be(PagingRequest.MaxPageSize);
    }

    [Fact]
    public void Normalize_Should_PreserveGetAll_When_GetAllIsTrue()
    {
        // Arrange
        PagingRequest request = new() { Page = 1, PageSize = 50, GetAll = true };

        // Act
        request.Normalize();

        // Assert
        request.GetAll.Should().BeTrue();
    }
}
