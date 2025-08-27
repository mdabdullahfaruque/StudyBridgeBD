using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyBridge.Shared.CQRS;
using StudyBridge.Shared.Common;
using StudyBridge.UserManagement.Application.Commands;
using StudyBridge.UserManagement.Application.DTOs;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public AuthController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.DisplayName);
                
            var result = await _dispatcher.CommandAsync(command);
            
            return Ok(ApiResponse<LoginResponse>.SuccessResult(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LoginResponse>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<LoginResponse>.FailureResult($"Registration failed: {ex.Message}"));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await _dispatcher.CommandAsync(command);
            
            return Ok(ApiResponse<LoginResponse>.SuccessResult(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<LoginResponse>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<LoginResponse>.FailureResult($"Login failed: {ex.Message}"));
        }
    }

    [HttpPost("google")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var command = new GoogleLoginCommand(request.IdToken);
            var result = await _dispatcher.CommandAsync(command);
            
            return Ok(ApiResponse<LoginResponse>.SuccessResult(result, "Login successful"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<LoginResponse>.FailureResult($"Login failed: {ex.Message}"));
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<bool>.FailureResult("User not authenticated"));

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            var result = await _dispatcher.CommandAsync(command);
            
            return Ok(ApiResponse<bool>.SuccessResult(result, "Password changed successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<bool>.FailureResult(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.FailureResult($"Password change failed: {ex.Message}"));
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var userInfo = new
            {
                UserId = userId,
                Email = email,
                Roles = roles,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            };

            return Ok(ApiResponse<object>.SuccessResult(userInfo, "User information retrieved"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult($"Failed to get user info: {ex.Message}"));
        }
    }
}
