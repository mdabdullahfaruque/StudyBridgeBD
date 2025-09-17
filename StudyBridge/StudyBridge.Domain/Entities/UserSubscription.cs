using StudyBridge.Domain.Common;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }
    public SubscriptionType SubscriptionType { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal Amount { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
}
