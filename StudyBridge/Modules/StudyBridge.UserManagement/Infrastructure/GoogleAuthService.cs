using Microsoft.Extensions.Configuration;
using StudyBridge.UserManagement.Application.Services;
using System.Text.Json;

namespace StudyBridge.UserManagement.Infrastructure;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public GoogleAuthService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<GoogleUserInfo> ValidateTokenAsync(string idToken)
    {
        var clientId = _configuration["GoogleAuth:ClientId"];
        var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}";
        
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException("Invalid Google token");
        }

        var json = await response.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json);

        if (tokenInfo?.Aud != clientId)
        {
            throw new UnauthorizedAccessException("Invalid audience");
        }

        return new GoogleUserInfo
        {
            Sub = tokenInfo.Sub ?? string.Empty,
            Email = tokenInfo.Email ?? string.Empty,
            Name = tokenInfo.Name ?? string.Empty,
            Picture = tokenInfo.Picture ?? string.Empty
        };
    }
}

internal record GoogleTokenInfo
{
    public string? Sub { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? Picture { get; init; }
    public string? Aud { get; init; }
}
