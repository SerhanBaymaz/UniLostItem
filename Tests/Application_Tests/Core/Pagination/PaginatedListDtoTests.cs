using Application.Core.Pagination;
using FluentAssertions;

namespace Tests.Application_Tests.Core.Pagination;

public class PaginatedListDtoTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var totalCount = 25;
        var pageNumber = 2;
        var pageSize = 10;

        // Act
        var paginatedList = new PaginatedListDto<string>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Assert
        paginatedList.Items.Should().BeEquivalentTo(items);
        paginatedList.TotalCount.Should().Be(totalCount);
        paginatedList.PageNumber.Should().Be(pageNumber);
        paginatedList.PageSize.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(25, 10, 3)]   // 25 items, 10 per page = 3 pages
    [InlineData(20, 10, 2)]   // 20 items, 10 per page = 2 pages
    [InlineData(15, 10, 2)]   // 15 items, 10 per page = 2 pages
    [InlineData(10, 10, 1)]   // 10 items, 10 per page = 1 page
    [InlineData(5, 10, 1)]    // 5 items, 10 per page = 1 page
    [InlineData(100, 25, 4)]  // 100 items, 25 per page = 4 pages
    public void TotalPages_WithVariousCounts_CalculatesCorrectly(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange & Act
        var paginatedList = new PaginatedListDto<string>
        {
            Items = new(),
            TotalCount = totalCount,
            PageNumber = 1,
            PageSize = pageSize
        };

        // Assert
        paginatedList.TotalPages.Should().Be(expectedTotalPages);
    }

    [Theory]
    [InlineData(1, false)]    // First page
    [InlineData(2, true)]     // Middle page
    [InlineData(5, true)]     // Later page
    public void HasPrevious_WithVariousPages_ReturnsCorrectValue(int pageNumber, bool expectedHasPrevious)
    {
        // Arrange & Act
        var paginatedList = new PaginatedListDto<string>
        {
            Items = new(),
            TotalCount = 50,
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Assert
        paginatedList.HasPrevious.Should().Be(expectedHasPrevious);
    }

    [Theory]
    [InlineData(1, 5, true)]    // Page 1 of 5 - has next
    [InlineData(2, 5, true)]    // Page 2 of 5 - has next
    [InlineData(4, 5, true)]    // Page 4 of 5 - has next
    [InlineData(5, 5, false)]   // Page 5 of 5 - no next (last page)
    public void HasNext_WithVariousPages_ReturnsCorrectValue(int pageNumber, int totalPages, bool expectedHasNext)
    {
        // Arrange & Act
        var paginatedList = new PaginatedListDto<string>
        {
            Items = new(),
            TotalCount = totalPages * 10,
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Assert
        paginatedList.HasNext.Should().Be(expectedHasNext);
    }

    [Fact]
    public void GetItems_ReturnsItemsCollection()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var paginatedList = new PaginatedListDto<int>
        {
            Items = items,
            TotalCount = 5,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = ((IPaginatedList)paginatedList).GetItems();

        // Assert
        result.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void DefaultConstructor_CreatesEmptyItemList()
    {
        // Arrange & Act
        var paginatedList = new PaginatedListDto<string>
        {
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        paginatedList.Items.Should().NotBeNull().And.BeEmpty();
    }
}
