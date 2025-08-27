using StudyBridge.Shared.Common;

namespace StudyBridge.UserManagement.Domain.Entities;

public record AppUser : BaseEntity
{
    public string? GoogleSub { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? PasswordHash { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailConfirmed { get; init; } = false;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; init; } = true;
    
    // Helper method to check if user is OAuth user
    public bool IsOAuthUser => !string.IsNullOrEmpty(GoogleSub);
    
    // Helper method to check if user is local user
    public bool IsLocalUser => !string.IsNullOrEmpty(PasswordHash);
}
