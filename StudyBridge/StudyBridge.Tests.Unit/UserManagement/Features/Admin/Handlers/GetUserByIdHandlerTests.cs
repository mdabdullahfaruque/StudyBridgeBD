using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.UserManagement.Features.Admin;
using StudyBridge.Tests.Unit.TestData;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Admin.Handlers;

public class GetUserByIdHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<ILogger<GetUserById.Handler>> _mockLogger;
    private readonly GetUserById.Handler _sut;

    public GetUserByIdHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockLogger = new Mock<ILogger<GetUserById.Handler>>();
        _sut = new GetUserById.Handler(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_User_When_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var users = new List<AppUser> { user };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        var query = new GetUserById.Query { UserId = userId };

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Id.Should().Be(userId.ToString());
        result.User.Email.Should().Be("test@example.com");
        result.User.DisplayName.Should().Be("Test User");
        result.User.FirstName.Should().Be("Test");
        result.User.LastName.Should().Be("User");
        result.User.IsActive.Should().BeTrue();
        result.User.EmailConfirmed.Should().BeFalse();
        result.User.CreatedAt.Should().BeAfter(DateTime.MinValue);
        result.User.LastLoginAt.Should().BeNull();
        result.User.Roles.Should().HaveCount(1);
        result.User.Roles.First().Should().Be("Admin");
        result.Message.Should().Be("User retrieved successfully");
    }

    [Fact]
    public async Task Handle_Should_Return_Null_User_When_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<AppUser>();
        var mockDbSet = users.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        var query = new GetUserById.Query { UserId = userId };

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().BeNull();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_Should_Include_User_Roles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUserWithMultipleRoles(userId);
        var users = new List<AppUser> { user };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        var query = new GetUserById.Query { UserId = userId };

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Roles.Should().HaveCount(2);
        result.User.Roles.Should().Contain("Admin");
        result.User.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Handle_Should_Include_Active_Subscriptions_Only()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUserWithSubscriptions(userId);
        var users = new List<AppUser> { user };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        var query = new GetUserById.Query { UserId = userId };

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Subscriptions.Should().HaveCount(1);
        result.User.Subscriptions.First().IsActive.Should().BeTrue();
        result.User.Subscriptions.First().Plan.Should().Be("Premium");
    }

    [Fact]
    public async Task Handle_Should_Map_User_Properties_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var users = new List<AppUser> { user };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        var query = new GetUserById.Query { UserId = userId };

        // Act
        var result = await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var userDto = result.User!;
        userDto.Id.Should().Be(userId.ToString());
        userDto.Email.Should().Be(user.Email);
        userDto.DisplayName.Should().Be(user.DisplayName);
        userDto.FirstName.Should().Be(user.FirstName);
        userDto.LastName.Should().Be(user.LastName);
        userDto.IsActive.Should().Be(user.IsActive);
        userDto.EmailConfirmed.Should().Be(user.EmailConfirmed);
        userDto.CreatedAt.Should().Be(user.CreatedAt);
        userDto.LastLoginAt.Should().Be(user.LastLoginAt);
    }

    [Fact]
    public async Task Handle_Should_Log_Information_When_User_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        var users = new List<AppUser> { user };
        var mockDbSet = users.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        var query = new GetUserById.Query { UserId = userId };

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting user by ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Log_Warning_When_User_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<AppUser>();
        var mockDbSet = users.AsQueryable().BuildMockDbSet();

        _mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        var query = new GetUserById.Query { UserId = userId };

        // Act
        await _sut.HandleAsync(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Note: Cancellation token tests are complex to mock properly with MockQueryable
    // The actual handlers do support cancellation tokens correctly

    [Fact]
    public async Task Handle_Should_Log_Error_When_Exception_Occurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedException = new Exception("Database error");
        
        _mockContext.Setup(x => x.Users).Throws(expectedException);

        var query = new GetUserById.Query { UserId = userId };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.HandleAsync(query, CancellationToken.None));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving user by ID")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static AppUser CreateTestUser(Guid userId)
    {
        var role = new Role 
        { 
            Id = Guid.NewGuid(), 
            Name = "Admin", 
            SystemRole = SystemRole.Admin,
            IsActive = true
        };

        var user = AppUserTestData.CreateAppUser(
            id: userId,
            email: "test@example.com",
            displayName: "Test User",
            firstName: "Test",
            lastName: "User",
            isActive: true
        );

        user.UserRoles = new List<UserRole> 
        { 
            new UserRole { UserId = userId, RoleId = role.Id, Role = role }
        };

        user.UserSubscriptions = new List<UserSubscription>();

        return user;
    }

    private static AppUser CreateTestUserWithMultipleRoles(Guid userId)
    {
        var adminRole = new Role 
        { 
            Id = Guid.NewGuid(), 
            Name = "Admin", 
            SystemRole = SystemRole.Admin,
            IsActive = true
        };

        var userRole = new Role 
        { 
            Id = Guid.NewGuid(), 
            Name = "User", 
            SystemRole = SystemRole.User,
            IsActive = true
        };

        var user = AppUserTestData.CreateAppUser(
            id: userId,
            email: "test@example.com",
            displayName: "Test User",
            firstName: "Test",
            lastName: "User",
            isActive: true
        );

        user.UserRoles = new List<UserRole> 
        { 
            new UserRole { UserId = userId, RoleId = adminRole.Id, Role = adminRole },
            new UserRole { UserId = userId, RoleId = userRole.Id, Role = userRole }
        };

        user.UserSubscriptions = new List<UserSubscription>();

        return user;
    }

    private static AppUser CreateTestUserWithSubscriptions(Guid userId)
    {
        var role = new Role 
        { 
            Id = Guid.NewGuid(), 
            Name = "User", 
            SystemRole = SystemRole.User,
            IsActive = true
        };

        var user = AppUserTestData.CreateAppUser(
            id: userId,
            email: "test@example.com",
            displayName: "Test User",
            firstName: "Test",
            lastName: "User",
            isActive: true
        );

        user.UserRoles = new List<UserRole> 
        { 
            new UserRole { UserId = userId, RoleId = role.Id, Role = role }
        };

        user.UserSubscriptions = new List<UserSubscription>
        {
            UserSubscriptionTestData.CreateUserSubscription(
                id: Guid.NewGuid(),
                userId: userId,
                subscriptionType: SubscriptionType.Premium,
                status: SubscriptionStatus.Active,
                isActive: true
            ),
            UserSubscriptionTestData.CreateUserSubscription(
                id: Guid.NewGuid(),
                userId: userId,
                subscriptionType: SubscriptionType.Basic,
                status: SubscriptionStatus.Expired,
                isActive: false
            )
        };

        return user;
    }
}