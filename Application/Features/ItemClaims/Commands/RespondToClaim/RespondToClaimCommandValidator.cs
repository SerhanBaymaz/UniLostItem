using Application.Core;
using FluentValidation;

namespace Application.Features.ItemClaims.Commands.RespondToClaim;

public class RespondToClaimCommandValidator : AbstractValidator<RespondToClaimCommand>
{
    public RespondToClaimCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Talep ID gereklidir");

        RuleFor(x => x.RespondToClaimDto).NotNull().WithMessage("Yanıt bilgileri gereklidir");

        When(x => x.RespondToClaimDto != null, () =>
        {
            RuleFor(x => x.RespondToClaimDto!.Comment)
                .MaximumLength(500).WithMessage("Yorum en fazla 500 karakter olabilir");
        });
    }
}
