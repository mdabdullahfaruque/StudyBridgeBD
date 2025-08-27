using StudyBridge.Domain.Entities;

namespace StudyBridge.Tests.Unit.TestData;

public static class TestDataBuilder
{
    // Predefined GUIDs for consistent testing
    public static readonly Guid SuperAdminRoleId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AdminRoleId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid UserRoleId = new("66666666-6666-6666-6666-666666666666");

    public static class Roles
    {
        public static Role SuperAdmin() => new()
        {
            Id = SuperAdminRoleId,
            Name = "Super Administrator",
            SystemRole = SystemRole.SuperAdmin,
            Description = "System role: SuperAdmin",
            IsActive = true
        };

        public static Role Admin() => new()
        {
            Id = AdminRoleId,
            Name = "Administrator",
            SystemRole = SystemRole.Admin,
            Description = "System role: Admin",
            IsActive = true
        };

        public static Role User() => new()
        {
            Id = UserRoleId,
            Name = "User",
            SystemRole = SystemRole.User,
            Description = "System role: User",
            IsActive = true
        };
    }

    public static class UserRoles
    {
        public static UserRole AdminUserRole(string userId) => new()
        {
            Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            UserId = userId,
            RoleId = AdminRoleId, // Admin
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            Role = Roles.Admin()
        };

        public static UserRole UserUserRole(string userId) => new()
        {
            Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            UserId = userId,
            RoleId = UserRoleId, // User
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            Role = Roles.User()
        };
    }

    public static class RolePermissions
    {
        public static RolePermission AdminViewUsers() => new()
        {
            Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            RoleId = AdminRoleId, // Admin
            Permission = Permission.ViewUsers,
            IsGranted = true
        };

        public static RolePermission AdminCreateUsers() => new()
        {
            Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            RoleId = AdminRoleId, // Admin
            Permission = Permission.CreateUsers,
            IsGranted = true
        };

        public static RolePermission UserViewContent() => new()
        {
            Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            RoleId = UserRoleId, // User
            Permission = Permission.ViewContent,
            IsGranted = true
        };
    }

    public static class Subscriptions
    {
        public static UserSubscription ActivePremium(string userId) => new()
        {
            Id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            UserId = userId,
            SubscriptionType = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            Amount = 99.99m,
            IsActive = true
        };

        public static UserSubscription ExpiredBasic(string userId) => new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            UserId = userId,
            SubscriptionType = SubscriptionType.Basic,
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Amount = 49.99m,
            IsActive = false
        };
    }

    public static class Profiles
    {
        public static UserProfile Complete(string userId) => new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000002"),
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            Country = "USA",
            City = "New York",
            IsEmailVerified = true,
            IsPhoneVerified = true
        };
    }
}
