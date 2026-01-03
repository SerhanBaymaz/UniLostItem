using Application.Features.Auth.Commands.Login;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new LoginCommand { LoginDto = new LoginDto { Email = "", Password = "Password1!" } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LoginDto.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new LoginCommand { LoginDto = new LoginDto { Email = "invalid-email", Password = "Password1!" } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LoginDto.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new LoginCommand { LoginDto = new LoginDto { Email = "test@test.com", Password = "" } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LoginDto.Password);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new LoginCommand { LoginDto = new LoginDto { Email = "test@test.com", Password = "Password1!" } };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}