using System.ComponentModel.DataAnnotations;

namespace StudyBridge.UserManagement.Application.DTOs;

public record RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;
    
    [Required]
    [MinLength(2)]
    public string FirstName { get; init; } = string.Empty;
    
    [Required]
    [MinLength(2)]
    public string LastName { get; init; } = string.Empty;
    
    [Required]
    public string DisplayName { get; init; } = string.Empty;
}

public record LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    public string Password { get; init; } = string.Empty;
}

public record ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; init; } = string.Empty;
}

public record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; init; } = string.Empty;
}
