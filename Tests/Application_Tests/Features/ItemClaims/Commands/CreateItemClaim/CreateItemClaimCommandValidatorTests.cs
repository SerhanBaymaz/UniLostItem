using Application.Features.ItemClaims.Commands.CreateItemClaim;
using FluentAssertions;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.CreateItemClaim;

public class CreateItemClaimCommandValidatorTests
{
    private readonly CreateItemClaimCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = "item-1",
                Description = "This is my item"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullDto_ShouldFail()
    {
        var command = new CreateItemClaimCommand { CreateItemClaimDto = null! };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Talep bilgileri gereklidir");
    }

    [Fact]
    public void Validate_WithEmptyLostItemId_ShouldFail()
    {
        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = "",
                Description = "Description"
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "İlan ID gereklidir");
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldFail()
    {
        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = "item-1",
                Description = ""
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Açıklama gereklidir");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldFail()
    {
        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = "item-1",
                Description = new string('A', 1001)
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Açıklama en fazla 1000 karakter olabilir");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = "",
                Description = ""
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(2);
    }
}
