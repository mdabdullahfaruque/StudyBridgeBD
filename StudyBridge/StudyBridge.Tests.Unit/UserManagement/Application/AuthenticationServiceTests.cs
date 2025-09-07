using FluentAssertions;
using Moq;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.Shared.Exceptions;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Application.Services;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.Tests.Unit.UserManagement.Application;

public class AuthenticationServiceTests
{
    private readonly Mock<IDispatcher> _mockDispatcher;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _mockDispatcher = new Mock<IDispatcher>();
        _sut = new AuthenticationService(_mockDispatcher.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        var response = TestDataBuilder.Responses.Authentication.SuccessfulRegisterResponse();
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.RegisterAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(response);
        result.Message.Should().Be("Registration successful");
        result.StatusCode.Should().Be(201);

        _mockDispatcher.Verify(x => x.CommandAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithConflictException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        var cancellationToken = CancellationToken.None;
        var conflictException = new ConflictException("User already exists");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(conflictException);

        // Act
        var result = await _sut.RegisterAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User already exists");
        result.StatusCode.Should().Be(409);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithValidationException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        var cancellationToken = CancellationToken.None;
        var validationErrors = new List<string> { "Email is required", "Password too weak" };
        var validationException = new ValidationException("Validation failed", validationErrors);

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(validationException);

        // Act
        var result = await _sut.RegisterAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.StatusCode.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(validationErrors);
    }

    [Fact]
    public async Task RegisterAsync_WithGenericException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        var cancellationToken = CancellationToken.None;
        var exception = new Exception("Database connection failed");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.RegisterAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Registration failed: Database connection failed");
        result.StatusCode.Should().Be(500);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var response = TestDataBuilder.Responses.Authentication.SuccessfulLoginResponse();
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.LoginAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(response);
        result.Message.Should().Be("Login successful");
        result.StatusCode.Should().Be(200);

        _mockDispatcher.Verify(x => x.CommandAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithUnauthorizedException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.InvalidLoginCommand();
        var cancellationToken = CancellationToken.None;
        var unauthorizedException = new UnauthorizedException("Invalid credentials");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(unauthorizedException);

        // Act
        var result = await _sut.LoginAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task LoginAsync_WithValidationException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var cancellationToken = CancellationToken.None;
        var validationErrors = new List<string> { "Email is required" };
        var validationException = new ValidationException("Validation failed", validationErrors);

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(validationException);

        // Act
        var result = await _sut.LoginAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.StatusCode.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(validationErrors);
    }

    #endregion

    #region GoogleLoginAsync Tests

    [Fact]
    public async Task GoogleLoginAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidGoogleLoginCommand();
        var response = TestDataBuilder.Responses.Authentication.SuccessfulGoogleLoginResponse();
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GoogleLoginAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(response);
        result.Message.Should().Be("Google login successful");
        result.StatusCode.Should().Be(200);

        _mockDispatcher.Verify(x => x.CommandAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GoogleLoginAsync_WithInvalidToken_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidGoogleLoginCommand();
        var cancellationToken = CancellationToken.None;
        var unauthorizedException = new UnauthorizedException("Invalid Google token");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(unauthorizedException);

        // Act
        var result = await _sut.GoogleLoginAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid Google token");
        result.StatusCode.Should().Be(401);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var response = TestDataBuilder.Responses.Authentication.SuccessfulChangePasswordResponse();
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.ChangePasswordAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(response);
        result.Message.Should().Be("Password changed successfully");
        result.StatusCode.Should().Be(200);

        _mockDispatcher.Verify(x => x.CommandAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithUnauthorizedException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.InvalidChangePasswordCommand();
        var cancellationToken = CancellationToken.None;
        var unauthorizedException = new UnauthorizedException("Current password is incorrect");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(unauthorizedException);

        // Act
        var result = await _sut.ChangePasswordAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Current password is incorrect");
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNotFoundException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var cancellationToken = CancellationToken.None;
        var notFoundException = new NotFoundException("User not found");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(notFoundException);

        // Act
        var result = await _sut.ChangePasswordAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithStudyBridgeException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var cancellationToken = CancellationToken.None;
        var customErrors = new List<string> { "Custom error 1", "Custom error 2" };
        var businessLogicException = new BusinessLogicException("Custom exception");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(businessLogicException);

        // Act
        var result = await _sut.ChangePasswordAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Custom exception");
        result.StatusCode.Should().Be(422);
        result.Errors.Should().BeEquivalentTo(new List<string> { "Custom exception" });
    }

    [Fact]
    public async Task ChangePasswordAsync_WithGenericException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var cancellationToken = CancellationToken.None;
        var exception = new Exception("Database error");

        _mockDispatcher.Setup(x => x.CommandAsync(command, cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.ChangePasswordAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Password change failed: Database error");
        result.StatusCode.Should().Be(500);
    }

    #endregion
}
