using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Services;

public interface IPermissionService
{
    // Permission checking methods
    Task<bool> HasPermissionAsync(string userId, Permission permission);
    Task<bool> HasPermissionAsync(string userId, string permissionKey);
    
    // User permission and role management
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
    Task<bool> AssignRoleToUserAsync(string userId, SystemRole role, string assignedBy);
    Task<bool> RemoveRoleFromUserAsync(string userId, SystemRole role);
    Task<IEnumerable<SystemRole>> GetUserRolesAsync(string userId);
    
    // Role management
    Task<bool> CreateRoleAsync(string name, SystemRole systemRole, IEnumerable<Permission> permissions);
    Task<bool> UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Permission> permissions);
    
    // Menu and permission management
    Task<IEnumerable<Menu>> GetUserMenusAsync(string userId);
    Task<Permission?> GetPermissionByKeyAsync(string permissionKey);
}
