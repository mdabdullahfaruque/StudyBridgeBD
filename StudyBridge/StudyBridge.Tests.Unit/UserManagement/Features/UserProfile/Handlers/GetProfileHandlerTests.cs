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

public class GetProfileHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ILogger<GetProfile.Handler>> _mockLogger;
    private readonly GetProfile.Handler _sut;

    public GetProfileHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockLogger = new Mock<ILogger<GetProfile.Handler>>();

        _sut = new GetProfile.Handler(
            _mockContext.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidUserId_ShouldReturnUserProfile()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        var query = new GetProfile.Query { UserId = user.Id.ToString() };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.DisplayName.Should().Be(user.DisplayName);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.AvatarUrl.Should().Be(user.AvatarUrl);
        result.CreatedAt.Should().Be(user.CreatedAt);
        result.LastLoginAt.Should().Be(user.LastLoginAt);
        result.IsActive.Should().Be(user.IsActive);
        result.EmailConfirmed.Should().Be(user.EmailConfirmed);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUserId_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetProfile.Query { UserId = Guid.NewGuid().ToString() };
        
        var users = new List<AppUser>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act & Assert
        var act = async () => await _sut.HandleAsync(query, CancellationToken.None);
        
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("User profile not found");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidGuidFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var query = new GetProfile.Query { UserId = "invalid-guid-format" };

        // Act & Assert
        var act = async () => await _sut.HandleAsync(query, CancellationToken.None);
        
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_WithInactiveUser_ShouldStillReturnProfile()
    {
        // Arrange
        var user = TestDataBuilder.Users.InactiveUser();
        var query = new GetProfile.Query { UserId = user.Id.ToString() };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleUsers_ShouldReturnCorrectUser()
    {
        // Arrange
        var user1 = TestDataBuilder.Users.ValidUser();
        var user2 = TestDataBuilder.Users.InactiveUser();
        var user3 = TestDataBuilder.Users.GoogleUser();
        
        var query = new GetProfile.Query { UserId = user2.Id.ToString() };
        
        var users = new List<AppUser> { user1, user2, user3 }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user2.Id);
        result.Email.Should().Be(user2.Email);
        result.DisplayName.Should().Be(user2.DisplayName);
    }

    [Fact]
    public async Task HandleAsync_WhenDatabaseThrowsException_ShouldRethrowException()
    {
        // Arrange
        var query = new GetProfile.Query { UserId = Guid.NewGuid().ToString() };
        
        _mockContext.Setup(x => x.Users)
            .Throws(new Exception("Database connection failed"));

        // Act & Assert
        var act = async () => await _sut.HandleAsync(query, CancellationToken.None);
        
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task HandleAsync_WithNullAvatarUrl_ShouldReturnNullAvatarUrl()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        user.AvatarUrl = null;
        var query = new GetProfile.Query { UserId = user.Id.ToString() };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNullLastLoginAt_ShouldReturnNullLastLoginAt()
    {
        // Arrange
        var user = TestDataBuilder.Users.ValidUser();
        user.LastLoginAt = null;
        var query = new GetProfile.Query { UserId = user.Id.ToString() };
        
        var users = new List<AppUser> { user }.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(x => x.Users).Returns(users.Object);

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.LastLoginAt.Should().BeNull();
    }
}
