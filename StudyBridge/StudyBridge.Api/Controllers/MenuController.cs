using Microsoft.AspNetCore.Mvc;
using StudyBridge.Api.DTOs;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuRepository _menuRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IMenuRepository menuRepository,
        IRoleRepository roleRepository,
        ILogger<MenuController> logger)
    {
        _menuRepository = menuRepository;
        _roleRepository = roleRepository;
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

            _logger.LogInformation("Getting menus for user: {UserId}", userId);

            // Get user's roles to determine menu access
            var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!roleClaims.Any())
            {
                _logger.LogWarning("No roles found for user: {UserId}", userId);
                return Ok(ApiResponse<List<MenuDto>>.SuccessResult(new List<MenuDto>(), "No menus available"));
            }

            _logger.LogInformation("User has {RoleCount} roles: {Roles}", roleClaims.Count, string.Join(", ", roleClaims));

            // Convert role names to role IDs
            var userRoleIds = new List<Guid>();
            foreach (var roleName in roleClaims)
            {
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role != null)
                {
                    userRoleIds.Add(role.Id);
                }
            }

            if (!userRoleIds.Any())
            {
                _logger.LogWarning("No valid role IDs found for user: {UserId}", userId);
                return Ok(ApiResponse<List<MenuDto>>.SuccessResult(new List<MenuDto>(), "No menus available"));
            }

            _logger.LogInformation("User has {RoleCount} valid role IDs: {RoleIds}", userRoleIds.Count, string.Join(", ", userRoleIds));

            // Get menus accessible by user's roles

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

    [HttpGet("debug")]
    public async Task<IActionResult> DebugMenus()
    {
        try
        {
            _logger.LogInformation("Debug menu endpoint called");

            // Get all active menus
            var allMenus = await _menuRepository.GetAllAsync();
            var activeMenus = allMenus.Where(m => m.IsActive).ToList();

            var debugInfo = new
            {
                TotalMenus = activeMenus.Count,
                Menus = activeMenus.Select(m => new
                {
                    Id = m.Id,
                    Name = m.Name,
                    DisplayName = m.DisplayName,
                    Route = m.Route,
                    MenuType = m.MenuType.ToString(),
                    ParentId = m.ParentMenuId,
                    SortOrder = m.SortOrder,
                    IsActive = m.IsActive
                }).ToList()
            };

            return Ok(ApiResponse<object>.SuccessResult(debugInfo, "Debug info retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debug endpoint");
            return StatusCode(500, ApiResponse<object>.FailureResult("Debug failed"));
        }
    }

    [HttpGet]
    [RequireMenu("system.view")]
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

    [HttpPost]
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
                // HasCrudPermissions removed
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
    [RequireMenu("system.manage")]
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

    private static MenuDto MapToMenuDtoWithChildren(Menu menu, List<Menu> allMenus)
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
}