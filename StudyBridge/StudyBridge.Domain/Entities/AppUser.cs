using StudyBridge.Domain.Common;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Domain.Entities;

public class AppUser : BaseEntity
{
    public string? GoogleSub { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public LoginProvider LoginProvider { get; set; } = LoginProvider.Local;
    
    // Helper method to check if user is OAuth user
    public bool IsOAuthUser => LoginProvider != LoginProvider.Local;
    
    // Helper method to check if user is local user
    public bool IsLocalUser => LoginProvider == LoginProvider.Local && !string.IsNullOrEmpty(PasswordHash);
    
    // Helper method to check if user is Google user
    public bool IsGoogleUser => LoginProvider == LoginProvider.Google && !string.IsNullOrEmpty(GoogleSub);
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    public virtual UserProfile? UserProfile { get; set; }
}
