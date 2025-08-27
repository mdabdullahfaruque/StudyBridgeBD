using StudyBridge.Domain.Common;

namespace StudyBridge.Domain.Entities;

public class UserRole : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
}
