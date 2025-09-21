using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Persistence;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
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

public interface IRoleMenuRepository
{
    Task<IEnumerable<RoleMenu>> GetRoleMenusAsync(Guid roleId);
    Task<IEnumerable<Menu>> GetMenusByUserIdAsync(Guid userId);
    Task<RoleMenu> AddAsync(RoleMenu roleMenu);
    Task UpdateAsync(RoleMenu roleMenu);
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
