using StudyBridge.Domain.Common;

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
    
    // Helper method to check if user is OAuth user
    public bool IsOAuthUser => !string.IsNullOrEmpty(GoogleSub);
    
    // Helper method to check if user is local user
    public bool IsLocalUser => !string.IsNullOrEmpty(PasswordHash);
}
