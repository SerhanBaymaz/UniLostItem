using Application.Features.Images.Commands.UploadImage;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Tests.Application_Tests.Features.Images.Commands.UploadImage;

public class UploadImageCommandValidatorTests
{
    private readonly UploadImageCommandValidator _validator = new();

    private static UploadImageCommand CreateCommand(string fileName, Stream stream)
    {
        return new UploadImageCommand
        {
            FileName = fileName,
            ImageStream = stream
        };
    }

    [Fact]
    public void Validate_ShouldFail_WhenFileNameIsEmpty()
    {
        var command = CreateCommand("", new MemoryStream(new byte[] { 1, 2, 3 }));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("Dosya adı boş olamaz");
    }

    [Fact]
    public void Validate_ShouldFail_WhenImageStreamIsNull()
    {
        var command = CreateCommand("photo.jpg", null!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ImageStream)
            .WithErrorMessage("Görsel dosyası gereklidir");
    }

    [Fact]
    public void Validate_ShouldFail_WhenImageStreamIsEmpty()
    {
        var command = CreateCommand("photo.jpg", new MemoryStream());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ImageStream)
            .WithErrorMessage("Görsel dosyası boş olamaz");
    }

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.png")]
    [InlineData("photo.webp")]
    public void Validate_ShouldPass_ForValidExtensions(string fileName)
    {
        var command = CreateCommand(fileName, new MemoryStream(new byte[] { 1, 2, 3 }));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }

    [Theory]
    [InlineData("virus.exe")]
    [InlineData("document.pdf")]
    [InlineData("image.bmp")]
    public void Validate_ShouldFail_ForInvalidExtensions(string fileName)
    {
        var command = CreateCommand(fileName, new MemoryStream(new byte[] { 1, 2, 3 }));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("Yalnızca JPG, JPEG, PNG ve WEBP formatları desteklenmektedir");
    }

    [Fact]
    public void Validate_ShouldPass_WhenFileSizeIsUnderLimit()
    {
        var command = CreateCommand("photo.jpg", new MemoryStream(new byte[19 * 1024 * 1024]));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.ImageStream);
    }

    [Fact]
    public void Validate_ShouldFail_WhenFileSizeExceedsLimit()
    {
        var command = CreateCommand("photo.jpg", new MemoryStream(new byte[21 * 1024 * 1024]));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ImageStream)
            .WithErrorMessage("Görsel boyutu en fazla 20 MB olabilir");
    }
}
