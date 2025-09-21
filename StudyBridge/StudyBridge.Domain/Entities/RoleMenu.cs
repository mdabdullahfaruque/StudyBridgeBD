using StudyBridge.Domain.Common;

namespace StudyBridge.Domain.Entities;

/// <summary>
/// Junction table for Role-Menu mapping
/// Simplified design where roles have direct access to menus
/// </summary>
public class RoleMenu : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid MenuId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? GrantedBy { get; set; } // Optional: User who granted this access

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
}