using FluentValidation;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.RegisterDto)
            .NotNull().WithMessage("Registration information is required.");

        RuleFor(x => x.RegisterDto.Email)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("A valid email address is required")
            .When(x => x.RegisterDto != null);

        RuleFor(x => x.RegisterDto.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character")
            .When(x => x.RegisterDto != null);

        RuleFor(x => x.RegisterDto.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .When(x => x.RegisterDto != null);

        RuleFor(x => x.RegisterDto.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .When(x => x.RegisterDto != null);
    }
}