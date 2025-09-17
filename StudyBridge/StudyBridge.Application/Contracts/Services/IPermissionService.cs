using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Services;

public interface IPermissionService
{
    // Permission checking methods
    Task<bool> HasPermissionAsync(Guid userId, Permission permission);
    Task<bool> HasPermissionAsync(Guid userId, string permissionKey);
    
    // User permission and role management
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId);
    Task<bool> AssignRoleToUserAsync(Guid userId, SystemRole role, string assignedBy);
    Task<bool> RemoveRoleFromUserAsync(Guid userId, SystemRole role);
    Task<IEnumerable<SystemRole>> GetUserRolesAsync(Guid userId);
    
    // Role management
    Task<bool> CreateRoleAsync(string name, SystemRole systemRole, IEnumerable<Permission> permissions);
    Task<bool> UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Permission> permissions);
    
    // Menu and permission management
    Task<IEnumerable<Menu>> GetUserMenusAsync(Guid userId);
    Task<Permission?> GetPermissionByKeyAsync(string permissionKey);
}
