using Application.Core;
using FluentValidation;

namespace Application.Features.ItemClaims.Commands.CreateItemClaim;

public class CreateItemClaimCommandValidator : AbstractValidator<CreateItemClaimCommand>
{
    public CreateItemClaimCommandValidator()
    {
        RuleFor(x => x.CreateItemClaimDto).NotNull().WithMessage("Talep bilgileri gereklidir");

        When(x => x.CreateItemClaimDto != null, () =>
        {
            RuleFor(x => x.CreateItemClaimDto!.LostItemId)
                .NotEmpty().WithMessage("İlan ID gereklidir");

            RuleFor(x => x.CreateItemClaimDto!.Description)
                .NotEmpty().WithMessage("Açıklama gereklidir")
                .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir");
        });
    }
}
