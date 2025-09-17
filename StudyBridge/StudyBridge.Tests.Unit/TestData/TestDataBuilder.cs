using StudyBridge.Domain.Entities;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.Tests.Unit.TestData;

public static class TestDataBuilder
{
    // Predefined GUIDs for consistent testing
    public static readonly Guid SuperAdminRoleId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AdminRoleId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid UserRoleId = new("66666666-6666-6666-6666-666666666666");
    
    // User IDs
    public static readonly Guid TestUserId = new("77777777-7777-7777-7777-777777777777");
    public static readonly string TestUserIdString = TestUserId.ToString();

    public static class Users
    {
        public static AppUser LocalUser() => new()
        {
            Id = TestUserId,
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "$2a$11$DummyHashForTestingPurposes123456789",
            EmailConfirmed = true,
            IsActive = true,
            LoginProvider = StudyBridge.Domain.Enums.LoginProvider.Local,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Alias for backward compatibility
        public static AppUser ValidUser() => LocalUser();

        public static AppUser InactiveUser() => new()
        {
            Id = new Guid("88888888-8888-8888-8888-888888888888"),
            Email = "inactive@example.com",
            DisplayName = "Inactive User",
            FirstName = "Inactive",
            LastName = "User",
            PasswordHash = "hashed_password_456",
            EmailConfirmed = false,
            IsActive = false,
            LoginProvider = StudyBridge.Domain.Enums.LoginProvider.Local,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        public static AppUser GoogleUser() => new()
        {
            Id = new Guid("99999999-9999-9999-9999-999999999999"),
            Email = "google@example.com",
            DisplayName = "Google User",
            FirstName = "Google",
            LastName = "User",
            GoogleSub = "12345678901234567890",
            AvatarUrl = "https://lh3.googleusercontent.com/a/default-user",
            PasswordHash = "", // OAuth users don't have passwords - use empty string instead of null
            EmailConfirmed = true,
            IsActive = true,
            LoginProvider = StudyBridge.Domain.Enums.LoginProvider.Google,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    public static class Commands
    {
        public static class Authentication
        {
            public static Register.Command ValidRegisterCommand() => new()
            {
                Email = "newuser@example.com",
                Password = "SecurePassword123!",
                FirstName = "New",
                LastName = "User",
                DisplayName = "New User"
            };

            public static Register.Command InvalidRegisterCommand() => new()
            {
                Email = "invalid-email",
                Password = "weak",
                FirstName = "",
                LastName = "",
                DisplayName = ""
            };

            public static Login.Command ValidLoginCommand() => new()
            {
                Email = "test@example.com",
                Password = "CorrectPassword123!"
            };

            public static Login.Command InvalidLoginCommand() => new()
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            public static GoogleLogin.Command ValidGoogleLoginCommand() => new()
            {
                IdToken = "valid_google_id_token_123",
                Email = "google@example.com",
                DisplayName = "Google User",
                FirstName = "Google",
                LastName = "User",
                GoogleSub = "12345678901234567890",
                AvatarUrl = "https://lh3.googleusercontent.com/a/default-user"
            };

            public static ChangePassword.Command ValidChangePasswordCommand() => new()
            {
                UserId = TestUserIdString,
                CurrentPassword = "CurrentPassword123!",
                NewPassword = "NewSecurePassword456!"
            };

            public static ChangePassword.Command InvalidChangePasswordCommand() => new()
            {
                UserId = TestUserIdString,
                CurrentPassword = "WrongCurrentPassword",
                NewPassword = "NewSecurePassword456!"
            };
        }
    }

    public static class Responses
    {
        public static class Authentication
        {
            public static Register.Response SuccessfulRegisterResponse() => new()
            {
                Token = "jwt_token_123",
                RefreshToken = "refresh_token_123",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Email = "newuser@example.com",
                DisplayName = "New User",
                UserId = TestUserIdString,
                Roles = new List<string> { "User" },
                RequiresEmailConfirmation = true
            };

            public static Login.Response SuccessfulLoginResponse() => new()
            {
                Token = "jwt_token_456",
                RefreshToken = "refresh_token_456",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Email = "test@example.com",
                DisplayName = "Test User",
                UserId = TestUserIdString,
                Roles = new List<string> { "User" }
            };

            public static GoogleLogin.Response SuccessfulGoogleLoginResponse() => new()
            {
                Token = "jwt_token_789",
                RefreshToken = "refresh_token_789",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Email = "google@example.com",
                DisplayName = "Google User",
                UserId = "99999999-9999-9999-9999-999999999999",
                Roles = new List<string> { "User" },
                IsNewUser = false
            };

            public static ChangePassword.Response SuccessfulChangePasswordResponse() => new()
            {
                Success = true,
                Message = "Password changed successfully"
            };
        }
    }

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
        public static UserRole AdminUserRole(Guid userId) => new()
        {
            Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            UserId = userId,
            RoleId = AdminRoleId, // Admin
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            Role = Roles.Admin()
        };

        public static UserRole UserUserRole(Guid userId) => new()
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
        // TODO: Update after new RBAC implementation
        // public static RolePermission AdminViewUsers() => new()
        // {
        //     Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
        //     RoleId = AdminRoleId, // Admin
        //     Permission = SystemPermission.ViewUsers,
        //     IsGranted = true
        // };

        // public static RolePermission AdminCreateUsers() => new()
        // {
        //     Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
        //     RoleId = AdminRoleId, // Admin
        //     Permission = SystemPermission.CreateUsers,
        //     IsGranted = true
        // };

        // public static RolePermission UserViewContent() => new()
        // {
        //     Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
        //     RoleId = UserRoleId, // User
        //     Permission = SystemPermission.ViewContent,
        //     IsGranted = true
        // };
    }

    public static class Subscriptions
    {
        public static UserSubscription ActivePremium(Guid userId) => new()
        {
            Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            UserId = userId,
            SubscriptionType = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            Amount = 99.99m,
            IsActive = true
        };

        public static UserSubscription ExpiredBasic(Guid userId) => new()
        {
            Id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
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
        public static UserProfile Complete(Guid userId) => new()
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