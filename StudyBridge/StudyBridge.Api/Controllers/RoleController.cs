using Microsoft.AspNetCore.Mvc;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Features.Admin;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[RequireMenu("roles.view")]
public class RoleController : ControllerBase
{
    private readonly IQueryHandler<GetRoles.Query, GetRoles.Response> _getRolesHandler;
    private readonly IQueryHandler<GetRoleById.Query, GetRoleById.Response> _getRoleByIdHandler;
    private readonly ICommandHandler<CreateRole.Command, CreateRole.Response> _createRoleHandler;
    private readonly ICommandHandler<UpdateRole.Command, UpdateRole.Response> _updateRoleHandler;
    private readonly ICommandHandler<DeleteRole.Command, DeleteRole.Response> _deleteRoleHandler;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IQueryHandler<GetRoles.Query, GetRoles.Response> getRolesHandler,
        IQueryHandler<GetRoleById.Query, GetRoleById.Response> getRoleByIdHandler,
        ICommandHandler<CreateRole.Command, CreateRole.Response> createRoleHandler,
        ICommandHandler<UpdateRole.Command, UpdateRole.Response> updateRoleHandler,
        ICommandHandler<DeleteRole.Command, DeleteRole.Response> deleteRoleHandler,
        ILogger<RoleController> logger)
    {
        _getRolesHandler = getRolesHandler;
        _getRoleByIdHandler = getRoleByIdHandler;
        _createRoleHandler = createRoleHandler;
        _updateRoleHandler = updateRoleHandler;
        _deleteRoleHandler = deleteRoleHandler;
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

    [HttpGet("{id}")]
    [RequireMenu("roles.view")]
    public async Task<IActionResult> GetRoleById(string id)
    {
        try
        {
            var query = new GetRoleById.Query { Id = id };
            var response = await _getRoleByIdHandler.HandleAsync(query);

            return Ok(ApiResponse<GetRoleById.Response>.SuccessResult(
                response, "Role retrieved successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid role ID format: {RoleId}", id);
            return BadRequest(ApiResponse<GetRoleById.Response>.FailureResult("Invalid role ID format"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Role not found: {RoleId}", id);
            return NotFound(ApiResponse<GetRoleById.Response>.FailureResult("Role not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role: {RoleId}", id);
            return StatusCode(500, ApiResponse<GetRoleById.Response>.FailureResult("Failed to retrieve role"));
        }
    }

    [HttpPost]
    [RequireMenu("roles.create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRole.Command command)
    {
        try
        {
            var response = await _createRoleHandler.HandleAsync(command);

            return CreatedAtAction(
                nameof(GetRoleById), 
                new { id = response.Role.Id }, 
                ApiResponse<CreateRole.Response>.SuccessResult(response, "Role created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid role data provided");
            return BadRequest(ApiResponse<CreateRole.Response>.FailureResult("Invalid role data provided"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Role creation failed");
            return Conflict(ApiResponse<CreateRole.Response>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {RoleName}", command.Name);
            return StatusCode(500, ApiResponse<CreateRole.Response>.FailureResult("Failed to create role"));
        }
    }

    [HttpPut("{id}")]
    [RequireMenu("roles.edit")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRole.Command command)
    {
        try
        {
            // Ensure the ID from the route matches the command
            command.Id = id;
            
            var response = await _updateRoleHandler.HandleAsync(command);

            return Ok(ApiResponse<UpdateRole.Response>.SuccessResult(
                response, "Role updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid role data provided for role: {RoleId}", id);
            return BadRequest(ApiResponse<UpdateRole.Response>.FailureResult("Invalid role data provided"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Role update failed: {RoleId}", id);
            return NotFound(ApiResponse<UpdateRole.Response>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            return StatusCode(500, ApiResponse<UpdateRole.Response>.FailureResult("Failed to update role"));
        }
    }

    [HttpDelete("{id}")]
    [RequireMenu("roles.delete")]
    public async Task<IActionResult> DeleteRole(string id, [FromQuery] bool forceDelete = false)
    {
        try
        {
            var command = new DeleteRole.Command 
            { 
                Id = id, 
                ForceDelete = forceDelete 
            };
            
            var response = await _deleteRoleHandler.HandleAsync(command);

            return Ok(ApiResponse<DeleteRole.Response>.SuccessResult(
                response, response.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid role ID format: {RoleId}", id);
            return BadRequest(ApiResponse<DeleteRole.Response>.FailureResult("Invalid role ID format"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Role deletion failed: {RoleId}", id);
            return Conflict(ApiResponse<DeleteRole.Response>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", id);
            return StatusCode(500, ApiResponse<DeleteRole.Response>.FailureResult("Failed to delete role"));
        }
    }
}