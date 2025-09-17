using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.Exceptions;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.Tests.Unit.UserManagement.Features.UserProfile.Handlers;

public class UpdateProfileHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ILogger<UpdateProfile.Handler>> _mockLogger;
    private readonly UpdateProfile.Handler _sut;

    public UpdateProfileHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockLogger = new Mock<ILogger<UpdateProfile.Handler>>();

        _sut = new UpdateProfile.Handler(
            _mockContext.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateUserProfileSuccessfully()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        var command = new UpdateProfile.Command
        {
            UserId = user.Id,
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last",
            AvatarUrl = "https://example.com/new-avatar.jpg"
        };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile updated successfully");

        user.DisplayName.Should().Be(command.DisplayName);
        user.FirstName.Should().Be(command.FirstName);
        user.LastName.Should().Be(command.LastName);
        user.AvatarUrl.Should().Be(command.AvatarUrl);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        _mockContext.Verify(x => x.Users.Update(user), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateProfile.Command
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found");

        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUserId_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateProfile.Command
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };

        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var user = TestDataBuilder.Users.InactiveUser();
        var command = new UpdateProfile.Command
        {
            UserId = user.Id,
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task HandleAsync_WithNullAvatarUrl_ShouldUpdateWithNullAvatarUrl()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        var command = new UpdateProfile.Command
        {
            UserId = user.Id,
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last",
            AvatarUrl = null
        };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        user.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenDatabaseUpdateFails_ShouldThrowException()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        var command = new UpdateProfile.Command
        {
            UserId = user.Id,
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database update failed"));

        // Act & Assert
        var act = async () => await _sut.HandleAsync(command, CancellationToken.None);
        
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database update failed");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyStrings_ShouldUpdateWithEmptyStrings()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        var command = new UpdateProfile.Command
        {
            UserId = user.Id,
            DisplayName = "",
            FirstName = "",
            LastName = "",
            AvatarUrl = ""
        };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        user.DisplayName.Should().Be("");
        user.FirstName.Should().Be("");
        user.LastName.Should().Be("");
        user.AvatarUrl.Should().Be("");
    }

    [Fact]
    public async Task HandleAsync_ShouldOnlyUpdateSpecifiedFields()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        var originalEmail = user.Email;
        var originalCreatedAt = user.CreatedAt;
        var originalPasswordHash = user.PasswordHash;
        var originalEmailConfirmed = user.EmailConfirmed;
        var originalIsActive = user.IsActive;
        
        var command = new UpdateProfile.Command
        {
            UserId = user.Id,
            DisplayName = "New Display Name",
            FirstName = "New First",
            LastName = "New Last",
            AvatarUrl = "https://example.com/new-avatar.jpg"
        };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        // These should be updated
        user.DisplayName.Should().Be(command.DisplayName);
        user.FirstName.Should().Be(command.FirstName);
        user.LastName.Should().Be(command.LastName);
        user.AvatarUrl.Should().Be(command.AvatarUrl);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        // These should remain unchanged
        user.Email.Should().Be(originalEmail);
        user.CreatedAt.Should().Be(originalCreatedAt);
        user.PasswordHash.Should().Be(originalPasswordHash);
        user.EmailConfirmed.Should().Be(originalEmailConfirmed);
        user.IsActive.Should().Be(originalIsActive);
    }

    [Fact]
    public async Task HandleAsync_WithLongStrings_ShouldUpdateSuccessfully()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        var longDisplayName = new string('A', 250);
        var longFirstName = new string('B', 100);
        var longLastName = new string('C', 100);
        
        var command = new UpdateProfile.Command
        {
            UserId = user.Id,
            DisplayName = longDisplayName,
            FirstName = longFirstName,
            LastName = longLastName,
            AvatarUrl = "https://example.com/avatar.jpg"
        };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        user.DisplayName.Should().Be(longDisplayName);
        user.FirstName.Should().Be(longFirstName);
        user.LastName.Should().Be(longLastName);
    }
}
