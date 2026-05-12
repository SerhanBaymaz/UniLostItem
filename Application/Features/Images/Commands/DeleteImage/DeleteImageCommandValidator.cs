using FluentValidation;

namespace Application.Features.Images.Commands.DeleteImage;

public class DeleteImageCommandValidator : AbstractValidator<DeleteImageCommand>
{
    public DeleteImageCommandValidator()
    {
        RuleFor(x => x.PublicId)
            .NotEmpty().WithMessage("Görsel kimliği (PublicId) boş olamaz");
    }
}
