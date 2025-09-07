using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyBridge.Shared.Controllers;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Features.UserProfile;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProfileController : BaseController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var result = await _profileService.GetProfileAsync(userId);
        return HandleServiceResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfile.Request request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var result = await _profileService.UpdateProfileAsync(userId, request);
        return HandleServiceResult(result);
    }
}
