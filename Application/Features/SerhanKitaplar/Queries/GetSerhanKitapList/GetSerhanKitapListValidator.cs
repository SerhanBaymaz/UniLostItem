using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Queries.Common.Enums;
using FluentValidation;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public class GetSerhanKitapListValidator : PagedAndSortedQueryValidator<GetSerhanKitapListQuery, KitapSortField>
{
    public GetSerhanKitapListValidator()
    {
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
}
