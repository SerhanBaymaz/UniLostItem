using FluentValidation;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.LoginDto)
            .NotNull().WithMessage("Login information is required.");

        RuleFor(x => x.LoginDto.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.")
            .When(x => x.LoginDto != null);

        RuleFor(x => x.LoginDto.Password)
            .NotEmpty().WithMessage("Password is required.")
            .When(x => x.LoginDto != null);
    }
}