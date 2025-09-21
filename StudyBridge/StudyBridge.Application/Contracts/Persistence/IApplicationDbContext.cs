using Microsoft.EntityFrameworkCore;
using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Persistence;

public interface IApplicationDbContext
{
    DbSet<AppUser> Users { get; set; }
    DbSet<Role> Roles { get; set; }
    DbSet<UserRole> UserRoles { get; set; }
    DbSet<RoleMenu> RoleMenus { get; set; }
    DbSet<Menu> Menus { get; set; }
    DbSet<UserProfile> UserProfiles { get; set; }
    DbSet<UserSubscription> UserSubscriptions { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
