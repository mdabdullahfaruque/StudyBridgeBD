namespace StudyBridge.UserManagement.Application.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

public record GoogleLoginRequest
{
    public string IdToken { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public UserDto? User { get; init; }
    public DateTime ExpiresAt { get; init; }
}
