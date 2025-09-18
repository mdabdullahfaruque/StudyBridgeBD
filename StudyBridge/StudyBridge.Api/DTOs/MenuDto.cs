namespace StudyBridge.Api.DTOs;

public class MenuDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public string? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int MenuType { get; set; }
    public List<string> RequiredPermissions { get; set; } = new();
    public List<MenuDto>? Children { get; set; }
}