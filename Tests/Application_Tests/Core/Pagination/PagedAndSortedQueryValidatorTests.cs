using Application.Core.Pagination;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Tests.Application_Tests.Core.Pagination;

public class PagedAndSortedQueryValidatorTests
{
    private sealed record TestQuery : PagedAndSortedQueryBase
    {
        // Test implementation of the abstract base record
    }

    private readonly PagedAndSortedQueryValidator _validator;

    public PagedAndSortedQueryValidatorTests()
    {
        _validator = new PagedAndSortedQueryValidator();
    }

    [Fact]
    public void ShouldHaveValidationError_WhenPageNumber_IsLessThan1()
    {
        // Arrange
        var query = new TestQuery { PageNumber = 0 };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage("Sayfa numarası en az 1 olmalıdır");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void ShouldHaveValidationError_WhenPageNumber_IsNegative(int pageNumber)
    {
        // Arrange
        var query = new TestQuery { PageNumber = pageNumber };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ShouldNotHaveValidationError_WhenPageNumber_IsValid(int pageNumber)
    {
        // Arrange
        var query = new TestQuery { PageNumber = pageNumber };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void ShouldHaveValidationError_WhenPageSize_IsLessThan1()
    {
        // Arrange
        var query = new TestQuery { PageSize = 0 };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır");
    }

    [Fact]
    public void ShouldHaveValidationError_WhenPageSize_IsGreaterThan100()
    {
        // Arrange
        var query = new TestQuery { PageSize = 101 };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(200)]
    public void ShouldHaveValidationError_WhenPageSize_IsOutOfRange(int pageSize)
    {
        // Arrange
        var query = new TestQuery { PageSize = pageSize };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void ShouldNotHaveValidationError_WhenPageSize_IsValid(int pageSize)
    {
        // Arrange
        var query = new TestQuery { PageSize = pageSize };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenSortBy_IsEmpty()
    {
        // Arrange
        var query = new TestQuery { SortBy = null };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenSortBy_HasValue()
    {
        // Arrange
        var query = new TestQuery { SortBy = "name" };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenAllValuesAreValid()
    {
        // Arrange
        var query = new TestQuery
        {
            PageNumber = 2,
            PageSize = 20,
            SortBy = "name",
            SortDescending = true
        };

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenUsingDefaults()
    {
        // Arrange
        var query = new TestQuery(); // Uses defaults

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveMultipleValidationErrors_WhenMultipleValuesAreInvalid()
    {
        // Arrange
        var query = new TestQuery
        {
            PageNumber = 0,
            PageSize = 150
        };

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void VirtualBeAValidSortField_ReturnsTrue_ByDefault()
    {
        // Arrange
        var query = new TestQuery { SortBy = "anyField" };

        // Act & Assert
        // The virtual method in base class returns true by default
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }
}
