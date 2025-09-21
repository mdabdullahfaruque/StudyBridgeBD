using Microsoft.AspNetCore.Mvc;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Features.Admin;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IQueryHandler<GetMenus.Query, GetMenus.Response> _getMenusHandler;
    private readonly IQueryHandler<GetMenuById.Query, GetMenuById.Response> _getMenuByIdHandler;
    private readonly ICommandHandler<CreateMenu.Command, CreateMenu.Response> _createMenuHandler;
    private readonly ICommandHandler<UpdateMenu.Command, UpdateMenu.Response> _updateMenuHandler;
    private readonly ICommandHandler<DeleteMenu.Command, DeleteMenu.Response> _deleteMenuHandler;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IQueryHandler<GetMenus.Query, GetMenus.Response> getMenusHandler,
        IQueryHandler<GetMenuById.Query, GetMenuById.Response> getMenuByIdHandler,
        ICommandHandler<CreateMenu.Command, CreateMenu.Response> createMenuHandler,
        ICommandHandler<UpdateMenu.Command, UpdateMenu.Response> updateMenuHandler,
        ICommandHandler<DeleteMenu.Command, DeleteMenu.Response> deleteMenuHandler,
        ILogger<MenuController> logger)
    {
        _getMenusHandler = getMenusHandler;
        _getMenuByIdHandler = getMenuByIdHandler;
        _createMenuHandler = createMenuHandler;
        _updateMenuHandler = updateMenuHandler;
        _deleteMenuHandler = deleteMenuHandler;
        _logger = logger;
    }

    [HttpGet("user-menus")]
    public async Task<IActionResult> GetUserMenus()
    {
        try
        {
            var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!roleClaims.Any())
            {
                return Ok(ApiResponse<GetMenus.Response>.SuccessResult(
                    new GetMenus.Response { Menus = new List<GetMenus.MenuDto>() }, 
                    "No menus available"));
            }

            var query = new GetMenus.Query
            {
                ForUser = true,
                UserRoles = roleClaims,
                IncludeInactive = false
            };

            var response = await _getMenusHandler.HandleAsync(query);
            return Ok(ApiResponse<GetMenus.Response>.SuccessResult(response, "User menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user menus");
            return StatusCode(500, ApiResponse<GetMenus.Response>.FailureResult("Failed to retrieve user menus"));
        }
    }

    [HttpGet]
    [RequireMenu("menus.view")]
    public async Task<IActionResult> GetMenus([FromQuery] string? menuType = null, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = new GetMenus.Query { IncludeInactive = includeInactive };
            if (!string.IsNullOrEmpty(menuType) && Enum.TryParse<Domain.Enums.MenuType>(menuType, true, out var parsedMenuType))
            {
                query.MenuType = parsedMenuType;
            }

            var response = await _getMenusHandler.HandleAsync(query);
            return Ok(ApiResponse<GetMenus.Response>.SuccessResult(response, "Menus retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menus");
            return StatusCode(500, ApiResponse<GetMenus.Response>.FailureResult("Failed to retrieve menus"));
        }
    }

    [HttpGet("{id}")]
    [RequireMenu("menus.view")]
    public async Task<IActionResult> GetMenuById(string id, [FromQuery] bool includeChildren = true, [FromQuery] bool includeRoles = false)
    {
        try
        {
            var query = new GetMenuById.Query { Id = id, IncludeChildren = includeChildren, IncludeRoles = includeRoles };
            var response = await _getMenuByIdHandler.HandleAsync(query);
            return Ok(ApiResponse<GetMenuById.Response>.SuccessResult(response, "Menu retrieved successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid menu ID format: {MenuId}", id);
            return BadRequest(ApiResponse<GetMenuById.Response>.FailureResult("Invalid menu ID format"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Menu not found: {MenuId}", id);
            return NotFound(ApiResponse<GetMenuById.Response>.FailureResult("Menu not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu: {MenuId}", id);
            return StatusCode(500, ApiResponse<GetMenuById.Response>.FailureResult("Failed to retrieve menu"));
        }
    }

    [HttpPost]
    [RequireMenu("menus.create")]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenu.Command command)
    {
        try
        {
            var response = await _createMenuHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetMenuById), new { id = response.Menu.Id }, 
                ApiResponse<CreateMenu.Response>.SuccessResult(response, "Menu created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu: {MenuName}", command.Name);
            return StatusCode(500, ApiResponse<CreateMenu.Response>.FailureResult("Failed to create menu"));
        }
    }

    [HttpPut("{id}")]
    [RequireMenu("menus.edit")]
    public async Task<IActionResult> UpdateMenu(string id, [FromBody] UpdateMenu.Command command)
    {
        try
        {
            command.MenuId = id;
            var response = await _updateMenuHandler.HandleAsync(command);
            return Ok(ApiResponse<UpdateMenu.Response>.SuccessResult(response, "Menu updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu: {MenuId}", id);
            return StatusCode(500, ApiResponse<UpdateMenu.Response>.FailureResult("Failed to update menu"));
        }
    }

    [HttpDelete("{id}")]
    [RequireMenu("menus.delete")]
    public async Task<IActionResult> DeleteMenu(string id, [FromQuery] bool forceDelete = false)
    {
        try
        {
            var command = new DeleteMenu.Command { MenuId = id, ForceDelete = forceDelete };
            var response = await _deleteMenuHandler.HandleAsync(command);
            return Ok(ApiResponse<DeleteMenu.Response>.SuccessResult(response, response.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu: {MenuId}", id);
            return StatusCode(500, ApiResponse<DeleteMenu.Response>.FailureResult("Failed to delete menu"));
        }
    }
}
