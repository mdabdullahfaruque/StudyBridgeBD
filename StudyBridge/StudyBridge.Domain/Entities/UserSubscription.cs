using StudyBridge.Domain.Common;

namespace StudyBridge.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public SubscriptionType SubscriptionType { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal Amount { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
}
