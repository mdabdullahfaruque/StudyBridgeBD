using Microsoft.EntityFrameworkCore;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Infrastructure.Data;

namespace StudyBridge.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Role?> GetBySystemRoleAsync(SystemRole systemRole)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.SystemRole == systemRole);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .Where(r => r.IsActive)
            .ToListAsync();
    }

    public async Task<Role> AddAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task UpdateAsync(Role role)
    {
        role.UpdatedAt = DateTime.UtcNow;
        _context.Entry(role).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role != null)
        {
            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

public class UserRoleRepository : IUserRoleRepository
{
    private readonly AppDbContext _context;

    public UserRoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
    }

    public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public async Task<UserRole> AddAsync(UserRole userRole)
    {
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
        return userRole;
    }

    public async Task UpdateAsync(UserRole userRole)
    {
        userRole.UpdatedAt = DateTime.UtcNow;
        _context.Entry(userRole).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var userRole = await _context.UserRoles.FindAsync(id);
        if (userRole != null)
        {
            userRole.IsActive = false;
            userRole.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AppDbContext _context;

    public RolePermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => rp.IsGranted)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    public async Task<RolePermission> AddAsync(RolePermission rolePermission)
    {
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();
        return rolePermission;
    }

    public async Task UpdateAsync(RolePermission rolePermission)
    {
        rolePermission.UpdatedAt = DateTime.UtcNow;
        _context.Entry(rolePermission).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var rolePermission = await _context.RolePermissions.FindAsync(id);
        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByRoleIdAsync(Guid roleId)
    {
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(rolePermissions);
        await _context.SaveChangesAsync();
    }
}

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly AppDbContext _context;

    public UserSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId)
    {
        return await _context.UserSubscriptions
            .Where(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserSubscription>> GetUserSubscriptionsAsync(Guid userId)
    {
        return await _context.UserSubscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserSubscription> AddAsync(UserSubscription subscription)
    {
        _context.UserSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task UpdateAsync(UserSubscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        _context.Entry(subscription).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var subscription = await _context.UserSubscriptions.FindAsync(id);
        if (subscription != null)
        {
            _context.UserSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }
    }
}

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;

    public UserProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserProfile> AddAsync(UserProfile profile)
    {
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateAsync(UserProfile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        _context.Entry(profile).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var profile = await _context.UserProfiles.FindAsync(id);
        if (profile != null)
        {
            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();
        }
    }
}
