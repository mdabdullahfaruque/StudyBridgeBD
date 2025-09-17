using StudyBridge.Domain.Common;

namespace StudyBridge.Domain.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
