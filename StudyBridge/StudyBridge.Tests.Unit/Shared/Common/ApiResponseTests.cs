using FluentAssertions;
using StudyBridge.Shared.Common;

namespace StudyBridge.Tests.Unit.Shared.Common;

public class ApiResponseTests
{
    [Fact]
    public void SuccessResult_WithData_ShouldCreateSuccessResponse()
    {
        // Arrange
        var testData = "Test data";
        var message = "Operation successful";

        // Act
        var result = ApiResponse<string>.SuccessResult(testData, message);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(message);
        result.Data.Should().Be(testData);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void SuccessResult_WithoutMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var testData = 42;

        // Act
        var result = ApiResponse<int>.SuccessResult(testData);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Success");
        result.Data.Should().Be(testData);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void FailureResult_WithMessageAndErrors_ShouldCreateFailureResponse()
    {
        // Arrange
        var message = "Operation failed";
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = ApiResponse<string>.FailureResult(message, errors);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Data.Should().BeNull();
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void FailureResult_WithoutErrors_ShouldCreateEmptyErrorsList()
    {
        // Arrange
        var message = "Operation failed";

        // Act
        var result = ApiResponse<string>.FailureResult(message);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Data.Should().BeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ApiResponse_NonGeneric_SuccessResult_ShouldCreateSuccessResponse()
    {
        // Arrange
        var message = "Success message";

        // Act
        var result = ApiResponse.SuccessResult(message);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(message);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ApiResponse_NonGeneric_FailureResult_ShouldCreateFailureResponse()
    {
        // Arrange
        var message = "Failure message";
        var errors = new List<string> { "Validation error" };

        // Act
        var result = ApiResponse.FailureResult(message, errors);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Errors.Should().BeEquivalentTo(errors);
    }
}
