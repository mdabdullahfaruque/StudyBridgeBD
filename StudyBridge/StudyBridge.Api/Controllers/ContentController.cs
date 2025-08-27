using Microsoft.AspNetCore.Mvc;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Infrastructure.Authorization;
using System.Security.Claims;

namespace StudyBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController : ControllerBase
{
    private readonly ILogger<ContentController> _logger;

    public ContentController(ILogger<ContentController> logger)
    {
        _logger = logger;
    }

    [HttpGet("vocabulary")]
    [RequireSubscription(SubscriptionType.VocabularyOnly)]
    public async Task<IActionResult> GetVocabularyContent()
    {
        _logger.LogInformation("User accessing vocabulary content");
        return Ok(new { message = "Vocabulary content retrieved successfully" });
    }

    [HttpGet("ielts")]
    [RequireSubscription(SubscriptionType.IeltsOnly)]
    public async Task<IActionResult> GetIeltsContent()
    {
        _logger.LogInformation("User accessing IELTS content");
        return Ok(new { message = "IELTS content retrieved successfully" });
    }

    [HttpGet("premium")]
    [RequireSubscription(SubscriptionType.Premium)]
    public async Task<IActionResult> GetPremiumContent()
    {
        _logger.LogInformation("User accessing premium content");
        return Ok(new { message = "Premium content retrieved successfully" });
    }

    [HttpPost("vocabulary")]
    [RequirePermission(Permission.ManageVocabularyModule)]
    public async Task<IActionResult> CreateVocabularyContent([FromBody] CreateContentRequest request)
    {
        _logger.LogInformation("Creating vocabulary content: {Title}", request.Title);
        return Ok(new { message = "Vocabulary content created successfully" });
    }

    [HttpPost("ielts")]
    [RequirePermission(Permission.ManageIeltsModule)]
    public async Task<IActionResult> CreateIeltsContent([FromBody] CreateContentRequest request)
    {
        _logger.LogInformation("Creating IELTS content: {Title}", request.Title);
        return Ok(new { message = "IELTS content created successfully" });
    }
}

public record CreateContentRequest(string Title, string Description, string Content);
