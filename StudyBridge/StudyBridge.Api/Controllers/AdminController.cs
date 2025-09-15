using Microsoft.AspNetCore.Mvc;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Infrastructure.Authorization;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IPermissionService permissionService,
        ISubscriptionService subscriptionService,
        ILogger<AdminController> logger)
    {
        _permissionService = permissionService;
        _subscriptionService = subscriptionService;
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
}

public record CreateUserRequest(string Email, string DisplayName);
public record AssignRoleRequest(string UserId, SystemRole Role);
public record CreateSubscriptionRequest(string UserId, SubscriptionType SubscriptionType, decimal Amount, DateTime EndDate);
