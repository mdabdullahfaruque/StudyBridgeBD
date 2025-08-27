using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Application.DTOs;

namespace StudyBridge.UserManagement.Application.Queries;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto?>;

public record GetUserByEmailQuery(string Email) : IQuery<UserDto?>;
