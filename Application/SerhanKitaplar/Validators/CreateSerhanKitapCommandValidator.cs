using FluentValidation;
using Application.SerhanKitaplar.Commands;

namespace Application.SerhanKitaplar.Validators;

public class CreateSerhanKitapCommandValidator : AbstractValidator<CreateSerhanKitap.Command>
{
    public CreateSerhanKitapCommandValidator()
    {
        RuleFor(x => x.SerhanKitapDto)
            .NotNull().WithMessage("Kitap bilgileri gereklidir")
            .OverridePropertyName("Kitap");

        RuleFor(x => x.SerhanKitapDto.KitapName)
            .NotEmpty().WithMessage("Kitap adı boş olamaz")
            .When(x => x.SerhanKitapDto != null)
            .OverridePropertyName("KitapName");

        RuleFor(x => x.SerhanKitapDto.KitapYazar)
            .NotEmpty().WithMessage("Kitap yazarı boş olamaz")
            .When(x => x.SerhanKitapDto != null)
            .OverridePropertyName("KitapYazar");

        RuleFor(x => x.SerhanKitapDto.KitapSayfaSayisi)
            .NotEmpty().WithMessage("Kitap sayfa sayısı boş olamaz")
            .GreaterThan(0).WithMessage("Kitap sayfa sayısı 0'dan büyük olmalıdır")
            .When(x => x.SerhanKitapDto != null)
            .OverridePropertyName("KitapSayfaSayisi");
    }
}
