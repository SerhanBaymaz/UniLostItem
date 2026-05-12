using Application.Features.Images.Commands.DeleteImage;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Tests.Application_Tests.Features.Images.Commands.DeleteImage;

public class DeleteImageCommandValidatorTests
{
    private readonly DeleteImageCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidPublicId_ShouldPass()
    {
        var command = new DeleteImageCommand { PublicId = "unilostitem/test123" };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.PublicId);
    }

    [Fact]
    public void Validate_WithEmptyPublicId_ShouldFail()
    {
        var command = new DeleteImageCommand { PublicId = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PublicId)
            .WithErrorMessage("Görsel kimliği (PublicId) boş olamaz");
    }

    [Fact]
    public void Validate_WithNullPublicId_ShouldFail()
    {
        var command = new DeleteImageCommand { PublicId = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PublicId);
    }
}
