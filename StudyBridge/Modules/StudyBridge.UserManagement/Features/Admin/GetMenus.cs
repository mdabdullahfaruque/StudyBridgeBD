using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Shared.CQRS;
using System.Security.Claims;

namespace StudyBridge.UserManagement.Features.Admin;

public static class GetMenus
{
    public class Query : IQuery<Response>
    {
        public MenuType? MenuType { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public bool ForUser { get; set; } = false;
        public List<string>? UserRoles { get; set; }
    }

    public class Response
    {
        public List<MenuDto> Menus { get; set; } = new();
        public int TotalCount { get; set; }
        public string Message { get; set; } = "Menus retrieved successfully";
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
        public List<string> RequiredPermissions { get; set; } = new();
    }

    public class Handler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(IApplicationDbContext context, IRoleRepository roleRepository, ILogger<Handler> logger)
        {
            _context = context;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting menus with filters - MenuType: {MenuType}, IncludeInactive: {IncludeInactive}, ForUser: {ForUser}", 
                    request.MenuType, request.IncludeInactive, request.ForUser);

                IQueryable<Menu> query = _context.Menus.AsQueryable();

                // Apply filters
                if (!request.IncludeInactive)
                {
                    query = query.Where(m => m.IsActive);
                }

                if (request.MenuType.HasValue)
                {
                    query = query.Where(m => m.MenuType == request.MenuType.Value);
                }

                // If this is for a user, filter by their roles
                if (request.ForUser && request.UserRoles != null && request.UserRoles.Any())
                {
                    // Get role IDs from role names
                    var roleIds = new List<Guid>();
                    foreach (var roleName in request.UserRoles)
                    {
                        var role = await _roleRepository.GetByNameAsync(roleName);
                        if (role != null)
                        {
                            roleIds.Add(role.Id);
                        }
                    }

                    if (roleIds.Any())
                    {
                        // Get menus that are accessible by these roles
                        var accessibleMenuIds = await _context.RoleMenus
                            .Where(rm => roleIds.Contains(rm.RoleId))
                            .Select(rm => rm.MenuId)
                            .Distinct()
                            .ToListAsync(cancellationToken);

                        query = query.Where(m => accessibleMenuIds.Contains(m.Id));
                    }
                    else
                    {
                        // No valid roles found, return empty list
                        query = query.Where(m => false);
                    }
                }

                // Include parent menu for reference
                var menus = await query
                    .Include(m => m.ParentMenu)
                    .Include(m => m.SubMenus.Where(c => request.IncludeInactive || c.IsActive))
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.DisplayName)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} menus", menus.Count);

                // Build hierarchical structure (only top-level items for the main list)
                var topLevelMenus = menus.Where(m => m.ParentMenuId == null).ToList();
                var menuDtos = topLevelMenus.Select(menu => MapToMenuDto(menu, menus)).ToList();

                return new Response
                {
                    Menus = menuDtos,
                    TotalCount = menus.Count,
                    Message = request.ForUser ? "User menus retrieved successfully" : "Menus retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menus");
                throw;
            }
        }

        private static MenuDto MapToMenuDto(Menu menu, List<Menu> allMenus)
        {
            return new MenuDto
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
                Children = allMenus
                    .Where(m => m.ParentMenuId == menu.Id)
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.DisplayName)
                    .Select(child => MapToMenuDto(child, allMenus))
                    .ToList(),
                RequiredPermissions = new List<string>() // Can be extended later if needed
            };
        }
    }
}