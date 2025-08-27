using StudyBridge.UserManagement.Domain.Entities;

namespace StudyBridge.UserManagement.Application.Services;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo> ValidateTokenAsync(string idToken);
}

public interface IJwtService
{
    string GenerateToken(AppUser user);
    Guid? ValidateToken(string token);
}

public record GoogleUserInfo
{
    public string Sub { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Picture { get; init; } = string.Empty;
}
