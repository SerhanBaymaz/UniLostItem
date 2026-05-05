using Application.Features.ItemClaims.Commands.RespondToClaim;
using FluentAssertions;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.RespondToClaim;

public class RespondToClaimCommandValidatorTests
{
    private readonly RespondToClaimCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = true }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldFail()
    {
        var command = new RespondToClaimCommand
        {
            Id = "",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = true }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Talep ID gereklidir");
    }

    [Fact]
    public void Validate_WithNullDto_ShouldFail()
    {
        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = null!
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Yanıt bilgileri gereklidir");
    }

    [Fact]
    public void Validate_WithCommentTooLong_ShouldFail()
    {
        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = new RespondToClaimDto
            {
                IsApproved = true,
                Comment = new string('A', 501)
            }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Yorum en fazla 500 karakter olabilir");
    }

    [Fact]
    public void Validate_WithNullComment_ShouldPass()
    {
        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = false, Comment = null }
        };

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
