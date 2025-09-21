using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class DeleteRole
{
    public class Command : ICommand<Response>
    {
        public string Id { get; set; } = string.Empty;
        public bool ForceDelete { get; set; } = false; // For system roles or roles with users
    }

    public class Validator : AbstractValidator<Command>
    {
        private readonly IApplicationDbContext _context;

        public Validator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Role ID is required")
                .MustAsync(BeValidRoleId).WithMessage("Role not found")
                .MustAsync(CanBeDeleted).WithMessage("Role cannot be deleted - it has associated users or is a system role. Use ForceDelete if necessary.");
        }

        private async Task<bool> BeValidRoleId(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var roleId))
                return false;

            return await _context.Roles
                .AnyAsync(r => r.Id == roleId, cancellationToken);
        }

        private async Task<bool> CanBeDeleted(Command command, string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var roleId))
                return false;

            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
                return false;

            // If force delete is enabled, allow deletion
            if (command.ForceDelete)
                return true;

            // Check if role has users
            if (role.UserRoles.Any())
                return false;

            // Allow deletion if no users are associated

            return true;
        }
    }

    public class Response
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Message { get; set; } = "Role deleted successfully";
        public bool WasForceDeleted { get; set; } = false;
        public int AffectedUsersCount { get; set; } = 0;
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
                _logger.LogInformation("Deleting role: {RoleId}, ForceDelete: {ForceDelete}", 
                    request.Id, request.ForceDelete);

                if (!Guid.TryParse(request.Id, out var roleId))
                {
                    throw new ArgumentException("Invalid role ID format");
                }

                // Get the role with all related data
                var role = await _context.Roles
                    .Include(r => r.UserRoles)
                    .Include(r => r.RoleMenus)
                    .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

                if (role == null)
                {
                    throw new InvalidOperationException($"Role with ID {request.Id} not found");
                }

                var affectedUsersCount = role.UserRoles.Count;
                var roleName = role.Name;

                // Handle force delete scenario
                if (request.ForceDelete || affectedUsersCount == 0)
                {
                    // Remove all user-role associations first
                    if (role.UserRoles.Any())
                    {
                        _logger.LogWarning("Force deleting role {RoleName} which has {UserCount} associated users", 
                            roleName, affectedUsersCount);
                        
                        foreach (var userRole in role.UserRoles.ToList())
                        {
                            _context.UserRoles.Remove(userRole);
                        }
                    }

                    // Remove all role-menu associations
                    if (role.RoleMenus.Any())
                    {
                        foreach (var roleMenu in role.RoleMenus.ToList())
                        {
                            _context.RoleMenus.Remove(roleMenu);
                        }
                    }

                    // Remove the role itself
                    _context.Roles.Remove(role);

                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Successfully deleted role {RoleName} with ID {RoleId}", 
                        roleName, roleId);

                    return new Response
                    {
                        RoleId = request.Id,
                        RoleName = roleName,
                        Message = request.ForceDelete 
                            ? $"Role '{roleName}' was force deleted along with {affectedUsersCount} user associations" 
                            : $"Role '{roleName}' deleted successfully",
                        WasForceDeleted = request.ForceDelete,
                        AffectedUsersCount = affectedUsersCount
                    };
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot delete role '{roleName}' because it has {affectedUsersCount} associated users. " +
                        "Use ForceDelete=true to override, or remove users from this role first.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", request.Id);
                throw;
            }
        }
    }
}