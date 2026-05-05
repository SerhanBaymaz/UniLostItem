using Application.Features.ItemClaims.Commands.AdminReviewClaim;
using FluentAssertions;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.AdminReviewClaim;

public class AdminReviewClaimCommandValidatorTests
{
    private readonly AdminReviewClaimCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new AdminReviewClaimCommand
        {
            Id = "claim-1",
            AdminReviewClaimDto = new AdminReviewClaimDto { IsApproved = true, Comment = "Looks good" }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new AdminReviewClaimCommand
        {
            Id = "",
            AdminReviewClaimDto = new AdminReviewClaimDto { IsApproved = true }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Talep ID gereklidir");
    }

    [Fact]
    public void Validate_WithNullDto_ShouldFail()
    {
        var command = new AdminReviewClaimCommand
        {
            Id = "claim-1",
            AdminReviewClaimDto = null!
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Admin değerlendirme bilgileri gereklidir");
    }

    [Fact]
    public void Validate_WithCommentTooLong_ShouldFail()
    {
        var command = new AdminReviewClaimCommand
        {
            Id = "claim-1",
            AdminReviewClaimDto = new AdminReviewClaimDto
            {
                IsApproved = false,
                Comment = new string('A', 501)
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Yorum en fazla 500 karakter olabilir");
    }
}
