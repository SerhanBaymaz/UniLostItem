using Application.Features.Auth.Commands.Register;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new RegisterCommand { RegisterDto = new RegisterDto { Email = "", Password = "Password1!" } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Email);
    }
}