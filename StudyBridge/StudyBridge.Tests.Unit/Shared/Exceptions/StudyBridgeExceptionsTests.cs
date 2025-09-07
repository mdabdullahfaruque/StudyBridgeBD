using FluentAssertions;
using StudyBridge.Shared.Exceptions;

namespace StudyBridge.Tests.Unit.Shared.Exceptions;

public class StudyBridgeExceptionsTests
{
    [Fact]
    public void ValidationException_WithMessage_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new List<string> { "Field is required", "Invalid format" };

        // Act
        var exception = new ValidationException(message, errors);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(400);
        exception.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ValidationException_WithErrorsList_ShouldUseDefaultMessage()
    {
        // Arrange
        var errors = new List<string> { "Field is required", "Invalid format" };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Validation failed");
        exception.StatusCode.Should().Be(400);
        exception.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void NotFoundException_WithMessage_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "User not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(404);
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().Should().Be(message);
    }

    [Fact]
    public void NotFoundException_WithEntityNameAndId_ShouldFormatMessage()
    {
        // Arrange
        var entityName = "User";
        var id = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(entityName, id);

        // Assert
        exception.Message.Should().Be($"User with id '{id}' was not found");
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public void UnauthorizedException_WithDefaultMessage_ShouldSetCorrectProperties()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.Message.Should().Be("Unauthorized access");
        exception.StatusCode.Should().Be(401);
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().Should().Be("Unauthorized access");
    }

    [Fact]
    public void UnauthorizedException_WithCustomMessage_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Invalid token";

        // Act
        var exception = new UnauthorizedException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(401);
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().Should().Be(message);
    }

    [Fact]
    public void ForbiddenException_WithDefaultMessage_ShouldSetCorrectProperties()
    {
        // Act
        var exception = new ForbiddenException();

        // Assert
        exception.Message.Should().Be("Access forbidden");
        exception.StatusCode.Should().Be(403);
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().Should().Be("Access forbidden");
    }

    [Fact]
    public void ForbiddenException_WithCustomMessage_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Insufficient permissions";

        // Act
        var exception = new ForbiddenException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(403);
    }

    [Fact]
    public void ConflictException_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Email already exists";

        // Act
        var exception = new ConflictException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(409);
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().Should().Be(message);
    }

    [Fact]
    public void BusinessLogicException_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Business rule violation";

        // Act
        var exception = new BusinessLogicException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(422);
        exception.Errors.Should().HaveCount(1);
        exception.Errors.First().Should().Be(message);
    }

    [Fact]
    public void StudyBridgeException_WithInnerException_ShouldSetCorrectProperties()
    {
        // Arrange
        var message = "Operation failed";
        var innerException = new ArgumentNullException("parameter");
        var statusCode = 500;
        var errors = new List<string> { "Internal error" };

        // Act
        var exception = new TestStudyBridgeException(message, innerException, statusCode, errors);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.StatusCode.Should().Be(statusCode);
        exception.Errors.Should().BeEquivalentTo(errors);
    }

    // Test implementation of abstract class
    private class TestStudyBridgeException : StudyBridgeException
    {
        public TestStudyBridgeException(string message, int statusCode = 400, List<string>? errors = null) 
            : base(message, statusCode, errors)
        {
        }

        public TestStudyBridgeException(string message, Exception innerException, int statusCode = 400, List<string>? errors = null) 
            : base(message, innerException, statusCode, errors)
        {
        }
    }
}
