using StudyBridge.Domain.Common;

namespace StudyBridge.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Permission Permission { get; set; }
    public bool IsGranted { get; set; } = true;
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
}
