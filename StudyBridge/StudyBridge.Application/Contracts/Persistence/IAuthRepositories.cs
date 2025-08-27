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
    Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId);
    Task<UserRole?> GetUserRoleAsync(string userId, Guid roleId);
    Task<UserRole> AddAsync(UserRole userRole);
    Task UpdateAsync(UserRole userRole);
    Task DeleteAsync(Guid id);
}

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(Guid roleId);
    Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(string userId);
    Task<RolePermission> AddAsync(RolePermission rolePermission);
    Task UpdateAsync(RolePermission rolePermission);
    Task DeleteAsync(Guid id);
    Task DeleteByRoleIdAsync(Guid roleId);
}

public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);
    Task<IEnumerable<UserSubscription>> GetUserSubscriptionsAsync(string userId);
    Task<UserSubscription> AddAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
    Task DeleteAsync(Guid id);
}

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<UserProfile> AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task DeleteAsync(Guid id);
}
