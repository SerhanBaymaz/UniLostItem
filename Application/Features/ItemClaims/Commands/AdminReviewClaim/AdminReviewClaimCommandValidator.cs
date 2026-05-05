using Application.Core;
using FluentValidation;

namespace Application.Features.ItemClaims.Commands.AdminReviewClaim;

public class AdminReviewClaimCommandValidator : AbstractValidator<AdminReviewClaimCommand>
{
    public AdminReviewClaimCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Talep ID gereklidir");

        RuleFor(x => x.AdminReviewClaimDto).NotNull().WithMessage("Admin değerlendirme bilgileri gereklidir");

        When(x => x.AdminReviewClaimDto != null, () =>
        {
            RuleFor(x => x.AdminReviewClaimDto!.Comment)
                .MaximumLength(500).WithMessage("Yorum en fazla 500 karakter olabilir");
        });
    }
}
