using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class UpdateMenu
{
    public class Command : ICommand<Response>
    {
        public string MenuId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public MenuType MenuType { get; set; } = MenuType.Admin;
        public string? ParentMenuId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
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

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Menu name is required")
                .MinimumLength(2).WithMessage("Menu name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Menu name cannot exceed 100 characters")
                .MustAsync(BeUniqueMenuNameForUpdate).WithMessage("Menu name already exists");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MinimumLength(2).WithMessage("Display name must be at least 2 characters")
                .MaximumLength(150).WithMessage("Display name cannot exceed 150 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Icon)
                .MaximumLength(100).WithMessage("Icon cannot exceed 100 characters");

            RuleFor(x => x.Route)
                .MaximumLength(255).WithMessage("Route cannot exceed 255 characters");

            RuleFor(x => x.ParentMenuId)
                .MustAsync(BeValidParentMenuId).WithMessage("Invalid parent menu ID")
                .MustAsync(NotCreateCircularReference).WithMessage("Parent menu would create circular reference")
                .When(x => !string.IsNullOrEmpty(x.ParentMenuId));

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be non-negative");
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

        private async Task<bool> BeUniqueMenuNameForUpdate(Command command, string name, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(command.MenuId, out var menuId))
                return false;

            return !await _context.Menus
                .AnyAsync(m => m.Name.ToLower() == name.ToLower() && m.Id != menuId, cancellationToken);
        }

        private async Task<bool> BeValidParentMenuId(string? parentMenuId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(parentMenuId))
                return true;

            if (!Guid.TryParse(parentMenuId, out var id))
                return false;

            return await _context.Menus
                .AnyAsync(m => m.Id == id && m.IsActive, cancellationToken);
        }

        private async Task<bool> NotCreateCircularReference(Command command, string? parentMenuId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(parentMenuId) || !Guid.TryParse(command.MenuId, out var menuId))
                return true;

            if (!Guid.TryParse(parentMenuId, out var parentId))
                return true;

            // Cannot set parent to itself
            if (menuId == parentId)
                return false;

            // Check if the potential parent is a descendant of the current menu
            var descendants = await GetAllDescendants(menuId, cancellationToken);
            return !descendants.Contains(parentId);
        }

        private async Task<List<Guid>> GetAllDescendants(Guid menuId, CancellationToken cancellationToken)
        {
            var descendants = new List<Guid>();
            var directChildren = await _context.Menus
                .Where(m => m.ParentMenuId == menuId && m.IsActive)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);

            descendants.AddRange(directChildren);

            foreach (var childId in directChildren)
            {
                var childDescendants = await GetAllDescendants(childId, cancellationToken);
                descendants.AddRange(childDescendants);
            }

            return descendants;
        }
    }

    public class Response
    {
        public CreateMenu.MenuDto Menu { get; set; } = new();
        public string Message { get; set; } = "Menu updated successfully";
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

                _logger.LogInformation("Updating menu: {MenuId} with name: {MenuName}", menuId, request.Name);

                var menu = await _context.Menus
                    .FirstOrDefaultAsync(m => m.Id == menuId && m.IsActive, cancellationToken);

                if (menu == null)
                {
                    throw new InvalidOperationException($"Menu with ID {menuId} not found");
                }

                // Parse parent menu ID if provided
                Guid? parentMenuId = null;
                if (!string.IsNullOrEmpty(request.ParentMenuId) && Guid.TryParse(request.ParentMenuId, out var parsedId))
                {
                    parentMenuId = parsedId;
                }

                // Update menu properties
                menu.Name = request.Name;
                menu.DisplayName = request.DisplayName;
                menu.Description = request.Description;
                menu.Icon = request.Icon;
                menu.Route = request.Route;
                menu.MenuType = request.MenuType;
                menu.ParentMenuId = parentMenuId;
                menu.SortOrder = request.SortOrder;
                menu.IsActive = request.IsActive;
                menu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                // Load the updated menu with relationships for response
                var updatedMenu = await _context.Menus
                    .Include(m => m.ParentMenu)
                    .Include(m => m.SubMenus.Where(c => c.IsActive))
                    .FirstOrDefaultAsync(m => m.Id == menuId, cancellationToken);

                if (updatedMenu == null)
                {
                    throw new InvalidOperationException("Failed to retrieve updated menu");
                }

                var response = new Response
                {
                    Menu = new CreateMenu.MenuDto
                    {
                        Id = updatedMenu.Id.ToString(),
                        Name = updatedMenu.Name,
                        DisplayName = updatedMenu.DisplayName,
                        Description = updatedMenu.Description ?? string.Empty,
                        Icon = updatedMenu.Icon,
                        Route = updatedMenu.Route,
                        MenuType = updatedMenu.MenuType,
                        ParentMenuId = updatedMenu.ParentMenuId?.ToString(),
                        ParentMenuName = updatedMenu.ParentMenu?.DisplayName,
                        SortOrder = updatedMenu.SortOrder,
                        IsActive = updatedMenu.IsActive,
                        CreatedAt = updatedMenu.CreatedAt,
                        Children = updatedMenu.SubMenus
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.SortOrder)
                            .Select(c => new CreateMenu.MenuDto
                            {
                                Id = c.Id.ToString(),
                                Name = c.Name,
                                DisplayName = c.DisplayName,
                                Description = c.Description ?? string.Empty,
                                Icon = c.Icon,
                                Route = c.Route,
                                MenuType = c.MenuType,
                                ParentMenuId = c.ParentMenuId?.ToString(),
                                SortOrder = c.SortOrder,
                                IsActive = c.IsActive,
                                CreatedAt = c.CreatedAt
                            }).ToList()
                    }
                };

                _logger.LogInformation("Successfully updated menu {MenuName} with ID {MenuId}", 
                    updatedMenu.Name, updatedMenu.Id);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu: {MenuId}", request.MenuId);
                throw;
            }
        }
    }
}