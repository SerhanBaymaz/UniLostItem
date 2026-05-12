using Application.Features.LostItems.Commands.UpdateLostItem;
using Domain.Common.Enums;
using FluentAssertions;

namespace Tests.Application_Tests.Features.LostItems.Commands.UpdateLostItem;

public class UpdateLostItemCommandValidatorTests
{
    private readonly UpdateLostItemCommandValidator _validator;

    public UpdateLostItemCommandValidatorTests()
    {
        _validator = new UpdateLostItemCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new UpdateLostItemCommand
        {
            Id = "valid-id",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Updated Location",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "updated@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new UpdateLostItemCommand
        {
            Id = "",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Title",
                Description = "Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Item ID boş olamaz");
    }

    [Fact]
    public void Validate_WithNullDto_ShouldFail()
    {
        var command = new UpdateLostItemCommand { Id = "id", UpdateLostItemDto = null! };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Item bilgileri gereklidir");
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        var command = new UpdateLostItemCommand
        {
            Id = "id",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "",
                Description = "Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc",
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
    public void Validate_WithFutureIncidentDate_ShouldFail()
    {
        var command = new UpdateLostItemCommand
        {
            Id = "id",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Title",
                Description = "Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow.AddDays(1),
                LocationLabel = "Loc",
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
    public void Validate_WithInvalidCoordinates_ShouldFail()
    {
        var command = new UpdateLostItemCommand
        {
            Id = "id",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Title",
                Description = "Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc",
                Latitude = 100.0,
                Longitude = 200.0,
                ContactInfo = "test@test.com"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Enlem -90 ile 90 arasında olmalıdır");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Boylam -180 ile 180 arasında olmalıdır");
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    public void Validate_WithValidImageExtensions_ShouldPass(string fileName)
    {
        var command = new UpdateLostItemCommand
        {
            Id = "id",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Title",
                Description = "Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com",
                ImageFileName = fileName
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("virus.exe")]
    [InlineData("doc.pdf")]
    public void Validate_WithInvalidImageExtensions_ShouldFail(string fileName)
    {
        var command = new UpdateLostItemCommand
        {
            Id = "id",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Title",
                Description = "Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com",
                ImageFileName = fileName
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Yalnızca JPG, JPEG, PNG ve WEBP formatları desteklenmektedir");
    }

    [Fact]
    public void Validate_WithLargeImageStream_ShouldFail()
    {
        var stream = new MemoryStream(new byte[21 * 1024 * 1024]); // 21MB
        var command = new UpdateLostItemCommand
        {
            Id = "id",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Title",
                Description = "Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com",
                ImageStream = stream
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Görsel boyutu en fazla 20 MB olabilir");
    }
}
