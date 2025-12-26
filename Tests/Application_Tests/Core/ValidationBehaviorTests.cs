using Application.Core;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace Tests.Application_Tests.Core;

public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestRequest>> _validatorMock;
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _nextMock;
    private readonly ValidationBehavior<TestRequest, TestResponse> _behavior;

    public ValidationBehaviorTests()
    {
        _validatorMock = new Mock<IValidator<TestRequest>>();
        _nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        _behavior = new ValidationBehavior<TestRequest, TestResponse>(_validatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidatorIsNull()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(null);
        var request = new TestRequest();
        var response = new TestResponse();

        _nextMock.Setup(x => x()).ReturnsAsync(response);

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(response);
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        // Arrange
        var request = new TestRequest();
        var response = new TestResponse();
        var validationResult = new ValidationResult(); // Valid result

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _nextMock.Setup(x => x()).ReturnsAsync(response);

        // Act
        var result = await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(response);
        _nextMock.Verify(x => x(), Times.Once);
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var request = new TestRequest();
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Property", "Error message")
        };
        var validationResult = new ValidationResult(failures);

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        Func<Task> act = async () => await _behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.Errors.First().ErrorMessage == "Error message");

        _nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToValidator()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var request = new TestRequest();
        var response = new TestResponse();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(request, cts.Token))
            .ReturnsAsync(validationResult);

        _nextMock.Setup(x => x()).ReturnsAsync(response);

        // Act
        await _behavior.Handle(request, _nextMock.Object, cts.Token);

        // Assert
        _validatorMock.Verify(v => v.ValidateAsync(request, cts.Token), Times.Once);
    }
}
public class TestRequest : IRequest<TestResponse>
{
    public string? Dummy { get; set; }
}
public class TestResponse
{
    public string? Dummy { get; set; }
}
