using Microsoft.AspNetCore.Mvc;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Features.Admin;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[RequirePermission("permissions.view")]
public class PermissionController : ControllerBase
{
    private readonly IQueryHandler<GetPermissions.Query, GetPermissions.Response> _getPermissionsHandler;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(
        IQueryHandler<GetPermissions.Query, GetPermissions.Response> getPermissionsHandler,
        ILogger<PermissionController> logger)
    {
        _getPermissionsHandler = getPermissionsHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPermissions()
    {
        try
        {
            var query = new GetPermissions.Query();
            var response = await _getPermissionsHandler.HandleAsync(query);

            return Ok(ApiResponse<GetPermissions.Response>.SuccessResult(
                response, "Permissions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            return StatusCode(500, ApiResponse<GetPermissions.Response>.FailureResult("Failed to retrieve permissions"));
        }
    }

    // TODO: Add Create, Update, Delete methods with proper permission checks
    // [HttpPost]
    // [RequirePermission("permissions.create")]
    
    // [HttpPut("{id}")]
    // [RequirePermission("permissions.edit")]
    
    // [HttpDelete("{id}")]
    // [RequirePermission("permissions.delete")]
}