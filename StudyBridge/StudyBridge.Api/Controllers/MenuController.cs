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
[Route("api/v1/[controller]")]
[RequirePermission("system.view")]
public class MenuController : ControllerBase
{
    private readonly IMenuRepository _menuRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IMenuRepository menuRepository,
        IPermissionRepository permissionRepository,
        IPermissionService permissionService,
        ILogger<MenuController> logger)
    {
        _menuRepository = menuRepository;
        _permissionRepository = permissionRepository;
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpGet("user-menus")]
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

            // Get actual user permissions from database
            var userPermissionsEntities = await _permissionService.GetUserPermissionsAsync(userId);
            var userPermissions = userPermissionsEntities.Select(p => p.PermissionKey).ToList();

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

    [HttpGet]
    [RequirePermission("system.view")]
    public async Task<IActionResult> GetAllMenus()
    {
        try
        {
            var menus = await _menuRepository.GetAllAsync();
            var menuDtos = menus.Select(MapToMenuDto).ToList();
            return Ok(ApiResponse<List<MenuDto>>.SuccessResult(menuDtos, "Menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all menus");
            return StatusCode(500, ApiResponse<List<MenuDto>>.FailureResult("Failed to retrieve menus"));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("system.view")]
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

    [HttpPost]
    [RequirePermission("system.manage")]
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
                HasCrudPermissions = request.HasCrudPermissions,
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

    [HttpPut("{id}")]
    [RequirePermission("system.manage")]
    public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] UpdateMenuRequest request)
    {
        try
        {
            var existingMenu = await _menuRepository.GetByIdAsync(id);
            if (existingMenu == null)
            {
                return NotFound(ApiResponse<MenuDto>.FailureResult("Menu not found"));
            }

            existingMenu.Name = request.Name;
            existingMenu.DisplayName = request.DisplayName;
            existingMenu.Description = request.Description;
            existingMenu.Icon = request.Icon;
            existingMenu.Route = request.Route;
            existingMenu.ParentMenuId = request.ParentMenuId;
            existingMenu.SortOrder = request.SortOrder;
            existingMenu.IsActive = request.IsActive;
            existingMenu.HasCrudPermissions = request.HasCrudPermissions;
            existingMenu.UpdatedAt = DateTime.UtcNow;

            await _menuRepository.UpdateAsync(existingMenu);
            var menuDto = MapToMenuDto(existingMenu);

            return Ok(ApiResponse<MenuDto>.SuccessResult(menuDto, "Menu updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu with ID {MenuId}", id);
            return StatusCode(500, ApiResponse<MenuDto>.FailureResult("Failed to update menu"));
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("system.manage")]
    public async Task<IActionResult> DeleteMenu(Guid id)
    {
        try
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null)
            {
                return NotFound(ApiResponse<object>.FailureResult("Menu not found"));
            }

            await _menuRepository.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResult("Menu deleted successfully", "Menu deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu with ID {MenuId}", id);
            return StatusCode(500, ApiResponse<object>.FailureResult("Failed to delete menu"));
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
            Description = menu.Description,
            Icon = menu.Icon,
            Route = menu.Route,
            ParentId = menu.ParentMenuId?.ToString(),
            SortOrder = menu.SortOrder,
            IsActive = menu.IsActive,
            MenuType = (int)menu.MenuType,
            RequiredPermissions = new List<string>() // Will be populated by calling code
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
            Description = menu.Description,
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
}