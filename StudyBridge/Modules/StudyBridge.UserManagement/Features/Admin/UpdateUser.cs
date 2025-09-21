using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class UpdateUser
{
    public class Command : ICommand<Response>
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; }
        public List<string> RoleIds { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        private readonly IApplicationDbContext _context;

        public Validator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User ID is required")
                .MustAsync(BeValidUserId).WithMessage("User not found");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(256).WithMessage("Email cannot exceed 256 characters")
                .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

            RuleFor(x => x.DisplayName)
                .MaximumLength(150).WithMessage("Display name cannot exceed 150 characters");

            RuleForEach(x => x.RoleIds)
                .MustAsync(BeValidRoleId).WithMessage("Invalid role ID");
        }

        private async Task<bool> BeValidUserId(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var userId))
                return false;

            return await _context.Users
                .AnyAsync(u => u.Id == userId, cancellationToken);
        }

        private async Task<bool> BeUniqueEmail(Command command, string email, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(command.Id, out var userId))
                return false;

            return !await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.Id != userId, cancellationToken);
        }

        private async Task<bool> BeValidRoleId(string roleId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(roleId, out var id))
                return false;

            return await _context.Roles
                .AnyAsync(r => r.Id == id && r.IsActive, cancellationToken);
        }
    }

    public class Response
    {
        public UserDto User { get; set; } = new();
        public string Message { get; set; } = "User updated successfully";
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<RoleDto> Roles { get; set; } = new();
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(IApplicationDbContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating user: {UserId}", request.Id);

                if (!Guid.TryParse(request.Id, out var userId))
                {
                    throw new ArgumentException("Invalid user ID format");
                }

                // Get the existing user
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {request.Id} not found");
                }

                // Update user properties
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.DisplayName = request.DisplayName ?? $"{request.FirstName} {request.LastName}";
                user.IsActive = request.IsActive;
                user.EmailConfirmed = request.EmailConfirmed;
                user.UpdatedAt = DateTime.UtcNow;

                // Update roles - remove existing ones and add new ones
                var existingUserRoles = user.UserRoles.ToList();
                foreach (var existingRole in existingUserRoles)
                {
                    _context.UserRoles.Remove(existingRole);
                }

                // Add new roles
                if (request.RoleIds.Any())
                {
                    var validRoleIds = request.RoleIds
                        .Where(id => Guid.TryParse(id, out _))
                        .Select(id => Guid.Parse(id))
                        .ToList();

                    var existingRoles = await _context.Roles
                        .Where(r => validRoleIds.Contains(r.Id) && r.IsActive)
                        .ToListAsync(cancellationToken);

                    foreach (var role in existingRoles)
                    {
                        var userRole = new UserRole
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            RoleId = role.Id,
                            IsActive = true,
                            AssignedAt = DateTime.UtcNow,
                            AssignedBy = "System" // TODO: Get current user ID from context
                        };

                        _context.UserRoles.Add(userRole);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Load the updated user with roles for response
                var updatedUser = await _context.Users
                    .Include(u => u.UserRoles.Where(ur => ur.IsActive))
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

                if (updatedUser == null)
                {
                    throw new InvalidOperationException("Failed to retrieve updated user");
                }

                var response = new Response
                {
                    User = new UserDto
                    {
                        Id = updatedUser.Id.ToString(),
                        FirstName = updatedUser.FirstName,
                        LastName = updatedUser.LastName,
                        Email = updatedUser.Email,
                        DisplayName = updatedUser.DisplayName,
                        IsActive = updatedUser.IsActive,
                        EmailConfirmed = updatedUser.EmailConfirmed,
                        CreatedAt = updatedUser.CreatedAt,
                        UpdatedAt = updatedUser.UpdatedAt,
                        Roles = updatedUser.UserRoles
                            .Where(ur => ur.IsActive && ur.Role.IsActive)
                            .Select(ur => new RoleDto
                            {
                                Id = ur.Role.Id.ToString(),
                                Name = ur.Role.Name,
                                Description = ur.Role.Description
                            }).ToList()
                    }
                };

                _logger.LogInformation("Successfully updated user {Email} with ID {UserId}", 
                    updatedUser.Email, updatedUser.Id);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", request.Id);
                throw;
            }
        }
    }
}