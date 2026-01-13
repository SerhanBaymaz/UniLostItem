using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;

public class GetSerhanKitapPaginatedListValidatorTests
{
    private readonly GetSerhanKitapPaginatedListValidator _validator;

    public GetSerhanKitapPaginatedListValidatorTests()
    {
        _validator = new GetSerhanKitapPaginatedListValidator();
    }

    [Fact]
    public void ShouldHaveValidationError_WhenPageNumber_IsLessThan1()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { PageNumber = 0 };

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
        var query = new GetSerhanKitapPaginatedListQuery { PageNumber = pageNumber };

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
        var query = new GetSerhanKitapPaginatedListQuery { PageNumber = pageNumber };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void ShouldHaveValidationError_WhenPageSize_IsLessThan1()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { PageSize = 0 };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır");
    }

    [Fact]
    public void ShouldHaveValidationError_WhenPageSize_IsGreaterThan100()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { PageSize = 101 };

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
        var query = new GetSerhanKitapPaginatedListQuery { PageSize = pageSize };

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
        var query = new GetSerhanKitapPaginatedListQuery { PageSize = pageSize };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("kitapname")]
    [InlineData("name")]
    [InlineData("ad")]
    [InlineData("kitapyazar")]
    [InlineData("yazar")]
    [InlineData("author")]
    [InlineData("kitapsayfasayisi")]
    [InlineData("sayfasayisi")]
    [InlineData("pagecount")]
    [InlineData("KITAPNAME")]
    [InlineData("KitapName")]
    public void ShouldNotHaveValidationError_WhenSortBy_IsValid(string sortBy)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { SortBy = sortBy };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Theory]
    [InlineData("invalidfield")]
    [InlineData("xyz")]
    [InlineData("description")]
    [InlineData("price")]
    public void ShouldHaveValidationError_WhenSortBy_IsInvalid(string sortBy)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { SortBy = sortBy };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.SortBy)
            .WithErrorMessage("Geçersiz sıralama alanı");
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenSortBy_IsEmpty()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { SortBy = null };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenAllValuesAreValid()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery
        {
            PageNumber = 2,
            PageSize = 20,
            SortBy = "kitapname",
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
        var query = new GetSerhanKitapPaginatedListQuery(); // Uses defaults

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldHaveMultipleValidationErrors_WhenMultipleValuesAreInvalid()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery
        {
            PageNumber = 0,
            PageSize = 150,
            SortBy = "invalidfield"
        };

        // Act & Assert
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    #region Filter Validation Tests

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void ShouldHaveValidationError_WhenMinPageCount_IsNegative(int minPageCount)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { MinPageCount = minPageCount };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.MinPageCount)
            .WithErrorMessage("Minimum sayfa sayısı 0 veya daha büyük olmalıdır");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void ShouldHaveValidationError_WhenMaxPageCount_IsNegative(int maxPageCount)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { MaxPageCount = maxPageCount };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.MaxPageCount)
            .WithErrorMessage("Maksimum sayfa sayısı 0 veya daha büyük olmalıdır");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ShouldNotHaveValidationError_WhenMinPageCount_IsValid(int minPageCount)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { MinPageCount = minPageCount };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.MinPageCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ShouldNotHaveValidationError_WhenMaxPageCount_IsValid(int maxPageCount)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { MaxPageCount = maxPageCount };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.MaxPageCount);
    }

    [Fact]
    public void ShouldHaveValidationError_WhenMaxPageCount_IsLessThanMinPageCount()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery
        {
            MinPageCount = 100,
            MaxPageCount = 50
        };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.MaxPageCount)
            .WithErrorMessage("Maksimum sayfa sayısı minimum sayfa sayısından büyük veya eşit olmalıdır");
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(100, 150)]
    [InlineData(100, 200)]
    [InlineData(0, 0)]
    [InlineData(0, 1000)]
    public void ShouldNotHaveValidationError_WhenMaxPageCount_IsGreaterOrEqualThanMinPageCount(int minPageCount, int maxPageCount)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery
        {
            MinPageCount = minPageCount,
            MaxPageCount = maxPageCount
        };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.MaxPageCount);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(100, null)]
    [InlineData(null, 500)]
    public void ShouldNotHaveValidationError_WhenPageCount_IsPartiallyNull(int? minPageCount, int? maxPageCount)
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery
        {
            MinPageCount = minPageCount,
            MaxPageCount = maxPageCount
        };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.MinPageCount);
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.MaxPageCount);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenSearchTerm_IsProvided()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { SearchTerm = "test" };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenKitapName_IsProvided()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { KitapName = "Book" };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.KitapName);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenKitapYazar_IsProvided()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery { KitapYazar = "Author" };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveValidationErrorFor(x => x.KitapYazar);
    }

    [Fact]
    public void ShouldNotHaveValidationError_WhenAllFiltersAreValid()
    {
        // Arrange
        var query = new GetSerhanKitapPaginatedListQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "kitapname",
            SearchTerm = "test",
            KitapName = "Book",
            KitapYazar = "Author",
            MinPageCount = 100,
            MaxPageCount = 500
        };

        // Act & Assert
        _validator.TestValidate(query)
            .ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
