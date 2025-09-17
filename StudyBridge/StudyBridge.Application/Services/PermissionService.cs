using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace StudyBridge.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILogger<PermissionService> _logger;

    private static readonly Dictionary<SystemRole, SystemRole[]> RoleHierarchy = new()
    {
        { SystemRole.SuperAdmin, new[] { SystemRole.Admin, SystemRole.Finance, SystemRole.Accounts, SystemRole.ContentManager, SystemRole.User } },
        { SystemRole.Admin, new[] { SystemRole.Finance, SystemRole.Accounts, SystemRole.ContentManager, SystemRole.User } },
        { SystemRole.Finance, new[] { SystemRole.User } },
        { SystemRole.Accounts, new[] { SystemRole.User } },
        { SystemRole.ContentManager, new[] { SystemRole.User } },
        { SystemRole.User, Array.Empty<SystemRole>() }
    };

    public PermissionService(
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IMenuRepository menuRepository,
        IPermissionRepository permissionRepository,
        ILogger<PermissionService> logger)
    {
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _menuRepository = menuRepository;
        _permissionRepository = permissionRepository;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Permission permission)
    {
        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return userPermissions.Any(p => p.Id == permission.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission.PermissionKey, userId);
            return false;
        }
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionKey)
    {
        try
        {
            var permission = await _permissionRepository.GetByKeyAsync(permissionKey);
            if (permission == null)
            {
                _logger.LogWarning("Permission with key {PermissionKey} not found", permissionKey);
                return false;
            }

            return await HasPermissionAsync(userId, permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission key {PermissionKey} for user {UserId}", permissionKey, userId);
            return false;
        }
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        try
        {
            return await _permissionRepository.GetUserPermissionsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return Enumerable.Empty<Permission>();
        }
    }

    public async Task<IEnumerable<SystemRole>> GetUserRolesAsync(Guid userId)
    {
        try
        {
            var userRoles = await _userRoleRepository.GetUserRolesAsync(userId);
            var roleIds = userRoles.Where(ur => ur.IsActive).Select(ur => ur.RoleId);
            
            var roles = new List<SystemRole>();
            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role != null)
                {
                    roles.Add(role.SystemRole);
                }
            }
            
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return Enumerable.Empty<SystemRole>();
        }
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, SystemRole role, string assignedBy)
    {
        try
        {
            var roleEntity = await _roleRepository.GetBySystemRoleAsync(role);
            if (roleEntity == null)
            {
                _logger.LogWarning("Role {Role} not found", role);
                return false;
            }

            var existingUserRole = await _userRoleRepository.GetUserRoleAsync(userId, roleEntity.Id);
            if (existingUserRole != null)
            {
                if (!existingUserRole.IsActive)
                {
                    existingUserRole.IsActive = true;
                    existingUserRole.AssignedAt = DateTime.UtcNow;
                    existingUserRole.AssignedBy = assignedBy;
                    await _userRoleRepository.UpdateAsync(existingUserRole);
                }
                return true;
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleEntity.Id,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRoleRepository.AddAsync(userRole);
            _logger.LogInformation("Role {Role} assigned to user {UserId} by {AssignedBy}", role, userId, assignedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", role, userId);
            return false;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, SystemRole role)
    {
        try
        {
            var roleEntity = await _roleRepository.GetBySystemRoleAsync(role);
            if (roleEntity == null)
            {
                return false;
            }

            var userRole = await _userRoleRepository.GetUserRoleAsync(userId, roleEntity.Id);
            if (userRole != null)
            {
                userRole.IsActive = false;
                await _userRoleRepository.UpdateAsync(userRole);
                _logger.LogInformation("Role {Role} removed from user {UserId}", role, userId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {Role} from user {UserId}", role, userId);
            return false;
        }
    }

    public async Task<bool> CreateRoleAsync(string name, SystemRole systemRole, IEnumerable<Permission> permissions)
    {
        try
        {
            var existingRole = await _roleRepository.GetBySystemRoleAsync(systemRole);
            if (existingRole != null)
            {
                _logger.LogWarning("Role {SystemRole} already exists", systemRole);
                return false;
            }

            var role = new Role
            {
                Name = name,
                SystemRole = systemRole,
                Description = $"System role: {systemRole}",
                IsActive = true
            };

            var createdRole = await _roleRepository.AddAsync(role);

            foreach (var permission in permissions)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = createdRole.Id,
                    PermissionId = permission.Id,
                    IsGranted = true,
                    GrantedAt = DateTime.UtcNow,
                    GrantedBy = "System" // TODO: Pass actual user who is creating the role
                };
                await _rolePermissionRepository.AddAsync(rolePermission);
            }

            _logger.LogInformation("Role {RoleName} created with {PermissionCount} permissions", name, permissions.Count());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role {RoleName}", name);
            return false;
        }
    }

    public async Task<bool> UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Permission> permissions)
    {
        try
        {
            // Remove existing permissions
            await _rolePermissionRepository.DeleteByRoleIdAsync(roleId);

            // Add new permissions
            foreach (var permission in permissions)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permission.Id,
                    IsGranted = true,
                    GrantedAt = DateTime.UtcNow,
                    GrantedBy = "System" // TODO: Pass actual user who is updating permissions
                };
                await _rolePermissionRepository.AddAsync(rolePermission);
            }

            _logger.LogInformation("Updated permissions for role {RoleId}", roleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permissions for role {RoleId}", roleId);
            return false;
        }
    }

    public async Task<IEnumerable<Menu>> GetUserMenusAsync(Guid userId)
    {
        try
        {
            return await _menuRepository.GetUserMenusAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting menus for user {UserId}", userId);
            return Enumerable.Empty<Menu>();
        }
    }

    public async Task<Permission?> GetPermissionByKeyAsync(string permissionKey)
    {
        try
        {
            return await _permissionRepository.GetByKeyAsync(permissionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permission by key {PermissionKey}", permissionKey);
            return null;
        }
    }

    private static SystemRole[] GetInheritedRoles(SystemRole role)
    {
        return RoleHierarchy.GetValueOrDefault(role, Array.Empty<SystemRole>());
    }
}
