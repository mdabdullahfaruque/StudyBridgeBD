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

public class RegisterHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IPasswordHasher<AppUser>> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockTokenService;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<ILogger<Register.Handler>> _mockLogger;
    private readonly Register.Handler _sut;
    
    public RegisterHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockPasswordHasher = new Mock<IPasswordHasher<AppUser>>();
        _mockTokenService = new Mock<IJwtTokenService>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockLogger = new Mock<ILogger<Register.Handler>>();
        
        _sut = new Register.Handler(
            _mockContext.Object,
            _mockPasswordHasher.Object,
            _mockTokenService.Object,
            _mockPermissionService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateUserAndReturnResponse()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        var cancellationToken = CancellationToken.None;
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);
        
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<AppUser>(), command.Password))
            .Returns("hashed_password_123");
            
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<string>(), command.Email, It.IsAny<List<string>>()))
            .Returns("jwt_token_123");
            
        _mockPermissionService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<string>(), SystemRole.User, "System"))
            .ReturnsAsync(true);
            
        _mockPermissionService.Setup(x => x.GetUserRolesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<SystemRole> { SystemRole.User });

        _mockContext.Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.DisplayName.Should().Be(command.DisplayName);
        result.Token.Should().Be("jwt_token_123");
        result.Roles.Should().Contain("User");
        result.RequiresEmailConfirmation.Should().BeTrue();

        _mockContext.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        _mockPermissionService.Verify(x => x.AssignRoleToUserAsync(It.IsAny<string>(), SystemRole.User, "System"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        var existingUser = TestDataBuilder.Users.ValidUser();
        existingUser.Email = command.Email;
        
        var users = new List<AppUser> { existingUser }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User with this email already exists");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordHashingSucceeds_ShouldUseHashedPassword()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        const string expectedHashedPassword = "secure_hashed_password_123";
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);
        
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<AppUser>(), command.Password))
            .Returns(expectedHashedPassword);
            
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns("token");
            
        _mockPermissionService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<string>(), SystemRole.User, "System"))
            .ReturnsAsync(true);
            
        _mockPermissionService.Setup(x => x.GetUserRolesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<SystemRole> { SystemRole.User });

        AppUser? capturedUser = null;
        users.Setup(x => x.Add(It.IsAny<AppUser>()))
            .Callback<AppUser>(user => capturedUser = user);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be(expectedHashedPassword);
        capturedUser.Email.Should().Be(command.Email);
        capturedUser.DisplayName.Should().Be(command.DisplayName);
        capturedUser.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenTokenServiceGeneratesToken_ShouldReturnCorrectToken()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        const string expectedToken = "expected_jwt_token_456";
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);
        
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<AppUser>(), It.IsAny<string>()))
            .Returns("hashed_password");
            
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<string>(), command.Email, It.IsAny<List<string>>()))
            .Returns(expectedToken);
            
        _mockPermissionService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<string>(), SystemRole.User, "System"))
            .ReturnsAsync(true);
            
        _mockPermissionService.Setup(x => x.GetUserRolesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<SystemRole> { SystemRole.User });

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be(expectedToken);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionServiceFails_ShouldStillCreateUser()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);
        
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<AppUser>(), It.IsAny<string>()))
            .Returns("hashed_password");
            
        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns("token");
            
        _mockPermissionService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<string>(), SystemRole.User, "System"))
            .ReturnsAsync(false); // Simulate failure
            
        _mockPermissionService.Setup(x => x.GetUserRolesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<SystemRole>()); // No roles assigned

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Roles.Should().BeEmpty();
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
