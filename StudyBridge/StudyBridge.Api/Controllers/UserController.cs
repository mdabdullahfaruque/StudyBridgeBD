using Microsoft.AspNetCore.Mvc;
using StudyBridge.Infrastructure.Authorization;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Features.Admin;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[RequireMenu("users.view")]
public class UserController : ControllerBase
{
    private readonly IQueryHandler<GetUsers.Query, GetUsers.Response> _getUsersHandler;
    private readonly IQueryHandler<GetUserById.Query, GetUserById.Response> _getUserByIdHandler;
    private readonly ICommandHandler<CreateUser.Command, CreateUser.Response> _createUserHandler;
    private readonly ICommandHandler<UpdateUser.Command, UpdateUser.Response> _updateUserHandler;
    private readonly ICommandHandler<DeleteUser.Command, DeleteUser.Response> _deleteUserHandler;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IQueryHandler<GetUsers.Query, GetUsers.Response> getUsersHandler,
        IQueryHandler<GetUserById.Query, GetUserById.Response> getUserByIdHandler,
        ICommandHandler<CreateUser.Command, CreateUser.Response> createUserHandler,
        ICommandHandler<UpdateUser.Command, UpdateUser.Response> updateUserHandler,
        ICommandHandler<DeleteUser.Command, DeleteUser.Response> deleteUserHandler,
        ILogger<UserController> logger)
    {
        _getUsersHandler = getUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _createUserHandler = createUserHandler;
        _updateUserHandler = updateUserHandler;
        _deleteUserHandler = deleteUserHandler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsers.Query query)
    {
        try
        {
            var response = await _getUsersHandler.HandleAsync(query);
            return Ok(ApiResponse<GetUsers.Response>.SuccessResult(
                response, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, ApiResponse<GetUsers.Response>.FailureResult("Failed to retrieve users"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            var query = new GetUserById.Query { UserId = id };
            var response = await _getUserByIdHandler.HandleAsync(query);

            return Ok(ApiResponse<GetUserById.Response>.SuccessResult(
                response, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return StatusCode(500, ApiResponse<GetUserById.Response>.FailureResult("Failed to retrieve user"));
        }
    }

    [HttpPost]
    [RequireMenu("users.create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUser.Command command)
    {
        try
        {
            var response = await _createUserHandler.HandleAsync(command);

            return CreatedAtAction(nameof(GetUserById), new { id = response.User?.Id }, 
                ApiResponse<CreateUser.Response>.SuccessResult(response, "User created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponse<CreateUser.Response>.FailureResult("Failed to create user"));
        }
    }

    [HttpPut("{id}")]
    [RequireMenu("users.edit")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUser.Command command)
    {
        try
        {
            // Ensure the ID from the route matches the command
            command.Id = id;
            
            var response = await _updateUserHandler.HandleAsync(command);

            return Ok(ApiResponse<UpdateUser.Response>.SuccessResult(
                response, "User updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid user data provided for user: {UserId}", id);
            return BadRequest(ApiResponse<UpdateUser.Response>.FailureResult("Invalid user data provided"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User update failed: {UserId}", id);
            return NotFound(ApiResponse<UpdateUser.Response>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return StatusCode(500, ApiResponse<UpdateUser.Response>.FailureResult("Failed to update user"));
        }
    }

    [HttpDelete("{id}")]
    [RequireMenu("users.delete")]
    public async Task<IActionResult> DeleteUser(string id, [FromQuery] bool forceDelete = false)
    {
        try
        {
            var command = new DeleteUser.Command 
            { 
                Id = id, 
                ForceDelete = forceDelete 
            };
            
            var response = await _deleteUserHandler.HandleAsync(command);

            return Ok(ApiResponse<DeleteUser.Response>.SuccessResult(
                response, response.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid user ID format: {UserId}", id);
            return BadRequest(ApiResponse<DeleteUser.Response>.FailureResult("Invalid user ID format"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User deletion failed: {UserId}", id);
            return Conflict(ApiResponse<DeleteUser.Response>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return StatusCode(500, ApiResponse<DeleteUser.Response>.FailureResult("Failed to delete user"));
        }
    }
}