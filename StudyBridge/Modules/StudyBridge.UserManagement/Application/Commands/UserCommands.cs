using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Application.DTOs;

namespace StudyBridge.UserManagement.Application.Commands;

public record GoogleLoginCommand(string IdToken) : ICommand<LoginResponse>;

public record UpdateUserCommand(Guid UserId, string DisplayName, string? AvatarUrl) : ICommand<UserDto>;
