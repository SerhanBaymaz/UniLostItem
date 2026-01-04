using Application.Features.Auth.Commands.RefreshToken;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator;

    public RefreshTokenCommandValidatorTests()
    {
        _validator = new RefreshTokenCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_AccessToken_Is_Empty()
    {
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "", RefreshToken = "refresh_token" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RefreshTokenDto.AccessToken);
    }

    [Fact]
    public void Should_Have_Error_When_RefreshToken_Is_Empty()
    {
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "access_token", RefreshToken = "" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RefreshTokenDto.RefreshToken);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "access_token", RefreshToken = "refresh_token" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
