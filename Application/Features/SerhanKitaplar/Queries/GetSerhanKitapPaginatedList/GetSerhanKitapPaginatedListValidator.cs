using Application.Core.Pagination;
using FluentValidation;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;

public class GetSerhanKitapPaginatedListValidator : PagedAndSortedQueryValidator
{
    protected override bool BeAValidSortField(string? sortBy)
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
