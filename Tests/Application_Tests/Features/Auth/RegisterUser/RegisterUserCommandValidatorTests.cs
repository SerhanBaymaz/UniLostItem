using Application.Features.Auth.RegisterUser;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var model = new RegisterUserCommand { Email = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var model = new RegisterUserCommand { Email = "invalid-email" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short")] // Too short
    [InlineData("nouppercase1!")] // No uppercase
    [InlineData("NOLOWERCASE1!")] // No lowercase
    [InlineData("NoNumber!")] // No number
    [InlineData("NoSpecialChar1")] // No special char
    public void Should_Have_Error_When_Password_Is_Weak(string password)
    {
        var model = new RegisterUserCommand { Password = password };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = new RegisterUserCommand
        {
            Email = "test@example.com",
            Password = "Password1!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
