using Application.Features.LostItems.Commands.CreateLostItem;
using Domain.Common.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Tests.Application_Tests.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemImageValidatorTests
{
    private readonly CreateLostItemCommandValidator _validator = new();

    private static CreateLostItemCommand CreateValidCommand(
        string? imageFileName = null, Stream? imageStream = null)
    {
        return new CreateLostItemCommand
        {
            CreateLostItemDto = new CreateLostItemDto
            {
                Title = "Test",
                Description = "Test Description",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Test Location",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "test@test.com",
                ImageFileName = imageFileName,
                ImageStream = imageStream
            }
        };
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    [InlineData("photo.JPG")]
    [InlineData("photo.JPEG")]
    public void Validate_ShouldPass_ForValidImageExtensions(string fileName)
    {
        var command = CreateValidCommand(imageFileName: fileName);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CreateLostItemDto.ImageFileName);
    }

    [Theory]
    [InlineData("virus.exe")]
    [InlineData("document.pdf")]
    [InlineData("script.js")]
    [InlineData("photo.bmp")]
    [InlineData("photo.gif")]
    [InlineData("photo.svg")]
    public void Validate_ShouldFail_ForInvalidImageExtensions(string fileName)
    {
        var command = CreateValidCommand(imageFileName: fileName);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CreateLostItemDto.ImageFileName)
            .WithErrorMessage("Yalnızca JPG, JPEG, PNG ve WEBP formatları desteklenmektedir");
    }

    [Fact]
    public void Validate_ShouldPass_WhenNoImageProvided()
    {
        var command = CreateValidCommand();
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CreateLostItemDto.ImageFileName);
        result.ShouldNotHaveValidationErrorFor(x => x.CreateLostItemDto.ImageStream);
    }

    [Fact]
    public void Validate_ShouldPass_WhenImageSizeIsWithinLimit()
    {
        var stream = new MemoryStream(new byte[19 * 1024 * 1024]);
        var command = CreateValidCommand(imageFileName: "photo.jpg", imageStream: stream);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.CreateLostItemDto.ImageStream);
    }

    [Fact]
    public void Validate_ShouldFail_WhenImageSizeExceedsLimit()
    {
        var stream = new MemoryStream(new byte[21 * 1024 * 1024]);
        var command = CreateValidCommand(imageFileName: "photo.jpg", imageStream: stream);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CreateLostItemDto.ImageStream)
            .WithErrorMessage("Görsel boyutu en fazla 20 MB olabilir");
    }
}
