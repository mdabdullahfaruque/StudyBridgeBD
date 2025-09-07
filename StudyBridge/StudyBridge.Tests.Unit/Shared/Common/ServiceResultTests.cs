using FluentAssertions;
using StudyBridge.Shared.Common;

namespace StudyBridge.Tests.Unit.Shared.Common;

public class ServiceResultTests
{
    [Fact]
    public void Success_WithData_ShouldCreateSuccessResult()
    {
        // Arrange
        var testData = "Test data";
        var message = "Operation successful";
        var statusCode = 200;

        // Act
        var result = ServiceResult<string>.Success(testData, message, statusCode);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(testData);
        result.Message.Should().Be(message);
        result.StatusCode.Should().Be(statusCode);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithDefaultParameters_ShouldUseDefaults()
    {
        // Arrange
        var testData = 42;

        // Act
        var result = ServiceResult<int>.Success(testData);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(testData);
        result.Message.Should().Be("");
        result.StatusCode.Should().Be(200);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithMessage_ShouldCreateFailureResult()
    {
        // Arrange
        var message = "Operation failed";
        var statusCode = 400;
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = ServiceResult<string>.Failure(message, statusCode, errors);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be(message);
        result.StatusCode.Should().Be(statusCode);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_WithoutErrors_ShouldCreateErrorFromMessage()
    {
        // Arrange
        var message = "Operation failed";
        var statusCode = 500;

        // Act
        var result = ServiceResult<string>.Failure(message, statusCode);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be(message);
        result.StatusCode.Should().Be(statusCode);
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().Be(message);
    }

    [Fact]
    public void Failure_WithErrorsList_ShouldCreateFailureResult()
    {
        // Arrange
        var errors = new List<string> { "Validation error 1", "Validation error 2" };
        var statusCode = 422;
        var message = "Validation failed";

        // Act
        var result = ServiceResult<string>.Failure(errors, statusCode, message);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be(message);
        result.StatusCode.Should().Be(statusCode);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_WithErrorsList_DefaultParameters_ShouldUseDefaults()
    {
        // Arrange
        var errors = new List<string> { "Error 1" };

        // Act
        var result = ServiceResult<string>.Failure(errors);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Operation failed");
        result.StatusCode.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(errors);
    }
}
