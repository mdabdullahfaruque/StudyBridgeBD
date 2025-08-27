using Moq;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Tests.Unit.TestData;
using FluentAssertions;

namespace StudyBridge.Tests.Unit.Services;

public class SubscriptionServiceTests
{
    private readonly Mock<IUserSubscriptionRepository> _mockSubscriptionRepository;
    private readonly Mock<ILogger<SubscriptionService>> _mockLogger;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _mockSubscriptionRepository = new Mock<IUserSubscriptionRepository>();
        _mockLogger = new Mock<ILogger<SubscriptionService>>();
        
        _sut = new SubscriptionService(
            _mockSubscriptionRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_WhenValid_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const SubscriptionType subscriptionType = SubscriptionType.Premium;
        const decimal amount = 99.99m;
        var endDate = DateTime.UtcNow.AddDays(30);

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync((UserSubscription?)null);

        _mockSubscriptionRepository.Setup(x => x.AddAsync(It.IsAny<UserSubscription>()))
            .ReturnsAsync((UserSubscription s) => s);

        // Act
        var result = await _sut.CreateSubscriptionAsync(userId, subscriptionType, amount, endDate);

        // Assert
        result.Should().BeTrue();
        _mockSubscriptionRepository.Verify(x => x.AddAsync(It.Is<UserSubscription>(s => 
            s.UserId == userId && 
            s.SubscriptionType == subscriptionType && 
            s.Amount == amount &&
            s.IsActive)), Times.Once);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_WhenExistingActiveSubscription_ShouldDeactivateOldAndCreateNew()
    {
        // Arrange
        const string userId = "test-user-id";
        const SubscriptionType subscriptionType = SubscriptionType.Premium;
        const decimal amount = 99.99m;
        var endDate = DateTime.UtcNow.AddDays(30);

        var existingSubscription = TestDataBuilder.Subscriptions.ActivePremium(userId);

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync(existingSubscription);

        _mockSubscriptionRepository.Setup(x => x.AddAsync(It.IsAny<UserSubscription>()))
            .ReturnsAsync((UserSubscription s) => s);

        // Act
        var result = await _sut.CreateSubscriptionAsync(userId, subscriptionType, amount, endDate);

        // Assert
        result.Should().BeTrue();
        _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<UserSubscription>(s => !s.IsActive)), Times.Once);
        _mockSubscriptionRepository.Verify(x => x.AddAsync(It.IsAny<UserSubscription>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptionAsync_WhenSubscriptionExists_ShouldReturnSubscription()
    {
        // Arrange
        const string userId = "test-user-id";
        var activeSubscription = TestDataBuilder.Subscriptions.ActivePremium(userId);

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync(activeSubscription);

        // Act
        var result = await _sut.GetActiveSubscriptionAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.SubscriptionType.Should().Be(SubscriptionType.Premium);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveSubscriptionAsync_WhenSubscriptionExpired_ShouldReturnNull()
    {
        // Arrange
        const string userId = "test-user-id";
        var expiredSubscription = TestDataBuilder.Subscriptions.ExpiredBasic(userId);
        expiredSubscription.IsActive = true; // Simulate DB state before expiry check

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync(expiredSubscription);

        // Act
        var result = await _sut.GetActiveSubscriptionAsync(userId);

        // Assert
        result.Should().BeNull();
        _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<UserSubscription>(s => !s.IsActive)), Times.Once);
    }

    [Fact]
    public async Task IsSubscriptionActiveAsync_WhenUserHasActiveSubscription_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        var activeSubscription = TestDataBuilder.Subscriptions.ActivePremium(userId);

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync(activeSubscription);

        // Act
        var result = await _sut.IsSubscriptionActiveAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSubscriptionActiveAsync_WithSpecificType_WhenUserHasMatchingSubscription_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        var activeSubscription = TestDataBuilder.Subscriptions.ActivePremium(userId);

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync(activeSubscription);

        // Act
        var result = await _sut.IsSubscriptionActiveAsync(userId, SubscriptionType.VocabularyOnly);

        // Assert
        result.Should().BeTrue(); // Premium includes all modules
    }

    [Fact]
    public async Task CancelSubscriptionAsync_WhenSubscriptionExists_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const string reason = "User requested cancellation";
        var activeSubscription = TestDataBuilder.Subscriptions.ActivePremium(userId);

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync(activeSubscription);

        // Act
        var result = await _sut.CancelSubscriptionAsync(userId, reason);

        // Assert
        result.Should().BeTrue();
        _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<UserSubscription>(s => 
            !s.IsActive && s.Notes!.Contains(reason))), Times.Once);
    }

    [Fact]
    public async Task RenewSubscriptionAsync_WhenSubscriptionExists_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const decimal renewalAmount = 99.99m;
        var newEndDate = DateTime.UtcNow.AddDays(60);
        var activeSubscription = TestDataBuilder.Subscriptions.ActivePremium(userId);

        _mockSubscriptionRepository.Setup(x => x.GetActiveSubscriptionAsync(userId))
            .ReturnsAsync(activeSubscription);

        // Act
        var result = await _sut.RenewSubscriptionAsync(userId, newEndDate, renewalAmount);

        // Assert
        result.Should().BeTrue();
        _mockSubscriptionRepository.Verify(x => x.UpdateAsync(It.Is<UserSubscription>(s => 
            s.EndDate == newEndDate)), Times.Once);
    }
}
