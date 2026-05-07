using Application.Features.Auth.Commands.UpdateUserProfile;
using FluentValidation;

namespace Application.Features.Auth.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UpdateUserProfileDto.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateUserProfileDto.FirstName));

        RuleFor(x => x.UpdateUserProfileDto.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateUserProfileDto.LastName));

        RuleFor(x => x.UpdateUserProfileDto.PhoneNumber)
            .NotEmpty().WithMessage("Phone number cannot be empty.")
            .Matches(@"^\+\d+$").WithMessage("Phone number must start with + and contain only digits after")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateUserProfileDto.PhoneNumber));
    }
}
