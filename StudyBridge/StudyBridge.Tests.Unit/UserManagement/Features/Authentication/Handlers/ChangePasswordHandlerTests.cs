using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Authentication.Handlers;

public class ChangePasswordHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IPasswordHashingService> _mockPasswordHashingService;
    private readonly Mock<ILogger<ChangePassword.Handler>> _mockLogger;
    private readonly ChangePassword.Handler _sut;

    public ChangePasswordHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockPasswordHashingService = new Mock<IPasswordHashingService>();
        _mockLogger = new Mock<ILogger<ChangePassword.Handler>>();

        _sut = new ChangePassword.Handler(
            _mockContext.Object,
            _mockPasswordHashingService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldChangePasswordSuccessfully()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Id = Guid.Parse(command.UserId);
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHashingService.Setup(x => x.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            .Returns(true);

        _mockPasswordHashingService.Setup(x => x.HashPassword(command.NewPassword))
            .Returns("new_hashed_password_123");

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Password changed successfully");

        user.PasswordHash.Should().Be("new_hashed_password_123");
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        _mockContext.Verify(x => x.Users.Update(user), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidUserId_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var user = TestDataBuilder.Users.InactiveUser();
        user.Id = Guid.Parse(command.UserId);
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task HandleAsync_WithOAuthOnlyUser_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var user = TestDataBuilder.Users.GoogleUser(); // OAuth user without password
        user.Id = Guid.Parse(command.UserId);
        user.IsActive = true;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot change password for OAuth-only users");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithIncorrectCurrentPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.InvalidChangePasswordCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Id = Guid.Parse(command.UserId);
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHashingService.Setup(x => x.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Current password is incorrect");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Id = Guid.Parse(command.UserId);
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHashingService.Setup(x => x.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            .Returns(true);

        _mockPasswordHashingService.Setup(x => x.HashPassword(command.NewPassword))
            .Returns("new_hashed_password");

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while changing password");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidGuidFormat_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            UserId = "invalid-guid-format",
            CurrentPassword = "current",
            NewPassword = "new"
        };

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while changing password");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldLogInformation()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Id = Guid.Parse(command.UserId);
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHashingService.Setup(x => x.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            .Returns(true);

        _mockPasswordHashingService.Setup(x => x.HashPassword(command.NewPassword))
            .Returns("new_hashed_password");

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Password changed successfully for user: {command.UserId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUnauthorized_ShouldLogWarning()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Password change attempt for invalid user: {command.UserId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
