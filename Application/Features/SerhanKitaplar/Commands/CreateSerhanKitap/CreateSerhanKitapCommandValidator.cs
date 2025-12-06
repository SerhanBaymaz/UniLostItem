using FluentValidation;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;

namespace Application.Features.SerhanKitaplar.Validators;

public class CreateSerhanKitapCommandValidator : AbstractValidator<CreateSerhanKitapCommand>
{
    public CreateSerhanKitapCommandValidator()
    {
        RuleFor(x => x.CreateSerhanKitapDto)
            .NotNull().WithMessage("Kitap bilgileri gereklidir")
            .OverridePropertyName("Kitap");

        RuleFor(x => x.CreateSerhanKitapDto.KitapName)
            .NotEmpty().WithMessage("Kitap adı boş olamaz")
            .When(x => x.CreateSerhanKitapDto != null)
            .OverridePropertyName("KitapName");

        RuleFor(x => x.CreateSerhanKitapDto.KitapYazar)
            .NotEmpty().WithMessage("Kitap yazarı boş olamaz")
            .When(x => x.CreateSerhanKitapDto != null)
            .OverridePropertyName("KitapYazar");

        RuleFor(x => x.CreateSerhanKitapDto.KitapSayfaSayisi)
            .NotEmpty().WithMessage("Kitap sayfa sayısı boş olamaz")
            .GreaterThan(0).WithMessage("Kitap sayfa sayısı 0'dan büyük olmalıdır")
            .When(x => x.CreateSerhanKitapDto != null)
            .OverridePropertyName("KitapSayfaSayisi");
    }
}
