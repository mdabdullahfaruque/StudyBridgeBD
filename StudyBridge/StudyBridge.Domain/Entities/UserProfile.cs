using StudyBridge.Domain.Common;

namespace StudyBridge.Domain.Entities;

public class UserProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? PreferredLanguage { get; set; } = "en";
    public string? TimeZone { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
}
