using FluentAssertions;
using Uganda.AdministrativeUnits.Contracts.Requests;
using ContractsAdministrativeLevel = Uganda.AdministrativeUnits.Contracts.Enums.AdministrativeLevel;
using Xunit;

namespace Uganda.AdministrativeUnits.Api.Tests.Contracts;

public sealed class SearchRequestTests
{
    [Fact]
    public void Normalize_Should_TrimQuery_When_QueryHasWhitespace()
    {
        // Arrange
        SearchRequest request = new() { Query = "  hoima  ", MaxResults = 10 };

        // Act
        request.Normalize();

        // Assert
        request.Query.Should().Be("hoima");
    }

    [Fact]
    public void Normalize_Should_UseEmptyQuery_When_QueryIsNull()
    {
        // Arrange
        SearchRequest request = new() { Query = null!, MaxResults = 10 };

        // Act
        request.Normalize();

        // Assert
        request.Query.Should().BeEmpty();
    }

    [Fact]
    public void Normalize_Should_ResetMaxResults_When_MaxResultsIsLessThanOne()
    {
        // Arrange
        SearchRequest request = new() { Query = "x", MaxResults = 0 };

        // Act
        request.Normalize();

        // Assert
        request.MaxResults.Should().Be(SearchRequest.DefaultMaxResults);
    }

    [Fact]
    public void Normalize_Should_CapMaxResults_When_MaxResultsExceedsMaximum()
    {
        // Arrange
        SearchRequest request = new()
        {
            Query = "x",
            MaxResults = 500,
            Level = ContractsAdministrativeLevel.Village,
        };

        // Act
        request.Normalize();

        // Assert
        request.MaxResults.Should().Be(SearchRequest.MaxAllowedResults);
        request.Level.Should().Be(ContractsAdministrativeLevel.Village);
    }

    [Fact]
    public void Normalize_Should_TrimParentCode_When_ParentCodeHasWhitespace()
    {
        // Arrange
        SearchRequest request = new()
        {
            Query = "x",
            ParentCode = " 06-028 ",
        };

        // Act
        request.Normalize();

        // Assert
        request.ParentCode.Should().Be("06-028");
    }

    [Fact]
    public void Normalize_Should_NullParentCode_When_ParentCodeIsWhitespace()
    {
        // Arrange
        SearchRequest request = new()
        {
            Query = "x",
            ParentCode = "   ",
        };

        // Act
        request.Normalize();

        // Assert
        request.ParentCode.Should().BeNull();
    }
}
