using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Persistence;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetBySystemRoleAsync(SystemRole systemRole);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Guid id);
}

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId);
    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId);
    Task<UserRole> AddAsync(UserRole userRole);
    Task UpdateAsync(UserRole userRole);
    Task DeleteAsync(Guid id);
}

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(Guid roleId);
    Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId);
    Task<RolePermission> AddAsync(RolePermission rolePermission);
    Task UpdateAsync(RolePermission rolePermission);
    Task DeleteAsync(Guid id);
    Task DeleteByRoleIdAsync(Guid roleId);
}

public interface IMenuRepository
{
    Task<Menu?> GetByIdAsync(Guid id);
    Task<Menu?> GetByNameAsync(string name);
    Task<IEnumerable<Menu>> GetAllAsync();
    Task<IEnumerable<Menu>> GetByParentIdAsync(Guid? parentId);
    Task<IEnumerable<Menu>> GetMenuTreeAsync();
    Task<IEnumerable<Menu>> GetUserMenusAsync(Guid userId);
    Task<Menu> AddAsync(Menu menu);
    Task UpdateAsync(Menu menu);
    Task DeleteAsync(Guid id);
}

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id);
    Task<Permission?> GetByKeyAsync(string permissionKey);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<Permission>> GetByMenuIdAsync(Guid menuId);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId);
    Task<Permission> AddAsync(Permission permission);
    Task UpdateAsync(Permission permission);
    Task DeleteAsync(Guid id);
}

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId);
    Task<IEnumerable<UserSubscription>> GetUserSubscriptionsAsync(Guid userId);
    Task<UserSubscription> AddAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
    Task DeleteAsync(Guid id);
}

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId);
    Task<UserProfile> AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task DeleteAsync(Guid id);
}
