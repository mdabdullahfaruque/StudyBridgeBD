using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class DeleteUser
{
    public class Command : ICommand<Response>
    {
        public string Id { get; set; } = string.Empty;
        public bool ForceDelete { get; set; } = false;
    }

    public class Validator : AbstractValidator<Command>
    {
        private readonly IApplicationDbContext _context;

        public Validator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("User ID is required")
                .MustAsync(BeValidUserId).WithMessage("User not found")
                .MustAsync(CanBeDeleted).WithMessage("User cannot be deleted - it is a system user or has associated data. Use ForceDelete if necessary.");
        }

        private async Task<bool> BeValidUserId(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var userId))
                return false;

            return await _context.Users
                .AnyAsync(u => u.Id == userId, cancellationToken);
        }

        private async Task<bool> CanBeDeleted(Command command, string id, CancellationToken cancellationToken)
        {
            // If force delete is enabled, skip restrictions
            if (command.ForceDelete)
                return true;

            if (!Guid.TryParse(id, out var userId))
                return false;

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return false;

            // Check if user is a system user (SuperAdmin or Admin)
            var systemRoles = user.UserRoles
                .Where(ur => ur.IsActive)
                .Select(ur => ur.Role.Name.ToLower())
                .ToList();

            if (systemRoles.Contains("superadmin") || systemRoles.Contains("admin"))
                return false;

            // Additional checks can be added here for other restrictions
            // e.g., user has active subscriptions, created content, etc.

            return true;
        }
    }

    public class Response
    {
        public bool IsDeleted { get; set; }
        public string Message { get; set; } = string.Empty;
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
                _logger.LogInformation("Deleting user: {UserId}", request.Id);

                if (!Guid.TryParse(request.Id, out var userId))
                {
                    throw new ArgumentException("Invalid user ID format");
                }

                // Get the existing user with related data
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .Include(u => u.UserSubscriptions)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {request.Id} not found");
                }

                // Check for system user protection if not force delete
                if (!request.ForceDelete)
                {
                    var userRoles = user.UserRoles
                        .Where(ur => ur.IsActive)
                        .Select(ur => ur.Role?.Name?.ToLower())
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    if (userRoles.Contains("superadmin") || userRoles.Contains("admin"))
                    {
                        throw new InvalidOperationException("Cannot delete system users (Admin/SuperAdmin). Use ForceDelete if absolutely necessary.");
                    }
                }

                // Soft delete: Set IsActive to false instead of hard delete
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                // Deactivate user roles
                foreach (var userRole in user.UserRoles.Where(ur => ur.IsActive))
                {
                    userRole.IsActive = false;
                    userRole.UpdatedAt = DateTime.UtcNow;
                }

                // Deactivate user subscriptions
                foreach (var subscription in user.UserSubscriptions.Where(s => s.IsActive))
                {
                    subscription.IsActive = false;
                    subscription.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync(cancellationToken);

                var message = request.ForceDelete ? 
                    $"User {user.Email} has been force deleted successfully" :
                    $"User {user.Email} has been deactivated successfully";

                _logger.LogInformation("Successfully processed delete request for user {Email} with ID {UserId}", 
                    user.Email, user.Id);

                return new Response
                {
                    IsDeleted = true,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", request.Id);
                throw;
            }
        }
    }
}