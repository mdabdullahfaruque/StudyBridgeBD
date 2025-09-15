using StudyBridge.Domain.Common;

namespace StudyBridge.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsGranted { get; set; } = true;
    public string? GrantedBy { get; set; } // Who granted this permission
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
