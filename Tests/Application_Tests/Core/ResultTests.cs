using Application.Core;
using FluentAssertions;

namespace Tests.Application_Tests.Core;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessResult()
    {
        // Arrange
        var value = "Test Value";
        var message = "Success Message";

        // Act
        var result = Result<string>.Success(message, value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Message.Should().Be(message);
        result.Code.Should().Be(0); // Default int value
    }

    [Fact]
    public void Failure_ShouldReturnFailureResult()
    {
        // Arrange
        var message = "Error Message";
        var code = 404;

        // Act
        var result = Result<string>.Failure(message, code);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Message.Should().Be(message);
        result.Code.Should().Be(code);
    }
}
