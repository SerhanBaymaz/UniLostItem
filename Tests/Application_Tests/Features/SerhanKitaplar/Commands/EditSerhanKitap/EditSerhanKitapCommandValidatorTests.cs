using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Commands.EditSerhanKitap;

public class EditSerhanKitapCommandValidatorTests
{
    private readonly EditSerhanKitapCommandValidator _validator;

    public EditSerhanKitapCommandValidatorTests()
    {
        _validator = new EditSerhanKitapCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Kitap_Is_Null()
    {
        var command = new EditSerhanKitapCommand { Id = "1", EditSerhanKitapDto = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Kitap")
            .WithErrorMessage("Kitap bilgileri gereklidir");
    }

    [Fact]
    public void Should_Have_Error_When_KitapName_Is_Empty()
    {
        var command = new EditSerhanKitapCommand
        {
            Id = "1",
            EditSerhanKitapDto = new EditSerhanKitapDto
            {
                KitapName = "",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 10
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("KitapName")
            .WithErrorMessage("Kitap adı boş olamaz");
    }

    [Fact]
    public void Should_Have_Error_When_KitapYazar_Is_Empty()
    {
        var command = new EditSerhanKitapCommand
        {
            Id = "1",
            EditSerhanKitapDto = new EditSerhanKitapDto
            {
                KitapName = "Kitap",
                KitapYazar = "",
                KitapSayfaSayisi = 10
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("KitapYazar")
            .WithErrorMessage("Kitap yazarı boş olamaz");
    }

    [Fact]
    public void Should_Have_Error_When_KitapSayfaSayisi_Is_Zero_Or_Negative()
    {
        var command = new EditSerhanKitapCommand
        {
            Id = "1",
            EditSerhanKitapDto = new EditSerhanKitapDto
            {
                KitapName = "Kitap",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 0
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("KitapSayfaSayisi")
            .WithErrorMessage("Kitap sayfa sayısı 0'dan büyük olmalıdır");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Valid()
    {
        var command = new EditSerhanKitapCommand
        {
            Id = "1",
            EditSerhanKitapDto = new EditSerhanKitapDto
            {
                KitapName = "Kitap",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 10
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
