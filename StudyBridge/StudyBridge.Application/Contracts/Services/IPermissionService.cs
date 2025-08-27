using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string userId, Permission permission);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
    Task<bool> AssignRoleToUserAsync(string userId, SystemRole role, string assignedBy);
    Task<bool> RemoveRoleFromUserAsync(string userId, SystemRole role);
    Task<IEnumerable<SystemRole>> GetUserRolesAsync(string userId);
    Task<bool> CreateRoleAsync(string name, SystemRole systemRole, IEnumerable<Permission> permissions);
    Task<bool> UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Permission> permissions);
}
