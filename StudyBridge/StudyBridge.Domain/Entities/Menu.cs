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

    // Navigation properties
    public Menu? ParentMenu { get; set; }
    public ICollection<Menu> SubMenus { get; set; } = new List<Menu>();
    public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}