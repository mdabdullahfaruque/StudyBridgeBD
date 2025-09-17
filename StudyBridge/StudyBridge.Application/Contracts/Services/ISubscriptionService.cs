using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Services;

public interface ISubscriptionService
{
    Task<bool> CreateSubscriptionAsync(Guid userId, SubscriptionType subscriptionType, decimal amount, DateTime endDate);
    Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId);
    Task<IEnumerable<UserSubscription>> GetUserSubscriptionHistoryAsync(Guid userId);
    Task<bool> IsSubscriptionActiveAsync(Guid userId, SubscriptionType? requiredType = null);
    Task<bool> CancelSubscriptionAsync(Guid userId, string reason);
    Task<bool> RenewSubscriptionAsync(Guid userId, DateTime newEndDate, decimal amount);
}
