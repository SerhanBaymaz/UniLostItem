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
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "",
                LastName = "TestLastName",
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Null()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = null!,
                LastName = "TestLastName",
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName)
            .WithErrorMessage("First name is required.");
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
                PhoneNumber = "1234567890"
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
                PhoneNumber = "1234567890"
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
                PhoneNumber = "1234567890"
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
    public void Should_Have_Error_When_LastName_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = "",
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Null()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "TestFirstName",
                LastName = null!,
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName)
            .WithErrorMessage("Last name is required.");
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
                PhoneNumber = "1234567890"
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
                PhoneNumber = "1234567890"
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
                PhoneNumber = "1234567890"
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
    public void Should_Have_Error_When_PhoneNumber_Is_Empty()
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

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber)
            .WithErrorMessage("Phone number is required.");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Null()
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
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber)
            .WithErrorMessage("Phone number is required.");
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
                PhoneNumber = new string('1', 21)
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
                PhoneNumber = new string('1', 20)
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
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Fields_Are_Invalid()
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

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.LastName);
        result.ShouldHaveValidationErrorFor(x => x.UpdateUserProfileDto.PhoneNumber);
    }

    #endregion

    #region Valid Command Tests

    [Fact]
    public void Should_Not_Have_Any_Errors_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto
            {
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
