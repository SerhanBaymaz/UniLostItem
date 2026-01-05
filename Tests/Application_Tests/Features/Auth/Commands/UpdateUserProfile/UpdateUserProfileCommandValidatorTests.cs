using Application.Features.Auth.Commands.UpdateUserProfile;
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator;

    public UpdateUserProfileCommandValidatorTests()
    {
        _validator = new UpdateUserProfileCommandValidator();
    }

    #region FirstName Tests

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Null()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = null!,
                LastName = "TestLastName",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "",
                LastName = "TestLastName",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Whitespace()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "   ",
                LastName = "TestLastName",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Exceeds_100_Characters()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = new string('A', 101),
                LastName = "TestLastName",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName)
            .WithErrorMessage("First name cannot exceed 100 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Exactly_100_Characters()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = new string('A', 100),
                LastName = "TestLastName",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FirstName_Is_Valid()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = "TestLastName",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
    }

    #endregion

    #region LastName Tests

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Null()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = null!,
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Whitespace()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "   ",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Exceeds_100_Characters()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = new string('A', 101),
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName)
            .WithErrorMessage("Last name cannot exceed 100 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Exactly_100_Characters()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = new string('A', 100),
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
    }

    [Fact]
    public void Should_Not_Have_Error_When_LastName_Is_Valid()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
    }

    #endregion

    #region PhoneNumber Tests

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Null()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = null!
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = ""
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert - Empty string is treated as whitespace, so validation doesn't run
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Whitespace()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = "   "
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Does_Not_Start_With_Plus()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber)
            .WithErrorMessage("Phone number must start with + and contain only digits after");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Contains_Non_Digit_Characters()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = "+123-456-7890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber)
            .WithErrorMessage("Phone number must start with + and contain only digits after");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Exceeds_20_Characters()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = "+" + new string('1', 20) // 21 characters total
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber)
            .WithErrorMessage("Phone number cannot exceed 20 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Exactly_20_Characters()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = "+" + new string('1', 19) // 20 characters total
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Valid()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    #endregion

    #region Optional Fields Tests

    [Fact]
    public void Should_Not_Have_Any_Errors_When_All_Fields_Are_Null()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = null!,
                LastName = null!,
                PhoneNumber = null!
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Any_Errors_When_All_Fields_Are_Empty_Strings()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "",
                LastName = "",
                PhoneNumber = ""
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert - Empty strings are treated as whitespace, so validation doesn't run for any field
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
    }

    #endregion

    #region Valid Command Tests

    [Fact]
    public void Should_Not_Have_Any_Errors_When_All_Fields_Are_Valid()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
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
    public void Should_Not_Have_Any_Errors_When_Only_FirstName_Is_Provided()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = "",
                PhoneNumber = ""
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert - Empty strings are treated as whitespace, so no validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Any_Errors_When_Only_LastName_Is_Provided()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "",
                LastName = "Doe",
                PhoneNumber = ""
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert - Empty strings are treated as whitespace, so no validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Any_Errors_When_Only_PhoneNumber_Is_Provided()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "",
                LastName = "",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Fields_Exceed_Maximum_Length()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = new string('A', 101),
                LastName = new string('A', 101),
                PhoneNumber = "+" + new string('1', 21)
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    #endregion
}
