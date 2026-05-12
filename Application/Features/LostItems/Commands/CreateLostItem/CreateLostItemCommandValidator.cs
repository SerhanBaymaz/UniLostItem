using FluentValidation;

namespace Application.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemCommandValidator : AbstractValidator<CreateLostItemCommand>
{
    public CreateLostItemCommandValidator()
    {
        RuleFor(x => x.CreateLostItemDto)
            .NotNull().WithMessage("Item bilgileri gereklidir")
            .OverridePropertyName("Item");

        RuleFor(x => x.CreateLostItemDto.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz")
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.Category)
            .IsInEnum().WithMessage("Geçersiz kategori")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.ItemType)
            .IsInEnum().WithMessage("Geçersiz item tipi")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.IncidentDate)
            .NotEmpty().WithMessage("Olay tarihi boş olamaz")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(2)).WithMessage("Olay tarihi gelecekte olamaz")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.ImageFileName)
            .Must(fileName =>
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return true;
                }
                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                return new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext);
            })
            .WithMessage("Yalnızca JPG, JPEG, PNG ve WEBP formatları desteklenmektedir")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.ImageStream)
            .Must(stream => stream == null || stream.Length <= 20 * 1024 * 1024)
            .WithMessage("Görsel boyutu en fazla 20 MB olabilir")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.ContactInfo)
            .NotEmpty().WithMessage("İletişim bilgisi boş olamaz")
            .MaximumLength(300).WithMessage("İletişim bilgisi en fazla 300 karakter olabilir")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.LocationLabel)
            .NotEmpty().WithMessage("Konum açıklaması boş olamaz")
            .MaximumLength(300).WithMessage("Konum açıklaması en fazla 300 karakter olabilir")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Enlem -90 ile 90 arasında olmalıdır")
            .When(x => x.CreateLostItemDto != null);

        RuleFor(x => x.CreateLostItemDto.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Boylam -180 ile 180 arasında olmalıdır")
            .When(x => x.CreateLostItemDto != null);
    }
}
