using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Application.DTOs;

namespace StudyBridge.UserManagement.Application.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string DisplayName
) : ICommand<LoginResponse>;

public record LoginCommand(
    string Email,
    string Password
) : ICommand<LoginResponse>;

public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword
) : ICommand<bool>;

public record ForgotPasswordCommand(
    string Email
) : ICommand<bool>;

public record ResetPasswordCommand(
    string Token,
    string Email,
    string NewPassword
) : ICommand<bool>;
