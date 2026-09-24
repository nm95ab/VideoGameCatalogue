using FluentAssertions;
using VideoGameCatalogue.Domain.Common;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Domain;

public class PagedResultTests
{
    [Theory]
    [InlineData(25, 10, 3)]
    [InlineData(20, 10, 2)]
    [InlineData(0, 10, 0)]
    [InlineData(5, 10, 1)]
    [InlineData(10, 0, 0)]
    public void TotalPages_ShouldCalculateCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        var items = new List<string>();
        var result = new PagedResult<string>(items, 1, pageSize, totalCount);

        result.TotalPages.Should().Be(expectedPages);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    public void HasPreviousPage_ShouldBeTrueOnlyWhenPageGreaterThanOne(int pageNumber, bool expectedHasPrevious)
    {
        var result = new PagedResult<string>([], pageNumber, 10, 50);

        result.HasPreviousPage.Should().Be(expectedHasPrevious);
    }

    [Theory]
    [InlineData(1, 30, 10, true)]
    [InlineData(2, 30, 10, true)]
    [InlineData(3, 30, 10, false)]
    [InlineData(4, 30, 10, false)]
    public void HasNextPage_ShouldBeTrueWhenPageLessThanTotalPages(int pageNumber, int totalCount, int pageSize, bool expectedHasNext)
    {
        var result = new PagedResult<string>([], pageNumber, pageSize, totalCount);

        result.HasNextPage.Should().Be(expectedHasNext);
    }
}
