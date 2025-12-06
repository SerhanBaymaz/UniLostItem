using FluentValidation;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;

namespace Application.Features.SerhanKitaplar.Validators;

public class EditSerhanKitapCommandValidator : AbstractValidator<EditSerhanKitapCommand>
{
    public EditSerhanKitapCommandValidator()
    {
        RuleFor(x => x.EditSerhanKitapDto)
            .NotNull().WithMessage("Kitap bilgileri gereklidir")
            .OverridePropertyName("Kitap");

        RuleFor(x => x.EditSerhanKitapDto.KitapName)
            .NotEmpty().WithMessage("Kitap adı boş olamaz")
            .When(x => x.EditSerhanKitapDto != null)
            .OverridePropertyName("KitapName");

        RuleFor(x => x.EditSerhanKitapDto.KitapYazar)
            .NotEmpty().WithMessage("Kitap yazarı boş olamaz")
            .When(x => x.EditSerhanKitapDto != null)
            .OverridePropertyName("KitapYazar");

        RuleFor(x => x.EditSerhanKitapDto.KitapSayfaSayisi)
            .NotEmpty().WithMessage("Kitap sayfa sayısı boş olamaz")
            .GreaterThan(0).WithMessage("Kitap sayfa sayısı 0'dan büyük olmalıdır")
            .When(x => x.EditSerhanKitapDto != null)
            .OverridePropertyName("KitapSayfaSayisi");
    }
}
