using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Tests.Unit.TestData;

public static class AppUserTestData
{
    public static AppUser CreateAppUser(
        Guid? id = null,
        string email = "test@example.com",
        string displayName = "Test User",
        string? firstName = "Test",
        string? lastName = "User",
        bool isActive = true,
        bool emailConfirmed = false,
        DateTime? createdAt = null,
        DateTime? lastLoginAt = null,
        string? passwordHash = null)
    {
        return new AppUser
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            FirstName = firstName,
            LastName = lastName,
            IsActive = isActive,
            EmailConfirmed = emailConfirmed,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            LastLoginAt = lastLoginAt,
            PasswordHash = passwordHash ?? "hashed_password",
            UserRoles = new List<UserRole>(),
            UserSubscriptions = new List<UserSubscription>()
        };
    }
}

public static class RoleTestData
{
    public static Role CreateRole(
        Guid? id = null,
        string name = "TestRole",
        string description = "Test role description",
        SystemRole systemRole = SystemRole.User,
        bool isActive = true,
        DateTime? createdAt = null)
    {
        return new Role
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description,
            SystemRole = systemRole,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UserRoles = new List<UserRole>(),
            RolePermissions = new List<RolePermission>()
        };
    }
}

public static class MenuTestData
{
    public static Menu CreateMenu(
        Guid? id = null,
        string name = "TestMenu",
        string displayName = "Test Menu",
        string? description = "Test menu description",
        string? icon = "test-icon",
        string? route = "/test",
        MenuType menuType = MenuType.Admin,
        int sortOrder = 1,
        bool isActive = true,
        DateTime? createdAt = null)
    {
        return new Menu
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            Description = description,
            Icon = icon,
            Route = route,
            MenuType = menuType,
            SortOrder = sortOrder,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Permissions = new List<Permission>()
        };
    }
}

public static class PermissionTestData
{
    public static Permission CreatePermission(
        Guid? id = null,
        string key = "test.permission",
        string displayName = "Test Permission",
        string? description = "Test permission description",
        PermissionType permissionType = PermissionType.View,
        Menu? menu = null,
        bool isActive = true,
        bool isSystemPermission = false,
        DateTime? createdAt = null)
    {
        var permission = new Permission
        {
            Id = id ?? Guid.NewGuid(),
            PermissionKey = key,
            DisplayName = displayName,
            Description = description,
            PermissionType = permissionType,
            IsActive = isActive,
            IsSystemPermission = isSystemPermission,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            RolePermissions = new List<RolePermission>()
        };

        if (menu != null)
        {
            permission.MenuId = menu.Id;
            permission.Menu = menu;
        }

        return permission;
    }
}

public static class UserSubscriptionTestData
{
    public static UserSubscription CreateUserSubscription(
        Guid? id = null,
        Guid? userId = null,
        SubscriptionType subscriptionType = SubscriptionType.Basic,
        SubscriptionStatus status = SubscriptionStatus.Active,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool isActive = true,
        DateTime? createdAt = null)
    {
        var start = startDate ?? DateTime.UtcNow;
        var end = endDate ?? start.AddMonths(1);

        return new UserSubscription
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            SubscriptionType = subscriptionType,
            Status = status,
            StartDate = start,
            EndDate = end,
            IsActive = isActive,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }
}