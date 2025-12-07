using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Validators;
using FluentAssertions;

namespace Tests.Features.SerhanKitaplar.Commands;

public class CreateSerhanKitapCommandValidatorTests
{
    private readonly CreateSerhanKitapCommandValidator _validator;

    public CreateSerhanKitapCommandValidatorTests()
    {
        _validator = new CreateSerhanKitapCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "Geçerli Kitap",
                KitapYazar = "Geçerli Yazar",
                KitapSayfaSayisi = 100
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullDto_ShouldFail()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = null!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Kitap bilgileri gereklidir");
    }

    [Fact]
    public void Validate_WithEmptyKitapName_ShouldFail()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 100
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Kitap adı boş olamaz");
    }

    [Fact]
    public void Validate_WithEmptyKitapYazar_ShouldFail()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "Kitap",
                KitapYazar = "",
                KitapSayfaSayisi = 100
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Kitap yazarı boş olamaz");
    }

    [Fact]
    public void Validate_WithZeroSayfaSayisi_ShouldFail()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "Kitap",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 0
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kitap sayfa sayısı 0'dan büyük olmalıdır");
    }

    [Fact]
    public void Validate_WithNegativeSayfaSayisi_ShouldFail()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "Kitap",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = -10
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kitap sayfa sayısı 0'dan büyük olmalıdır");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "",
                KitapYazar = "",
                KitapSayfaSayisi = 0
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kitap adı boş olamaz");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kitap yazarı boş olamaz");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kitap sayfa sayısı boş olamaz");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Kitap sayfa sayısı 0'dan büyük olmalıdır");
    }

    [Fact]
    public void Validate_WithWhitespaceKitapName_ShouldFail()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "   ",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 100
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Kitap adı boş olamaz");
    }

    [Fact]
    public void Validate_WithWhitespaceKitapYazar_ShouldFail()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "Kitap",
                KitapYazar = "   ",
                KitapSayfaSayisi = 100
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Kitap yazarı boş olamaz");
    }

    [Fact]
    public void Validate_WithValidMinimumSayfaSayisi_ShouldPass()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "Minimum Sayfa Kitap",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 1
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidLargeSayfaSayisi_ShouldPass()
    {
        // Arrange
        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = new CreateSerhanKitapDto
            {
                KitapName = "Çok Sayfalı Kitap",
                KitapYazar = "Yazar",
                KitapSayfaSayisi = 10000
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
