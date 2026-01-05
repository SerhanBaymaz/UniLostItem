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

    #region RegisterDto Null Tests

    [Fact]
    public void Should_Have_Error_When_RegisterDto_Is_Null()
    {
        // Arrange
        var command = new RegisterCommand { RegisterDto = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto)
            .WithErrorMessage("Registration information is required.");
    }

    #endregion

    #region Email Tests

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Email)
            .WithErrorMessage("Email address is required");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = null!,
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Email)
            .WithErrorMessage("Email address is required");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "invalid-email",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Email)
            .WithErrorMessage("A valid email address is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegisterDto.Email);
    }

    #endregion

    #region Password Tests

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Pass1",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password)
            .WithErrorMessage("Password must be at least 6 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Uppercase()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Lowercase()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "PASSWORD1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Number()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password)
            .WithErrorMessage("Password must contain at least one number");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Special_Character()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password)
            .WithErrorMessage("Password must contain at least one special character");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Password_Is_Valid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegisterDto.Password);
    }

    #endregion

    #region FirstName Tests

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Null()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = null!,
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Valid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegisterDto.FirstName);
    }

    #endregion

    #region LastName Tests

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Empty()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.LastName)
            .WithErrorMessage("Last name is required");
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Null()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = null!,
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.LastName)
            .WithErrorMessage("Last name is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Valid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegisterDto.LastName);
    }

    #endregion

    #region PhoneNumber Tests

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Empty()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = ""
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber)
            .WithErrorMessage("Phone number is required");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Null()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = null!
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber)
            .WithErrorMessage("Phone number is required");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Does_Not_Start_With_Plus()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber)
            .WithErrorMessage("Phone number must start with + and contain only digits after");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Contains_Non_Digit_Characters()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+123-456-7890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber)
            .WithErrorMessage("Phone number must start with + and contain only digits after");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Exceeds_20_Characters()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+" + new string('1', 20) // 21 characters total
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber)
            .WithErrorMessage("Phone number cannot exceed 20 characters");
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Exactly_20_Characters()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+" + new string('1', 19) // 20 characters total
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber);
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Valid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber);
    }

    #endregion

    #region Valid Command Tests

    [Fact]
    public void Should_Not_Have_Any_Errors_When_Command_Is_Valid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password1!",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Fields_Are_Invalid()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto
            {
                Email = "invalid",
                Password = "weak",
                FirstName = "",
                LastName = "",
                PhoneNumber = "invalid"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Email);
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password);
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.LastName);
        result.ShouldHaveValidationErrorFor(x => x.RegisterDto.PhoneNumber);
    }

    #endregion
}
