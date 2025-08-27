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
        ILogger<PermissionService> logger)
    {
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(string userId, Permission permission)
    {
        try
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return userPermissions.Contains(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
    {
        try
        {
            var userRoles = await _userRoleRepository.GetUserRolesAsync(userId);
            var activeRoles = userRoles.Where(ur => ur.IsActive);

            var allPermissions = new HashSet<Permission>();

            foreach (var userRole in activeRoles)
            {
                // Get direct role permissions
                var rolePermissions = await _rolePermissionRepository.GetRolePermissionsAsync(userRole.RoleId);
                foreach (var rp in rolePermissions.Where(rp => rp.IsGranted))
                {
                    allPermissions.Add(rp.Permission);
                }

                // Get inherited permissions from role hierarchy
                var role = await _roleRepository.GetByIdAsync(userRole.RoleId);
                if (role != null)
                {
                    var inheritedRoles = GetInheritedRoles(role.SystemRole);
                    foreach (var inheritedRole in inheritedRoles)
                    {
                        var inheritedRoleEntity = await _roleRepository.GetBySystemRoleAsync(inheritedRole);
                        if (inheritedRoleEntity != null)
                        {
                            var inheritedPermissions = await _rolePermissionRepository.GetRolePermissionsAsync(inheritedRoleEntity.Id);
                            foreach (var ip in inheritedPermissions.Where(ip => ip.IsGranted))
                            {
                                allPermissions.Add(ip.Permission);
                            }
                        }
                    }
                }
            }

            return allPermissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return Enumerable.Empty<Permission>();
        }
    }

    public async Task<IEnumerable<SystemRole>> GetUserRolesAsync(string userId)
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

    public async Task<bool> AssignRoleToUserAsync(string userId, SystemRole role, string assignedBy)
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

    public async Task<bool> RemoveRoleFromUserAsync(string userId, SystemRole role)
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
                    Permission = permission,
                    IsGranted = true
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
                    Permission = permission,
                    IsGranted = true
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

    private static SystemRole[] GetInheritedRoles(SystemRole role)
    {
        return RoleHierarchy.GetValueOrDefault(role, Array.Empty<SystemRole>());
    }
}
