using FluentValidation;

namespace Application.Features.Images.Commands.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    public UploadImageCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Dosya adı boş olamaz")
            .Must(fileName =>
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                return new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext);
            })
            .WithMessage("Yalnızca JPG, JPEG, PNG ve WEBP formatları desteklenmektedir");

        RuleFor(x => x.ImageStream)
            .NotNull().WithMessage("Görsel dosyası gereklidir")
            .Must(stream => stream != null && stream.Length > 0).WithMessage("Görsel dosyası boş olamaz")
            .Must(stream => stream == null || stream.Length <= 20 * 1024 * 1024).WithMessage("Görsel boyutu en fazla 20 MB olabilir");
    }
}
