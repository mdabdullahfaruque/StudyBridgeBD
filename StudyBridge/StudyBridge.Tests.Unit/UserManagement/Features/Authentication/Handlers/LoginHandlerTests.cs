using FluentAssertions;
using Microsoft.AspNetCore.Identity;
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

public class LoginHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IPasswordHasher<AppUser>> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ILogger<Login.Handler>> _mockLogger;
    private readonly Login.Handler _sut;

    public LoginHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher<AppUser>>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockLogger = new Mock<ILogger<Login.Handler>>();

        _sut = new Login.Handler(
            _mockContext.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenService.Object,
            _mockPermissionService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Email = command.Email;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash!, command.Password))
            .Returns(PasswordVerificationResult.Success);

        _mockPermissionService.Setup(x => x.GetUserRolesAsync(user.Id.ToString()))
            .ReturnsAsync(new List<SystemRole> { SystemRole.User });

        _mockJwtTokenService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email, It.IsAny<List<string>>()))
            .Returns("jwt_token_123");

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt_token_123");
        result.Email.Should().Be(user.Email);
        result.DisplayName.Should().Be(user.DisplayName);
        result.UserId.Should().Be(user.Id.ToString());
        result.Roles.Should().Contain("User");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var user = TestDataBuilder.Users.InactiveUser();
        user.Email = command.Email;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task HandleAsync_WithUserWithoutPasswordHash_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var user = TestDataBuilder.Users.GoogleUser(); // OAuth user without password
        user.Email = command.Email;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task HandleAsync_WithIncorrectPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.InvalidLoginCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Email = command.Email;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash!, command.Password))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Email = command.Email;
        var originalLastLogin = user.LastLoginAt;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash!, command.Password))
            .Returns(PasswordVerificationResult.Success);

        _mockPermissionService.Setup(x => x.GetUserRolesAsync(user.Id.ToString()))
            .ReturnsAsync(new List<SystemRole> { SystemRole.User });

        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns("token");

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        user.LastLoginAt.Should().NotBe(originalLastLogin);
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        
        _mockContext.Verify(x => x.Users.Update(user), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleRoles_ShouldReturnAllRoles()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Email = command.Email;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash!, command.Password))
            .Returns(PasswordVerificationResult.Success);

        _mockPermissionService.Setup(x => x.GetUserRolesAsync(user.Id.ToString()))
            .ReturnsAsync(new List<SystemRole> { SystemRole.User, SystemRole.Admin });

        _mockJwtTokenService.Setup(x => x.GenerateToken(user.Id.ToString(), user.Email, It.IsAny<List<string>>()))
            .Returns("token");

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Roles.Should().Contain("User");
        result.Roles.Should().Contain("Admin");
        result.Roles.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldLogInformation()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();
        var user = TestDataBuilder.Users.ValidUser();
        user.Email = command.Email;
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockPasswordHasher.Setup(x => x.VerifyHashedPassword(user, user.PasswordHash!, command.Password))
            .Returns(PasswordVerificationResult.Success);

        _mockPermissionService.Setup(x => x.GetUserRolesAsync(user.Id.ToString()))
            .ReturnsAsync(new List<SystemRole> { SystemRole.User });

        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns("token");

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"User logged in successfully: {command.Email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
