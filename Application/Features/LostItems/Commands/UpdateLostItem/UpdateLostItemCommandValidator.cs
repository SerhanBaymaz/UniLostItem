using FluentValidation;

namespace Application.Features.LostItems.Commands.UpdateLostItem;

public class UpdateLostItemCommandValidator : AbstractValidator<UpdateLostItemCommand>
{
    public UpdateLostItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item ID boş olamaz");

        RuleFor(x => x.UpdateLostItemDto)
            .NotNull().WithMessage("Item bilgileri gereklidir")
            .OverridePropertyName("Item");

        RuleFor(x => x.UpdateLostItemDto.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .When(x => x.UpdateLostItemDto != null);

        RuleFor(x => x.UpdateLostItemDto.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz")
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir")
            .When(x => x.UpdateLostItemDto != null);

        RuleFor(x => x.UpdateLostItemDto.Category)
            .IsInEnum().WithMessage("Geçersiz kategori")
            .When(x => x.UpdateLostItemDto != null);

        RuleFor(x => x.UpdateLostItemDto.IncidentDate)
            .NotEmpty().WithMessage("Olay tarihi boş olamaz")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Olay tarihi gelecekte olamaz")
            .When(x => x.UpdateLostItemDto != null);

        RuleFor(x => x.UpdateLostItemDto.ImageUrl)
            .MaximumLength(500).WithMessage("Resim URL en fazla 500 karakter olabilir")
            .When(x => x.UpdateLostItemDto != null && !string.IsNullOrEmpty(x.UpdateLostItemDto.ImageUrl));

        RuleFor(x => x.UpdateLostItemDto.ContactInfo)
            .NotEmpty().WithMessage("İletişim bilgisi boş olamaz")
            .MaximumLength(300).WithMessage("İletişim bilgisi en fazla 300 karakter olabilir")
            .When(x => x.UpdateLostItemDto != null);

        RuleFor(x => x.UpdateLostItemDto.LocationLabel)
            .NotEmpty().WithMessage("Konum açıklaması boş olamaz")
            .MaximumLength(300).WithMessage("Konum açıklaması en fazla 300 karakter olabilir")
            .When(x => x.UpdateLostItemDto != null);

        RuleFor(x => x.UpdateLostItemDto.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Enlem -90 ile 90 arasında olmalıdır")
            .When(x => x.UpdateLostItemDto != null);

        RuleFor(x => x.UpdateLostItemDto.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Boylam -180 ile 180 arasında olmalıdır")
            .When(x => x.UpdateLostItemDto != null);
    }
}
