using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class DeleteMenu
{
    public class Command : ICommand<Response>
    {
        public string MenuId { get; set; } = string.Empty;
        public bool ForceDelete { get; set; } = false;
    }

    public class Validator : AbstractValidator<Command>
    {
        private readonly IApplicationDbContext _context;

        public Validator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(x => x.MenuId)
                .NotEmpty().WithMessage("Menu ID is required")
                .Must(BeValidGuid).WithMessage("Menu ID must be a valid GUID")
                .MustAsync(MenuExists).WithMessage("Menu does not exist");

            RuleFor(x => x)
                .MustAsync(CanBeDeleted).WithMessage("Menu cannot be deleted as it has active child menus or is assigned to roles")
                .When(x => !x.ForceDelete);
        }

        private bool BeValidGuid(string menuId)
        {
            return Guid.TryParse(menuId, out _);
        }

        private async Task<bool> MenuExists(string menuId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(menuId, out var id))
                return false;

            return await _context.Menus
                .AnyAsync(m => m.Id == id && m.IsActive, cancellationToken);
        }

        private async Task<bool> CanBeDeleted(Command command, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(command.MenuId, out var menuId))
                return false;

            // Check if menu has active sub menus
            var hasActiveChildren = await _context.Menus
                .AnyAsync(m => m.ParentMenuId == menuId && m.IsActive, cancellationToken);

            if (hasActiveChildren)
                return false;

            // Check if menu is assigned to any roles
            var isAssignedToRoles = await _context.RoleMenus
                .AnyAsync(rm => rm.MenuId == menuId, cancellationToken);

            return !isAssignedToRoles;
        }
    }

    public class Response
    {
        public string MenuId { get; set; } = string.Empty;
        public string MenuName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public string Message { get; set; } = "Menu deleted successfully";
        public List<string> WarningMessages { get; set; } = new();
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
                if (!Guid.TryParse(request.MenuId, out var menuId))
                {
                    throw new ArgumentException("Invalid menu ID format");
                }

                _logger.LogInformation("Deleting menu: {MenuId}, ForceDelete: {ForceDelete}", menuId, request.ForceDelete);

                var menu = await _context.Menus
                    .Include(m => m.SubMenus.Where(c => c.IsActive))
                    .Include(m => m.RoleMenus)
                    .FirstOrDefaultAsync(m => m.Id == menuId && m.IsActive, cancellationToken);

                if (menu == null)
                {
                    throw new InvalidOperationException($"Menu with ID {menuId} not found");
                }

                var response = new Response
                {
                    MenuId = menu.Id.ToString(),
                    MenuName = menu.DisplayName
                };

                var warningMessages = new List<string>();

                // Handle force delete scenario
                if (request.ForceDelete)
                {
                    // First, handle child menus
                    if (menu.SubMenus.Any(c => c.IsActive))
                    {
                        var activeChildrenCount = menu.SubMenus.Count(c => c.IsActive);
                        
                        // Soft delete all active child menus
                        foreach (var childMenu in menu.SubMenus.Where(c => c.IsActive))
                        {
                            childMenu.IsActive = false;
                            childMenu.UpdatedAt = DateTime.UtcNow;
                        }

                        warningMessages.Add($"Deleted {activeChildrenCount} child menu(s)");
                        _logger.LogWarning("Force deleting {Count} child menus for parent menu {MenuId}", 
                            activeChildrenCount, menuId);
                    }

                    // Handle role assignments
                    if (menu.RoleMenus.Any())
                    {
                        var roleAssignmentCount = menu.RoleMenus.Count;
                        
                        // Remove role-menu assignments
                        _context.RoleMenus.RemoveRange(menu.RoleMenus);
                        
                        warningMessages.Add($"Removed menu from {roleAssignmentCount} role assignment(s)");
                        _logger.LogWarning("Force removing {Count} role assignments for menu {MenuId}", 
                            roleAssignmentCount, menuId);
                    }
                }

                // Soft delete the menu
                menu.IsActive = false;
                menu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                response.IsDeleted = true;
                response.WarningMessages = warningMessages;

                if (warningMessages.Any())
                {
                    response.Message = $"Menu deleted successfully with warnings: {string.Join(", ", warningMessages)}";
                }

                _logger.LogInformation("Successfully deleted menu {MenuName} with ID {MenuId}. Warnings: {Warnings}", 
                    menu.DisplayName, menuId, string.Join(", ", warningMessages));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu: {MenuId}", request.MenuId);
                throw;
            }
        }
    }
}