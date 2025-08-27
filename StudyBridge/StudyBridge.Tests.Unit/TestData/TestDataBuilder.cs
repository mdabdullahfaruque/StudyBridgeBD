using StudyBridge.Domain.Entities;

namespace StudyBridge.Tests.Unit.TestData;

public static class TestDataBuilder
{
    public static class Roles
    {
        public static Role SuperAdmin() => new()
        {
            Id = 1,
            Name = "Super Administrator",
            SystemRole = SystemRole.SuperAdmin,
            Description = "System role: SuperAdmin",
            IsActive = true
        };

        public static Role Admin() => new()
        {
            Id = 2,
            Name = "Administrator",
            SystemRole = SystemRole.Admin,
            Description = "System role: Admin",
            IsActive = true
        };

        public static Role User() => new()
        {
            Id = 6,
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
            Id = 1,
            UserId = userId,
            RoleId = 2, // Admin
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            Role = Roles.Admin()
        };

        public static UserRole UserUserRole(string userId) => new()
        {
            Id = 2,
            UserId = userId,
            RoleId = 6, // User
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            Role = Roles.User()
        };
    }

    public static class RolePermissions
    {
        public static RolePermission AdminViewUsers() => new()
        {
            Id = 1,
            RoleId = 2, // Admin
            Permission = Permission.ViewUsers,
            IsGranted = true
        };

        public static RolePermission AdminCreateUsers() => new()
        {
            Id = 2,
            RoleId = 2, // Admin
            Permission = Permission.CreateUsers,
            IsGranted = true
        };

        public static RolePermission UserViewContent() => new()
        {
            Id = 3,
            RoleId = 6, // User
            Permission = Permission.ViewContent,
            IsGranted = true
        };
    }

    public static class Subscriptions
    {
        public static UserSubscription ActivePremium(string userId) => new()
        {
            Id = 1,
            UserId = userId,
            SubscriptionType = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            Amount = 99.99m,
            IsActive = true
        };

        public static UserSubscription ExpiredBasic(string userId) => new()
        {
            Id = 2,
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
            Id = 1,
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
