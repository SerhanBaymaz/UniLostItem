using FluentValidation;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public class GetSerhanKitapListValidator : AbstractValidator<GetSerhanKitapListQuery>
{
    public GetSerhanKitapListValidator()
    {
        // Pagination and sorting validation
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası en az 1 olmalıdır");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır");

        RuleFor(x => x.SortBy)
            .Must(BeAValidSortField)
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage("Geçersiz sıralama alanı");

        // Filter validation
        RuleFor(x => x.MinPageCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPageCount.HasValue)
            .WithMessage("Minimum sayfa sayısı 0 veya daha büyük olmalıdır");

        RuleFor(x => x.MaxPageCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPageCount.HasValue)
            .WithMessage("Maksimum sayfa sayısı 0 veya daha büyük olmalıdır");

        RuleFor(x => x.MaxPageCount)
            .GreaterThanOrEqualTo(x => x.MinPageCount)
            .When(x => x.MinPageCount.HasValue && x.MaxPageCount.HasValue)
            .WithMessage("Maksimum sayfa sayısı minimum sayfa sayısından büyük veya eşit olmalıdır");
    }

    private static bool BeAValidSortField(string? sortBy)
    {
        var validFields = new[]
        {
            "kitapname", "name", "ad",
            "kitapyazar", "yazar", "author",
            "kitapsayfasayisi", "sayfasayisi", "pagecount"
        };
        return validFields.Contains(sortBy?.ToLowerInvariant());
    }
}
