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
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IMenuRepository _menuRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IQueryHandler<GetUsers.Query, GetUsers.Response> _getUsersHandler;
    private readonly IQueryHandler<GetUserById.Query, GetUserById.Response> _getUserByIdHandler;
    private readonly ICommandHandler<CreateUser.Command, CreateUser.Response> _createUserHandler;
    private readonly IQueryHandler<GetRoles.Query, GetRoles.Response> _getRolesHandler;
    private readonly IQueryHandler<GetPermissions.Query, GetPermissions.Response> _getPermissionsHandler;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IPermissionService permissionService,
        ISubscriptionService subscriptionService,
        IMenuRepository menuRepository,
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository,
        IQueryHandler<GetUsers.Query, GetUsers.Response> getUsersHandler,
        IQueryHandler<GetUserById.Query, GetUserById.Response> getUserByIdHandler,
        ICommandHandler<CreateUser.Command, CreateUser.Response> createUserHandler,
        IQueryHandler<GetRoles.Query, GetRoles.Response> getRolesHandler,
        IQueryHandler<GetPermissions.Query, GetPermissions.Response> getPermissionsHandler,
        ILogger<AdminController> logger)
    {
        _permissionService = permissionService;
        _subscriptionService = subscriptionService;
        _menuRepository = menuRepository;
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
        _getUsersHandler = getUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _createUserHandler = createUserHandler;
        _getRolesHandler = getRolesHandler;
        _getPermissionsHandler = getPermissionsHandler;
        _logger = logger;
    }

    [HttpGet("users")]
    [RequirePermission("users.view")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsers.Query query)
    {
        _logger.LogInformation("Admin viewing users list - Page: {PageNumber}, Size: {PageSize}", query.PageNumber, query.PageSize);
        
        var response = await _getUsersHandler.HandleAsync(query);
        return Ok(ApiResponse<GetUsers.Response>.SuccessResult(response, "Users retrieved successfully"));
    }

    [HttpGet("users/{userId}")]
    [RequirePermission("users.view")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        _logger.LogInformation("Admin viewing user details: {UserId}", userId);
        
        var query = new GetUserById.Query { UserId = userId };
        var response = await _getUserByIdHandler.HandleAsync(query);
        return Ok(ApiResponse<GetUserById.Response>.SuccessResult(response, "User details retrieved successfully"));
    }

    [HttpPost("users")]
    [RequirePermission("users.create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUser.Command command)
    {
        _logger.LogInformation("Admin creating new user: {Email}", command.Email);
        
        var response = await _createUserHandler.HandleAsync(command);
        return Ok(ApiResponse<CreateUser.Response>.SuccessResult(response, "User created successfully"));
    }

    [HttpGet("roles")]
    [RequirePermission("roles.view")]
    public async Task<IActionResult> GetRoles()
    {
        _logger.LogInformation("Admin viewing roles list");
        
        var query = new GetRoles.Query();
        var response = await _getRolesHandler.HandleAsync(query);
        return Ok(ApiResponse<GetRoles.Response>.SuccessResult(response, "Roles retrieved successfully"));
    }

    [HttpGet("permissions")]
    [RequirePermission("permissions.view")]
    public async Task<IActionResult> GetPermissions()
    {
        _logger.LogInformation("Admin viewing permissions tree");
        
        var query = new GetPermissions.Query();
        var response = await _getPermissionsHandler.HandleAsync(query);
        return Ok(ApiResponse<GetPermissions.Response>.SuccessResult(response, "Permissions retrieved successfully"));
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
        var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Unauthorized();

        var success = await _permissionService.AssignRoleToUserAsync(
            request.UserId, 
            request.Role, 
            currentUserIdString);

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
public record AssignRoleRequest(Guid UserId, SystemRole Role);
public record CreateSubscriptionRequest(Guid UserId, SubscriptionType SubscriptionType, decimal Amount, DateTime EndDate);
