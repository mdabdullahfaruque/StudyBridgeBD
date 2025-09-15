using StudyBridge.Domain.Common;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Domain.Entities;

public class Menu : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public MenuType MenuType { get; set; }
    public Guid? ParentMenuId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool HasCrudPermissions { get; set; } = false; // Option to have CRUD permissions

    // Navigation properties
    public Menu? ParentMenu { get; set; }
    public ICollection<Menu> SubMenus { get; set; } = new List<Menu>();
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}