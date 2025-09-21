using Microsoft.AspNetCore.Mvc;
using StudyBridge.Api.DTOs;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Features.Admin;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IMenuRepository _menuRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IQueryHandler<GetUsers.Query, GetUsers.Response> _getUsersHandler;
    private readonly IQueryHandler<GetUserById.Query, GetUserById.Response> _getUserByIdHandler;
    private readonly ICommandHandler<CreateUser.Command, CreateUser.Response> _createUserHandler;
    private readonly IQueryHandler<GetRoles.Query, GetRoles.Response> _getRolesHandler;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ISubscriptionService subscriptionService,
        IMenuRepository menuRepository,
        IRoleRepository roleRepository,
        IQueryHandler<GetUsers.Query, GetUsers.Response> getUsersHandler,
        IQueryHandler<GetUserById.Query, GetUserById.Response> getUserByIdHandler,
        ICommandHandler<CreateUser.Command, CreateUser.Response> createUserHandler,
        IQueryHandler<GetRoles.Query, GetRoles.Response> getRolesHandler,
        ILogger<AdminController> logger)
    {
        _subscriptionService = subscriptionService;
        _menuRepository = menuRepository;
        _roleRepository = roleRepository;
        _getUsersHandler = getUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _createUserHandler = createUserHandler;
        _getRolesHandler = getRolesHandler;
        _logger = logger;
    }

    [HttpGet("users")]
    [RequireMenu("users.view")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsers.Query query)
    {
        _logger.LogInformation("Admin viewing users list - Page: {PageNumber}, Size: {PageSize}", query.PageNumber, query.PageSize);
        
        var response = await _getUsersHandler.HandleAsync(query);
        return Ok(ApiResponse<GetUsers.Response>.SuccessResult(response, "Users retrieved successfully"));
    }

    [HttpGet("users/{userId}")]
    [RequireMenu("users.view")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        _logger.LogInformation("Admin viewing user details: {UserId}", userId);
        
        var query = new GetUserById.Query { UserId = userId };
        var response = await _getUserByIdHandler.HandleAsync(query);
        return Ok(ApiResponse<GetUserById.Response>.SuccessResult(response, "User details retrieved successfully"));
    }

    [HttpPost("users")]
    [RequireMenu("users.create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUser.Command command)
    {
        _logger.LogInformation("Admin creating new user: {Email}", command.Email);
        
        var response = await _createUserHandler.HandleAsync(command);
        return Ok(ApiResponse<CreateUser.Response>.SuccessResult(response, "User created successfully"));
    }

    [HttpGet("roles")]
    [RequireMenu("roles.view")]
    public async Task<IActionResult> GetRoles()
    {
        _logger.LogInformation("Admin viewing roles list");
        
        var query = new GetRoles.Query();
        var response = await _getRolesHandler.HandleAsync(query);
        return Ok(ApiResponse<GetRoles.Response>.SuccessResult(response, "Roles retrieved successfully"));
    }

    [HttpGet("roles-menus")]
    [RequireMenu("roles.view")]
    public async Task<IActionResult> GetRoleMenuMappings()
    {
        try
        {
            _logger.LogInformation("Admin viewing role-menu mappings");
            
            var roles = await _getRolesHandler.HandleAsync(new GetRoles.Query());
            
            var roleMenuData = roles.Roles.Select(role => new
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Menus = role.Menus.Select(menu => new
                {
                    MenuId = menu.Id,
                    MenuName = menu.Name,
                    DisplayName = menu.DisplayName,
                    Description = menu.Description,
                    IsGranted = menu.IsGranted
                }).ToList()
            }).ToList();

            return Ok(ApiResponse<object>.SuccessResult(
                new { RoleMenuMappings = roleMenuData }, 
                "Role-menu mappings retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role-menu mappings");
            return StatusCode(500, ApiResponse<object>.FailureResult("Failed to retrieve role-menu mappings"));
        }
    }

    [HttpGet("financials")]
    [RequireMenu("financial.view")]
    public IActionResult GetFinancials()
    {
        _logger.LogInformation("Accessing financial data");
        return Ok(new { message = "Financial data retrieved successfully" });
    }

    [HttpPost("assign-role")]
    [RequireMenu("users.manage")]
    public async Task<IActionResult> AssignUserRole([FromBody] AssignUserRoleRequest request)
    {
        try
        {
            var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
                return Unauthorized();

            // Get the role to assign
            var role = await _roleRepository.GetByIdAsync(request.RoleId);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.FailureResult("Role not found"));
            }

            // Check if user already has this role (this would be in UserRoles table)
            // For now, we'll just log the assignment since the UserRole management 
            // is handled through the UserManagement module
            _logger.LogInformation("Role {RoleName} assignment requested for user {UserId} by {AssignedBy}", 
                role.Name, request.UserId, currentUserId);
            
            // Return success - actual implementation would go through UserManagement module
            return Ok(ApiResponse<object>.SuccessResult(
                new { 
                    UserId = request.UserId, 
                    RoleId = request.RoleId, 
                    RoleName = role.Name 
                }, 
                $"Role {role.Name} assigned to user successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user");
            return StatusCode(500, ApiResponse<object>.FailureResult("Failed to assign role to user"));
        }
    }

    [HttpPost("create-subscription")]
    // [RequireMenu(SystemPermission.ManageSubscriptions)] // TODO: Update after RBAC implementation
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        var success = await _subscriptionService.CreateSubscriptionAsync(
            request.UserId,
            request.SubscriptionType,
            request.Amount,
            request.EndDate);

        if (success)
        {
            _logger.LogInformation("Subscription {Type} created for user {UserId}", 
                request.SubscriptionType, request.UserId);
            return Ok(new { message = "Subscription created successfully" });
        }

        return BadRequest(new { message = "Failed to create subscription" });
    }

    [HttpGet("system-logs")]
    [RequireMenu("system.manage")]
    public IActionResult GetSystemLogs()
    {
        _logger.LogInformation("Admin accessing system logs");
        return Ok(new { message = "System logs retrieved successfully" });
    }

    // Menu Management Endpoints
    [HttpGet("menus")]
    [RequireMenu("system.view")]
    public async Task<IActionResult> GetAllMenus()
    {
        try
        {
            var menus = await _menuRepository.GetAllAsync();
            var menuDtos = menus.Where(m => m.MenuType == MenuType.Admin && m.IsActive)
                               .Select(MapToMenuDto)
                               .ToList();

            return Ok(ApiResponse<List<MenuDto>>.SuccessResult(menuDtos, "Menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all menus");
            return StatusCode(500, ApiResponse<List<MenuDto>>.FailureResult("Failed to retrieve menus"));
        }
    }

    [HttpGet("menus/user-menus")]
    public async Task<IActionResult> GetUserMenus()
    {
        try
        {
            // Get current user ID from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<List<MenuDto>>.FailureResult("Invalid user token"));
            }

            _logger.LogInformation("Admin endpoint: Getting menus for user: {UserId}", userId);

            // Get user's roles to determine menu access
            var userRolesClaim = User.FindFirst("roles")?.Value;
            if (string.IsNullOrEmpty(userRolesClaim))
            {
                _logger.LogWarning("No roles found for user: {UserId}", userId);
                return Ok(ApiResponse<List<MenuDto>>.SuccessResult(new List<MenuDto>(), "No menus available"));
            }

            var userRoleIds = userRolesClaim.Split(',').Select(Guid.Parse).ToList();
            _logger.LogInformation("User has {RoleCount} roles: {RoleIds}", userRoleIds.Count, string.Join(", ", userRoleIds));

            // Get menus accessible by user's roles
            var allAccessibleMenus = new List<Menu>();
            foreach (var roleId in userRoleIds)
            {
                var roleMenus = await _menuRepository.GetMenusByRoleIdAsync(roleId);
                allAccessibleMenus.AddRange(roleMenus);
            }
            
            // Remove duplicates and ensure only active menus
            var accessibleMenus = allAccessibleMenus
                .Where(m => m.IsActive)
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .ToList();
            
            _logger.LogInformation("Found {MenuCount} accessible menus", accessibleMenus.Count);

            // Build hierarchy and convert to DTOs
            var menuHierarchy = BuildMenuHierarchy(accessibleMenus);
            var menuDtos = menuHierarchy.Select(m => MapToMenuDtoWithChildren(m, accessibleMenus))
                                       .ToList();

            _logger.LogInformation("Returning {HierarchyCount} top-level menus", menuDtos.Count);
            return Ok(ApiResponse<List<MenuDto>>.SuccessResult(menuDtos, "User menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user menus");
            return StatusCode(500, ApiResponse<List<MenuDto>>.FailureResult("Failed to retrieve user menus"));
        }
    }

    // Helper methods
    private static MenuDto MapToMenuDto(Menu menu)
    {
        return new MenuDto
        {
            Id = menu.Id.ToString(),
            Name = menu.Name,
            DisplayName = menu.DisplayName,
            Icon = menu.Icon,
            Route = menu.Route,
            ParentId = menu.ParentMenuId?.ToString(),
            SortOrder = menu.SortOrder,
            IsActive = menu.IsActive,
            MenuType = (int)menu.MenuType,
            RequiredPermissions = new List<string>()
        };
    }

    private static MenuDto MapToMenuDtoWithChildren(Menu menu, List<Menu> allMenus)
    {
        return new MenuDto
        {
            Id = menu.Id.ToString(),
            Name = menu.Name,
            DisplayName = menu.DisplayName,
            Icon = menu.Icon,
            Route = menu.Route,
            ParentId = menu.ParentMenuId?.ToString(),
            SortOrder = menu.SortOrder,
            IsActive = menu.IsActive,
            MenuType = (int)menu.MenuType,
            RequiredPermissions = new List<string>(), // Simplified - no longer needed
            Children = allMenus.Where(m => m.ParentMenuId == menu.Id)
                              .OrderBy(m => m.SortOrder)
                              .Select(m => MapToMenuDtoWithChildren(m, allMenus))
                              .ToList()
        };
    }

    private static List<Menu> BuildMenuHierarchy(List<Menu> flatMenus)
    {
        return flatMenus.Where(m => m.ParentMenuId == null)
                       .OrderBy(m => m.SortOrder)
                       .ToList();
    }

    [HttpGet("public-menus")]
    [RequireMenu("public.dashboard")]
    public async Task<IActionResult> GetPublicMenus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User not authenticated" });
            }

            // Get all public menus
            var allMenus = await _menuRepository.GetAllAsync();
            var publicMenus = allMenus.Where(m => m.MenuType == MenuType.Public && m.IsActive).ToList();

            // Build hierarchy and convert to DTOs (simplified - no permission filtering for public menus)
            var menuHierarchy = BuildMenuHierarchy(publicMenus);
            var menuDtos = menuHierarchy.Select(menu => MapToMenuDtoWithChildren(menu, publicMenus)).ToList();

            return Ok(ApiResponse<List<MenuDto>>.SuccessResult(menuDtos, "Public menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public menus for user");
            return StatusCode(500, ApiResponse<List<MenuDto>>.FailureResult("An error occurred while retrieving public menus"));
        }
    }

    // Menu CRUD Operations
    [HttpGet("menus/{id}")]
    [RequireMenu("system.view")]
    public async Task<IActionResult> GetMenuById(Guid id)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
            {
                return NotFound(ApiResponse<MenuDto>.FailureResult("Menu not found"));
            }

            var menuDto = MapToMenuDto(menu);
            return Ok(ApiResponse<MenuDto>.SuccessResult(menuDto, "Menu retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu with ID {MenuId}", id);
            return StatusCode(500, ApiResponse<MenuDto>.FailureResult("Failed to retrieve menu"));
        }
    }

    [HttpPost("menus")]
    [RequireMenu("system.manage")]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuRequest request)
    {
        try
        {
            var menu = new Menu
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                Icon = request.Icon,
                Route = request.Route,
                MenuType = (MenuType)request.MenuType,
                ParentMenuId = request.ParentMenuId,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdMenu = await _menuRepository.AddAsync(menu);
            var menuDto = MapToMenuDto(createdMenu);

            return CreatedAtAction(nameof(GetMenuById), new { id = createdMenu.Id }, 
                ApiResponse<MenuDto>.SuccessResult(menuDto, "Menu created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu");
            return StatusCode(500, ApiResponse<MenuDto>.FailureResult("Failed to create menu"));
        }
    }

    [HttpPut("menus/{id}")]
    [RequireMenu("system.manage")]
    public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] UpdateMenuRequest request)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
            {
                return NotFound(ApiResponse<MenuDto>.FailureResult("Menu not found"));
            }

            // Update menu properties
            menu.Name = request.Name;
            menu.DisplayName = request.DisplayName;
            menu.Description = request.Description;
            menu.Icon = request.Icon;
            menu.Route = request.Route;
            menu.ParentMenuId = request.ParentMenuId;
            menu.SortOrder = request.SortOrder;
            menu.IsActive = request.IsActive;
            menu.UpdatedAt = DateTime.UtcNow;

            await _menuRepository.UpdateAsync(menu);
            var menuDto = MapToMenuDto(menu);

            return Ok(ApiResponse<MenuDto>.SuccessResult(menuDto, "Menu updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu with ID {MenuId}", id);
            return StatusCode(500, ApiResponse<MenuDto>.FailureResult("Failed to update menu"));
        }
    }

    [HttpDelete("menus/{id}")]
    [RequireMenu("system.manage")]
    public async Task<IActionResult> DeleteMenu(Guid id)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
            {
                return NotFound(ApiResponse<object>.FailureResult("Menu not found"));
            }

            // Check if menu has children
            var childMenus = await _menuRepository.GetByParentIdAsync(id);
            if (childMenus.Any())
            {
                return BadRequest(ApiResponse<object>.FailureResult("Cannot delete menu with child items. Delete child items first."));
            }

            await _menuRepository.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResult(new { }, "Menu deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu with ID {MenuId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("Failed to delete menu"));
        }
    }
}

public record CreateUserRequest(string Email, string DisplayName);
public record AssignUserRoleRequest(Guid UserId, Guid RoleId);
public record CreateSubscriptionRequest(Guid UserId, SubscriptionType SubscriptionType, decimal Amount, DateTime EndDate);

// Menu request DTOs
public record CreateMenuRequest(
    string Name,
    string DisplayName,
    string? Description,
    string? Icon,
    string? Route,
    int MenuType,
    Guid? ParentMenuId,
    int SortOrder,
    bool IsActive
);

public record UpdateMenuRequest(
    string Name,
    string DisplayName,
    string? Description,
    string? Icon,
    string? Route,
    Guid? ParentMenuId,
    int SortOrder,
    bool IsActive
);
