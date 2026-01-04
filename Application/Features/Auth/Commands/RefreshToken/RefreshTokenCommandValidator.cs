using FluentValidation;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshTokenDto)
            .NotNull().WithMessage("Refresh token information is required.");

        RuleFor(x => x.RefreshTokenDto.AccessToken)
            .NotEmpty().WithMessage("Access token is required.")
            .When(x => x.RefreshTokenDto != null);

        RuleFor(x => x.RefreshTokenDto.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .When(x => x.RefreshTokenDto != null);
    }
}
