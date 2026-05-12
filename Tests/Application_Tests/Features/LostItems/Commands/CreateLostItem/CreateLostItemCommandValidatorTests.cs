using Application.Features.LostItems.Commands.CreateLostItem;
using Domain.Common.Enums;
using FluentAssertions;

namespace Tests.Application_Tests.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemCommandValidatorTests
{
    private readonly CreateLostItemCommandValidator _validator;

    public CreateLostItemCommandValidatorTests()
    {
        _validator = new CreateLostItemCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "iPhone 15 Pro Max",
                Description = "Siyah renk, kılıfı yok",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Kütüphane B Blok",
                Latitude = 41.0082,
                Longitude = 28.9784,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullDto_ShouldFail()
    {
        var command = new CreateLostItemCommand { CreateLostItemDto = null! };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Item bilgileri gereklidir");
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "",
                Description = "Açıklama",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Konum",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Başlık boş olamaz");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = new string('A', 201),
                Description = "Açıklama",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Konum",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Başlık en fazla 200 karakter olabilir");
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "Başlık",
                Description = "",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Konum",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Açıklama boş olamaz");
    }

    [Fact]
    public void Validate_WithEmptyContactInfo_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "Başlık",
                Description = "Açıklama",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Konum",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = ""
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "İletişim bilgisi boş olamaz");
    }

    [Fact]
    public void Validate_WithFutureIncidentDate_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "Başlık",
                Description = "Açıklama",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(1),
                LocationLabel = "Konum",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Olay tarihi gelecekte olamaz");
    }

    [Fact]
    public void Validate_WithInvalidLatitude_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "Başlık",
                Description = "Açıklama",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Konum",
                Latitude = 91.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Enlem -90 ile 90 arasında olmalıdır");
    }

    [Fact]
    public void Validate_WithInvalidLongitude_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "Başlık",
                Description = "Açıklama",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Konum",
                Latitude = 41.0,
                Longitude = 181.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Boylam -180 ile 180 arasında olmalıdır");
    }

    [Fact]
    public void Validate_WithEmptyLocationLabel_ShouldFail()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "Başlık",
                Description = "Açıklama",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Konum açıklaması boş olamaz");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        var command = new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "",
                Description = "",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(1),
                LocationLabel = "",
                Latitude = 91.0,
                Longitude = 181.0,
                ContactInfo = ""
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(6);
    }
}
