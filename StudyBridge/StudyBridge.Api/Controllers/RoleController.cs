using Microsoft.AspNetCore.Mvc;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Features.Admin;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[RequirePermission("roles.view")]
public class RoleController : ControllerBase
{
    private readonly IQueryHandler<GetRoles.Query, GetRoles.Response> _getRolesHandler;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IQueryHandler<GetRoles.Query, GetRoles.Response> getRolesHandler,
        ILogger<RoleController> logger)
    {
        _getRolesHandler = getRolesHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var query = new GetRoles.Query();
            var response = await _getRolesHandler.HandleAsync(query);

            return Ok(ApiResponse<GetRoles.Response>.SuccessResult(
                response, "Roles retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, ApiResponse<GetRoles.Response>.FailureResult("Failed to retrieve roles"));
        }
    }

    // TODO: Add Create, Update, Delete methods with proper permission checks
    // [HttpPost]
    // [RequirePermission("roles.create")]
    
    // [HttpPut("{id}")]
    // [RequirePermission("roles.edit")]
    
    // [HttpDelete("{id}")]
    // [RequirePermission("roles.delete")]
}