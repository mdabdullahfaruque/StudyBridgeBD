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
    private readonly ILogger<UserController> _logger;

    public UserController(
        IQueryHandler<GetUsers.Query, GetUsers.Response> getUsersHandler,
        IQueryHandler<GetUserById.Query, GetUserById.Response> getUserByIdHandler,
        ICommandHandler<CreateUser.Command, CreateUser.Response> createUserHandler,
        ILogger<UserController> logger)
    {
        _getUsersHandler = getUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _createUserHandler = createUserHandler;
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

    // TODO: Add Update, Delete methods with proper permission checks
    // [HttpPut("{id}")]
    // [RequireMenu("users.edit")]
    
    // [HttpDelete("{id}")]
    // [RequireMenu("users.delete")]
}