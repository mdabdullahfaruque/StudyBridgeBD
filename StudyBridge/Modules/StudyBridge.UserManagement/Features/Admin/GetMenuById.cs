using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Enums;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class GetMenuById
{
    public class Query : IQuery<Response>
    {
        public string Id { get; set; } = string.Empty;
        public bool IncludeChildren { get; set; } = true;
        public bool IncludeRoles { get; set; } = false;
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Menu ID is required")
                .Must(BeValidGuid).WithMessage("Menu ID must be a valid GUID");
        }

        private bool BeValidGuid(string id)
        {
            return Guid.TryParse(id, out _);
        }
    }

    public class Response
    {
        public MenuDto Menu { get; set; } = new();
        public string Message { get; set; } = "Menu retrieved successfully";
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
        public DateTime UpdatedAt { get; set; }
        public List<MenuDto> Children { get; set; } = new();
        public List<RoleInfo> AssignedRoles { get; set; } = new();
    }

    public class RoleInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class Handler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(IApplicationDbContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Query request, CancellationToken cancellationToken)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var menuId))
                {
                    throw new ArgumentException("Invalid menu ID format");
                }

                _logger.LogInformation("Getting menu by ID: {MenuId}, IncludeChildren: {IncludeChildren}, IncludeRoles: {IncludeRoles}", 
                    menuId, request.IncludeChildren, request.IncludeRoles);

                var query = _context.Menus.AsQueryable();

                // Include parent menu
                query = query.Include(m => m.ParentMenu);

                // Include children if requested
                if (request.IncludeChildren)
                {
                    query = query.Include(m => m.SubMenus.Where(c => c.IsActive));
                }

                // Include role assignments if requested
                if (request.IncludeRoles)
                {
                    query = query.Include(m => m.RoleMenus)
                                .ThenInclude(rm => rm.Role);
                }

                var menu = await query
                    .FirstOrDefaultAsync(m => m.Id == menuId && m.IsActive, cancellationToken);

                if (menu == null)
                {
                    throw new InvalidOperationException($"Menu with ID {menuId} not found");
                }

                var menuDto = new MenuDto
                {
                    Id = menu.Id.ToString(),
                    Name = menu.Name,
                    DisplayName = menu.DisplayName,
                    Description = menu.Description ?? string.Empty,
                    Icon = menu.Icon,
                    Route = menu.Route,
                    MenuType = menu.MenuType,
                    ParentMenuId = menu.ParentMenuId?.ToString(),
                    ParentMenuName = menu.ParentMenu?.DisplayName,
                    SortOrder = menu.SortOrder,
                    IsActive = menu.IsActive,
                    CreatedAt = menu.CreatedAt,
                    UpdatedAt = menu.UpdatedAt
                };

                // Add children if requested and available
                if (request.IncludeChildren && menu.SubMenus.Any())
                {
                    menuDto.Children = menu.SubMenus
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.SortOrder)
                        .ThenBy(c => c.DisplayName)
                        .Select(child => new MenuDto
                        {
                            Id = child.Id.ToString(),
                            Name = child.Name,
                            DisplayName = child.DisplayName,
                            Description = child.Description ?? string.Empty,
                            Icon = child.Icon,
                            Route = child.Route,
                            MenuType = child.MenuType,
                            ParentMenuId = child.ParentMenuId?.ToString(),
                            ParentMenuName = menu.DisplayName,
                            SortOrder = child.SortOrder,
                            IsActive = child.IsActive,
                            CreatedAt = child.CreatedAt,
                            UpdatedAt = child.UpdatedAt
                        }).ToList();
                }

                // Add role assignments if requested and available
                if (request.IncludeRoles && menu.RoleMenus.Any())
                {
                    menuDto.AssignedRoles = menu.RoleMenus
                        .Where(rm => rm.Role.IsActive)
                        .Select(rm => new RoleInfo
                        {
                            Id = rm.Role.Id.ToString(),
                            Name = rm.Role.Name,
                            DisplayName = rm.Role.Name // Role doesn't have DisplayName, using Name
                        }).ToList();
                }

                _logger.LogInformation("Successfully retrieved menu {MenuName} with ID {MenuId}", 
                    menu.DisplayName, menuId);

                return new Response
                {
                    Menu = menuDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu: {MenuId}", request.Id);
                throw;
            }
        }
    }
}