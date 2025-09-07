using FluentAssertions;
using Moq;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.Shared.Exceptions;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Application.Services;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.Tests.Unit.UserManagement.Application;

public class ProfileServiceTests
{
    private readonly Mock<IDispatcher> _mockDispatcher;
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        _mockDispatcher = new Mock<IDispatcher>();
        _sut = new ProfileService(_mockDispatcher.Object);
    }

    #region GetProfileAsync Tests

    [Fact]
    public async Task GetProfileAsync_WithValidUserId_ShouldReturnSuccessResult()
    {
        // Arrange
        const string userId = "test-user-id";
        var expectedResponse = new GetProfile.Response
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            AvatarUrl = "https://example.com/avatar.jpg",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastLoginAt = DateTime.UtcNow.AddHours(-2),
            IsActive = true,
            EmailConfirmed = true
        };
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.QueryAsync(It.Is<GetProfile.Query>(q => q.UserId == userId), cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.GetProfileAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedResponse);
        result.Message.Should().Be("Profile retrieved successfully");
        result.StatusCode.Should().Be(200);

        _mockDispatcher.Verify(x => x.QueryAsync(It.Is<GetProfile.Query>(q => q.UserId == userId), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetProfileAsync_WithNotFoundException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "non-existent-user-id";
        var exception = new NotFoundException("User profile not found");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.QueryAsync(It.IsAny<GetProfile.Query>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetProfileAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("User profile not found");
        result.StatusCode.Should().Be(404);
        result.Errors.Should().BeEquivalentTo(exception.Errors);
    }

    [Fact]
    public async Task GetProfileAsync_WithUnauthorizedException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "unauthorized-user-id";
        var exception = new UnauthorizedException("Access denied");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.QueryAsync(It.IsAny<GetProfile.Query>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetProfileAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Access denied");
        result.StatusCode.Should().Be(401);
        result.Errors.Should().BeEquivalentTo(exception.Errors);
    }

    [Fact]
    public async Task GetProfileAsync_WithStudyBridgeException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "test-user-id";
        var exception = new BusinessLogicException("Custom error");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.QueryAsync(It.IsAny<GetProfile.Query>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetProfileAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Custom error");
        result.StatusCode.Should().Be(422);
        result.Errors.Should().BeEquivalentTo(exception.Errors);
    }

    [Fact]
    public async Task GetProfileAsync_WithGenericException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "test-user-id";
        var exception = new Exception("Database connection failed");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.QueryAsync(It.IsAny<GetProfile.Query>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetProfileAsync(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed to get profile: Database connection failed");
        result.StatusCode.Should().Be(500);
    }

    #endregion

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        const string userId = "test-user-id";
        var request = new UpdateProfile.Request
        {
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last",
            AvatarUrl = "https://example.com/new-avatar.jpg"
        };
        var expectedResponse = new UpdateProfile.Response
        {
            Success = true,
            Message = "Profile updated successfully"
        };
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(It.Is<UpdateProfile.Command>(c => 
            c.UserId == userId &&
            c.DisplayName == request.DisplayName &&
            c.FirstName == request.FirstName &&
            c.LastName == request.LastName &&
            c.AvatarUrl == request.AvatarUrl), cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedResponse);
        result.Message.Should().Be("Profile updated successfully");
        result.StatusCode.Should().Be(200);

        _mockDispatcher.Verify(x => x.CommandAsync(It.Is<UpdateProfile.Command>(c => 
            c.UserId == userId &&
            c.DisplayName == request.DisplayName &&
            c.FirstName == request.FirstName &&
            c.LastName == request.LastName &&
            c.AvatarUrl == request.AvatarUrl), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNotFoundException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "non-existent-user-id";
        var request = new UpdateProfile.Request
        {
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };
        var exception = new NotFoundException("User not found");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(It.IsAny<UpdateProfile.Command>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.StatusCode.Should().Be(404);
        result.Errors.Should().BeEquivalentTo(exception.Errors);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithUnauthorizedException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "unauthorized-user-id";
        var request = new UpdateProfile.Request
        {
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };
        var exception = new UnauthorizedException("Access denied");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(It.IsAny<UpdateProfile.Command>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Access denied");
        result.StatusCode.Should().Be(401);
        result.Errors.Should().BeEquivalentTo(exception.Errors);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithValidationException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "test-user-id";
        var request = new UpdateProfile.Request
        {
            DisplayName = "",
            FirstName = "",
            LastName = ""
        };
        var exception = new ValidationException("Validation failed", new List<string> { "Display name is required" });
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(It.IsAny<UpdateProfile.Command>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.StatusCode.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(exception.Errors);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithStudyBridgeException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "test-user-id";
        var request = new UpdateProfile.Request
        {
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };
        var exception = new BusinessLogicException("Custom error");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(It.IsAny<UpdateProfile.Command>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Custom error");
        result.StatusCode.Should().Be(422);
        result.Errors.Should().BeEquivalentTo(exception.Errors);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithGenericException_ShouldReturnFailureResult()
    {
        // Arrange
        const string userId = "test-user-id";
        var request = new UpdateProfile.Request
        {
            DisplayName = "Updated Display Name",
            FirstName = "Updated First",
            LastName = "Updated Last"
        };
        var exception = new Exception("Database update failed");
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(It.IsAny<UpdateProfile.Command>(), cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed to update profile: Database update failed");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateProfileAsync_WithNullRequest_ShouldCreateCommandWithEmptyValues()
    {
        // Arrange
        const string userId = "test-user-id";
        var request = new UpdateProfile.Request
        {
            DisplayName = null!,
            FirstName = null!,
            LastName = null!,
            AvatarUrl = null
        };
        var expectedResponse = new UpdateProfile.Response
        {
            Success = true,
            Message = "Profile updated successfully"
        };
        var cancellationToken = CancellationToken.None;

        _mockDispatcher.Setup(x => x.CommandAsync(It.IsAny<UpdateProfile.Command>(), cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.UpdateProfileAsync(userId, request, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockDispatcher.Verify(x => x.CommandAsync(It.Is<UpdateProfile.Command>(c => 
            c.UserId == userId &&
            c.DisplayName == null &&
            c.FirstName == null &&
            c.LastName == null &&
            c.AvatarUrl == null), cancellationToken), Times.Once);
    }

    #endregion
}
