using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class CreateMenu
{
    public class Command : ICommand<Response>
    {
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

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Menu name is required")
                .MinimumLength(2).WithMessage("Menu name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Menu name cannot exceed 100 characters")
                .MustAsync(BeUniqueMenuName).WithMessage("Menu name already exists");

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
                .When(x => !string.IsNullOrEmpty(x.ParentMenuId));

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be non-negative");
        }

        private async Task<bool> BeUniqueMenuName(string name, CancellationToken cancellationToken)
        {
            return !await _context.Menus
                .AnyAsync(m => m.Name.ToLower() == name.ToLower(), cancellationToken);
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
    }

    public class Response
    {
        public MenuDto Menu { get; set; } = new();
        public string Message { get; set; } = "Menu created successfully";
    }

    public class MenuDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public MenuType MenuType { get; set; }
        public string? ParentMenuId { get; set; }
        public string? ParentMenuName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MenuDto> Children { get; set; } = new();
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
                _logger.LogInformation("Creating new menu: {MenuName}", request.Name);

                // Parse parent menu ID if provided
                Guid? parentMenuId = null;
                if (!string.IsNullOrEmpty(request.ParentMenuId) && Guid.TryParse(request.ParentMenuId, out var parsedId))
                {
                    parentMenuId = parsedId;
                }

                // Create the menu entity
                var menu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    DisplayName = request.DisplayName,
                    Description = request.Description,
                    Icon = request.Icon,
                    Route = request.Route,
                    MenuType = request.MenuType,
                    ParentMenuId = parentMenuId,
                    SortOrder = request.SortOrder,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Menus.Add(menu);
                await _context.SaveChangesAsync(cancellationToken);

                // Load the created menu with parent for response
                var createdMenu = await _context.Menus
                    .Include(m => m.ParentMenu)
                    .Include(m => m.SubMenus.Where(c => c.IsActive))
                    .FirstOrDefaultAsync(m => m.Id == menu.Id, cancellationToken);

                if (createdMenu == null)
                {
                    throw new InvalidOperationException("Failed to retrieve created menu");
                }

                var response = new Response
                {
                    Menu = new MenuDto
                    {
                        Id = createdMenu.Id.ToString(),
                        Name = createdMenu.Name,
                        DisplayName = createdMenu.DisplayName,
                        Description = createdMenu.Description ?? string.Empty,
                        Icon = createdMenu.Icon,
                        Route = createdMenu.Route,
                        MenuType = createdMenu.MenuType,
                        ParentMenuId = createdMenu.ParentMenuId?.ToString(),
                        ParentMenuName = createdMenu.ParentMenu?.DisplayName,
                        SortOrder = createdMenu.SortOrder,
                        IsActive = createdMenu.IsActive,
                        CreatedAt = createdMenu.CreatedAt,
                        Children = createdMenu.SubMenus
                            .Where(c => c.IsActive)
                            .Select(c => new MenuDto
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

                _logger.LogInformation("Successfully created menu {MenuName} with ID {MenuId}", 
                    createdMenu.Name, createdMenu.Id);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu: {MenuName}", request.Name);
                throw;
            }
        }
    }
}