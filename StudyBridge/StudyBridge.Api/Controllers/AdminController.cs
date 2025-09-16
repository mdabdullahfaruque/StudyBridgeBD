using Microsoft.AspNetCore.Mvc;
using StudyBridge.Api.DTOs;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IMenuRepository _menuRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IPermissionService permissionService,
        ISubscriptionService subscriptionService,
        IMenuRepository menuRepository,
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository,
        ILogger<AdminController> logger)
    {
        _permissionService = permissionService;
        _subscriptionService = subscriptionService;
        _menuRepository = menuRepository;
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    [HttpGet("users")]
    [RequirePermission("users.view")]
    public IActionResult GetUsers()
    {
        _logger.LogInformation("Admin viewing users list");
        return Ok(new { message = "Users list retrieved successfully" });
    }

    [HttpPost("users")]
    // [RequirePermission(SystemPermission.CreateUsers)] // TODO: Update after RBAC implementation
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Admin creating new user: {Email}", request.Email);
        return Ok(new { message = "User created successfully" });
    }

    [HttpDelete("users/{userId}")]
    // [RequirePermission(SystemPermission.DeleteUsers)] // TODO: Update after RBAC implementation
    public IActionResult DeleteUser(string userId)
    {
        _logger.LogWarning("Admin deleting user: {UserId}", userId);
        return Ok(new { message = "User deleted successfully" });
    }

    [HttpGet("financials")]
    [RequireRole(SystemRole.Finance, SystemRole.Admin, SystemRole.SuperAdmin)]
    public async Task<IActionResult> GetFinancials()
    {
        _logger.LogInformation("Accessing financial data");
        return Ok(new { message = "Financial data retrieved successfully" });
    }

    [HttpPost("assign-role")]
    // [RequirePermission(SystemPermission.ManageUserRoles)] // TODO: Update after RBAC implementation
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized();

        var success = await _permissionService.AssignRoleToUserAsync(
            request.UserId, 
            request.Role, 
            currentUserId);

        if (success)
        {
            _logger.LogInformation("Role {Role} assigned to user {UserId} by {AssignedBy}", 
                request.Role, request.UserId, currentUserId);
            return Ok(new { message = "Role assigned successfully" });
        }

        return BadRequest(new { message = "Failed to assign role" });
    }

    [HttpPost("create-subscription")]
    // [RequirePermission(SystemPermission.ManageSubscriptions)] // TODO: Update after RBAC implementation
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
    // [RequirePermission(SystemPermission.ViewSystemLogs)] // TODO: Update after RBAC implementation
    public async Task<IActionResult> GetSystemLogs()
    {
        _logger.LogInformation("Admin accessing system logs");
        return Ok(new { message = "System logs retrieved successfully" });
    }

    // Menu Management Endpoints
    [HttpGet("menus")]
    [RequirePermission("system.view")]
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
            // TODO: Get actual user permissions from JWT token
            // For now, simulate SuperAdmin permissions
            var userPermissions = new List<string>
            {
                "dashboard.view", "users.view", "users.create", "users.edit", "users.delete", "users.manage",
                "roles.view", "roles.create", "roles.edit", "roles.delete", "roles.manage",
                "permissions.view", "permissions.create", "permissions.edit", "permissions.delete", "permissions.manage",
                "content.view", "content.create", "content.edit", "content.delete", "content.manage",
                "financials.view", "financials.manage", "reports.view", "analytics.view",
                "system.view", "system.manage", "system.logs"
            };

            var allMenus = await _menuRepository.GetAllAsync();
            var adminMenus = allMenus.Where(m => m.MenuType == MenuType.Admin && m.IsActive).ToList();

            // Get permissions for each menu
            var menuPermissions = new Dictionary<Guid, List<string>>();
            foreach (var menu in adminMenus)
            {
                var permissions = await _permissionRepository.GetByMenuIdAsync(menu.Id);
                menuPermissions[menu.Id] = permissions.Where(p => p.IsActive)
                                                    .Select(p => p.PermissionKey)
                                                    .ToList();
            }

            // Filter menus based on user permissions
            var accessibleMenus = adminMenus.Where(menu =>
            {
                var requiredPermissions = menuPermissions.GetValueOrDefault(menu.Id, new List<string>());
                return requiredPermissions.Count == 0 || 
                       requiredPermissions.Any(p => userPermissions.Contains(p));
            }).ToList();

            // Build hierarchy and convert to DTOs
            var menuHierarchy = BuildMenuHierarchy(accessibleMenus);
            var menuDtos = menuHierarchy.Select(m => MapToMenuDtoWithChildren(m, accessibleMenus, menuPermissions, userPermissions))
                                       .ToList();

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

    private static MenuDto MapToMenuDtoWithChildren(Menu menu, List<Menu> allMenus, Dictionary<Guid, List<string>> menuPermissions, List<string> userPermissions)
    {
        var requiredPermissions = menuPermissions.GetValueOrDefault(menu.Id, new List<string>());
        
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
            RequiredPermissions = requiredPermissions,
            Children = allMenus.Where(m => m.ParentMenuId == menu.Id)
                              .Where(child =>
                              {
                                  var childPermissions = menuPermissions.GetValueOrDefault(child.Id, new List<string>());
                                  return childPermissions.Count == 0 || 
                                         childPermissions.Any(p => userPermissions.Contains(p));
                              })
                              .OrderBy(m => m.SortOrder)
                              .Select(m => MapToMenuDtoWithChildren(m, allMenus, menuPermissions, userPermissions))
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
    [RequirePermission("public.dashboard")]
    public async Task<IActionResult> GetPublicMenus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User not authenticated" });
            }

            // For now, simulate User permissions for public menus
            var userPermissions = new List<string>
            {
                "public.dashboard", "public.vocabulary", "public.learning"
            };
            
            // Get all public menus
            var allMenus = await _menuRepository.GetAllAsync();
            var publicMenus = allMenus.Where(m => m.MenuType == MenuType.Public && m.IsActive).ToList();

            // Get permissions for public menus
            var menuPermissions = new Dictionary<Guid, List<string>>();
            foreach (var menu in publicMenus)
            {
                var permissions = await _permissionRepository.GetByMenuIdAsync(menu.Id);
                menuPermissions[menu.Id] = permissions.Where(p => p.IsActive)
                                                    .Select(p => p.PermissionKey)
                                                    .ToList();
            }

            // Filter menus by user permissions and build hierarchy
            var accessibleMenus = publicMenus.Where(menu =>
            {
                var requiredPermissions = menuPermissions.GetValueOrDefault(menu.Id, new List<string>());
                return requiredPermissions.Count == 0 || requiredPermissions.Any(p => userPermissions.Contains(p));
            }).ToList();

            var menuHierarchy = BuildMenuHierarchy(accessibleMenus);
            var menuDtos = menuHierarchy.Select(menu => MapToMenuDtoWithChildren(menu, accessibleMenus, menuPermissions, userPermissions)).ToList();

            return Ok(ApiResponse<List<MenuDto>>.SuccessResult(menuDtos, "Public menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public menus for user");
            return StatusCode(500, ApiResponse<List<MenuDto>>.FailureResult("An error occurred while retrieving public menus"));
        }
    }
}

public record CreateUserRequest(string Email, string DisplayName);
public record AssignRoleRequest(string UserId, SystemRole Role);
public record CreateSubscriptionRequest(string UserId, SubscriptionType SubscriptionType, decimal Amount, DateTime EndDate);
