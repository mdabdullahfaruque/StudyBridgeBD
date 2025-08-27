using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Services;

public interface ISubscriptionService
{
    Task<bool> CreateSubscriptionAsync(string userId, SubscriptionType subscriptionType, decimal amount, DateTime endDate);
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);
    Task<IEnumerable<UserSubscription>> GetUserSubscriptionHistoryAsync(string userId);
    Task<bool> IsSubscriptionActiveAsync(string userId, SubscriptionType? requiredType = null);
    Task<bool> CancelSubscriptionAsync(string userId, string reason);
    Task<bool> RenewSubscriptionAsync(string userId, DateTime newEndDate, decimal amount);
}
