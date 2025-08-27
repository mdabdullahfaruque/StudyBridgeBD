using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace StudyBridge.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IUserSubscriptionRepository subscriptionRepository,
        ILogger<SubscriptionService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
    }

    public async Task<bool> CreateSubscriptionAsync(string userId, SubscriptionType subscriptionType, decimal amount, DateTime endDate)
    {
        try
        {
            // Deactivate any existing active subscription
            var existingSubscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (existingSubscription != null)
            {
                existingSubscription.IsActive = false;
                await _subscriptionRepository.UpdateAsync(existingSubscription);
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionType = subscriptionType,
                StartDate = DateTime.UtcNow,
                EndDate = endDate,
                Amount = amount,
                IsActive = true
            };

            await _subscriptionRepository.AddAsync(subscription);
            _logger.LogInformation("Created subscription {SubscriptionType} for user {UserId}", subscriptionType, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for user {UserId}", userId);
            return false;
        }
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId);
            
            // Check if subscription is still valid
            if (subscription != null && subscription.EndDate < DateTime.UtcNow)
            {
                subscription.IsActive = false;
                await _subscriptionRepository.UpdateAsync(subscription);
                return null;
            }

            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscription for user {UserId}", userId);
            return null;
        }
    }

    public async Task<IEnumerable<UserSubscription>> GetUserSubscriptionHistoryAsync(string userId)
    {
        try
        {
            return await _subscriptionRepository.GetUserSubscriptionsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription history for user {UserId}", userId);
            return Enumerable.Empty<UserSubscription>();
        }
    }

    public async Task<bool> IsSubscriptionActiveAsync(string userId, SubscriptionType? requiredType = null)
    {
        try
        {
            var subscription = await GetActiveSubscriptionAsync(userId);
            
            if (subscription == null || !subscription.IsActive)
                return false;

            if (requiredType.HasValue)
            {
                return subscription.SubscriptionType == requiredType.Value || 
                       subscription.SubscriptionType == SubscriptionType.AllModules ||
                       subscription.SubscriptionType == SubscriptionType.Premium;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subscription status for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string userId, string reason)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (subscription != null)
            {
                subscription.IsActive = false;
                subscription.Notes = $"Cancelled: {reason}";
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("Cancelled subscription for user {UserId}. Reason: {Reason}", userId, reason);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> RenewSubscriptionAsync(string userId, DateTime newEndDate, decimal amount)
    {
        try
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (subscription != null)
            {
                subscription.EndDate = newEndDate;
                subscription.Amount += amount;
                subscription.Notes = $"Renewed on {DateTime.UtcNow:yyyy-MM-dd}";
                await _subscriptionRepository.UpdateAsync(subscription);
                
                _logger.LogInformation("Renewed subscription for user {UserId} until {EndDate}", userId, newEndDate);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing subscription for user {UserId}", userId);
            return false;
        }
    }
}
