using Application.Core.Pagination;
using FluentAssertions;

namespace Tests.Application_Tests.Core.Pagination;

public class PagedAndSortedQueryBaseTests
{
    public enum TestSortField { Name, CreatedDate, Price }

    private sealed record TestQuery : PagedAndSortedQueryBase<TestSortField>
    {
        // Test implementation of the abstract base record
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var query = new TestQuery();

        // Assert
        query.PageNumber.Should().Be(1);
        query.PageSize.Should().Be(10);
        query.SortBy.Should().BeNull();
        query.SortDescending.Should().BeFalse();
    }

    [Fact]
    public void CanOverrideDefaultValues()
    {
        // Arrange & Act
        var query = new TestQuery
        {
            PageNumber = 3,
            PageSize = 25,
            SortBy = TestSortField.Name,
            SortDescending = true
        };

        // Assert
        query.PageNumber.Should().Be(3);
        query.PageSize.Should().Be(25);
        query.SortBy.Should().Be(TestSortField.Name);
        query.SortDescending.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(5, 20)]
    [InlineData(10, 50)]
    [InlineData(100, 100)]
    public void CanSetVariousPageNumberAndPageSize(int pageNumber, int pageSize)
    {
        // Arrange & Act
        var query = new TestQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Assert
        query.PageNumber.Should().Be(pageNumber);
        query.PageSize.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(TestSortField.Name)]
    [InlineData(TestSortField.CreatedDate)]
    [InlineData(TestSortField.Price)]
    [InlineData(null)]
    public void CanSetVariousSortByValues(TestSortField? sortBy)
    {
        // Arrange & Act
        var query = new TestQuery
        {
            SortBy = sortBy
        };

        // Assert
        query.SortBy.Should().Be(sortBy);
    }

    [Fact]
    public void RecordType_ProvidesValueEquality()
    {
        // Arrange
        var query1 = new TestQuery
        {
            PageNumber = 2,
            PageSize = 20,
            SortBy = TestSortField.Name,
            SortDescending = true
        };

        var query2 = new TestQuery
        {
            PageNumber = 2,
            PageSize = 20,
            SortBy = TestSortField.Name,
            SortDescending = true
        };

        var query3 = new TestQuery
        {
            PageNumber = 3,
            PageSize = 20,
            SortBy = TestSortField.Name,
            SortDescending = true
        };

        // Assert
        query1.Should().Be(query2);  // Same values
        query1.Should().NotBe(query3); // Different PageNumber
    }

    [Fact]
    public void RecordType_WithExpressionInit_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var query = new TestQuery
        {
            PageNumber = 5,
            PageSize = 15,
            SortBy = TestSortField.CreatedDate,
            SortDescending = false
        };

        // Assert
        query.PageNumber.Should().Be(5);
        query.PageSize.Should().Be(15);
        query.SortBy.Should().Be(TestSortField.CreatedDate);
        query.SortDescending.Should().BeFalse();
    }
}
