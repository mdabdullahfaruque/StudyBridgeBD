using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyBridge.Shared.Controllers;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Features.Authentication;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register.Command command)
    {
        var result = await _authenticationService.RegisterAsync(command);
        return HandleServiceResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login.Command command)
    {
        var result = await _authenticationService.LoginAsync(command);
        return HandleServiceResult(result);
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLogin.Command command)
    {
        var result = await _authenticationService.GoogleLoginAsync(command);
        return HandleServiceResult(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePassword.Command command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        // Set the UserId from the authenticated user
        var passwordChangeCommand = new ChangePassword.Command
        {
            UserId = userId,
            CurrentPassword = command.CurrentPassword,
            NewPassword = command.NewPassword
        };

        var result = await _authenticationService.ChangePasswordAsync(passwordChangeCommand);
        return HandleServiceResult(result);
    }
}