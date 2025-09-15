using StudyBridge.Domain.Common;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Domain.Entities;

public class Permission : BaseEntity
{
    public Guid MenuId { get; set; }
    public PermissionType PermissionType { get; set; }
    public string PermissionKey { get; set; } = string.Empty; // e.g., 'users.view', 'reports.create'
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemPermission { get; set; } = false; // Cannot be deleted if true

    // Navigation properties
    public Menu Menu { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
