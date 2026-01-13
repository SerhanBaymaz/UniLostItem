using FluentValidation;

namespace Application.Core.Pagination;

public class PagedAndSortedQueryValidator : AbstractValidator<PagedAndSortedQueryBase>
{
    public PagedAndSortedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası en az 1 olmalıdır");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır");

        // Feature-specific validators will override BeAValidSortField
        RuleFor(x => x.SortBy)
            .Must(BeAValidSortField)
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage("Geçersiz sıralama alanı");
    }

    protected virtual bool BeAValidSortField(string? sortBy) => true;
}
