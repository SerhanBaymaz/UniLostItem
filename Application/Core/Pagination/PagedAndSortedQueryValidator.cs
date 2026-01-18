using FluentValidation;

namespace Application.Core.Pagination;

public class PagedAndSortedQueryValidator<TQuery, TSortEnum> : AbstractValidator<TQuery> 
    where TQuery : PagedAndSortedQueryBase<TSortEnum>
    where TSortEnum : struct, Enum
{
    public PagedAndSortedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası en az 1 olmalıdır");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır");
    }
}
