using FluentAssertions;
using Uganda.AdministrativeUnits.Contracts.Responses;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Contracts;

public sealed class PagedResultTests
{
    [Fact]
    public void TotalPages_Should_RoundUp_When_ItemsRemain()
    {
        // Arrange
        PagedResult<int> result = new()
        {
            Items = [1, 2],
            Page = 1,
            PageSize = 2,
            TotalCount = 5,
        };

        // Act / Assert
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void TotalPages_Should_BeZero_When_PageSizeIsInvalid()
    {
        // Arrange
        PagedResult<int> result = new()
        {
            Page = 1,
            PageSize = 0,
            TotalCount = 10,
        };

        // Act / Assert
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_Should_BeTrue_When_PageIsGreaterThanOne()
    {
        // Arrange
        PagedResult<int> result = new()
        {
            Page = 2,
            PageSize = 10,
            TotalCount = 20,
        };

        // Act / Assert
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }
}
