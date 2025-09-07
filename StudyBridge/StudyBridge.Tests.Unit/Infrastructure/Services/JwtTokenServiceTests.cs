using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using StudyBridge.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace StudyBridge.Tests.Unit.Infrastructure.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtTokenService _sut;
    private const string TestSecretKey = "this-is-a-test-secret-key-that-is-at-least-32-characters-long";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";
    private const string TestExpirationMinutes = "60";

    public JwtTokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(x => x["JWT:SecretKey"]).Returns(TestSecretKey);
        _mockConfiguration.Setup(x => x["JWT:Issuer"]).Returns(TestIssuer);
        _mockConfiguration.Setup(x => x["JWT:Audience"]).Returns(TestAudience);
        _mockConfiguration.Setup(x => x["JWT:ExpirationMinutes"]).Returns(TestExpirationMinutes);

        _sut = new JwtTokenService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_WithValidParameters_ShouldReturnValidJwtToken()
    {
        // Arrange
        const string userId = "test-user-id";
        const string email = "test@example.com";
        var roles = new List<string> { "User", "Admin" };

        // Act
        var token = _sut.GenerateToken(userId, email, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        jsonToken.Should().NotBeNull();
        jsonToken.Issuer.Should().Be(TestIssuer);
        jsonToken.Audiences.Should().Contain(TestAudience);
        
        var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(userId);
        
        var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(email);
        
        var roleClaims = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(2);
        roleClaims.Select(c => c.Value).Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void GenerateToken_WithEmptyRoles_ShouldReturnTokenWithoutRoleClaims()
    {
        // Arrange
        const string userId = "test-user-id";
        const string email = "test@example.com";
        var roles = new List<string>();

        // Act
        var token = _sut.GenerateToken(userId, email, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var roleClaims = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Should().BeEmpty();
    }

    [Fact]
    public void GenerateToken_ShouldIncludeRequiredStandardClaims()
    {
        // Arrange
        const string userId = "test-user-id";
        const string email = "test@example.com";
        var roles = new List<string> { "User" };

        // Act
        var token = _sut.GenerateToken(userId, email, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        // Check standard JWT claims
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const string email = "test@example.com";
        var roles = new List<string> { "User" };
        
        var token = _sut.GenerateToken(userId, email, roles);

        // Act
        var result = _sut.ValidateToken(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        const string invalidToken = "invalid.jwt.token";

        // Act
        var result = _sut.ValidateToken(invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        // Create a service with very short expiration
        var shortExpirationConfig = new Mock<IConfiguration>();
        shortExpirationConfig.Setup(x => x["JWT:SecretKey"]).Returns(TestSecretKey);
        shortExpirationConfig.Setup(x => x["JWT:Issuer"]).Returns(TestIssuer);
        shortExpirationConfig.Setup(x => x["JWT:Audience"]).Returns(TestAudience);
        shortExpirationConfig.Setup(x => x["JWT:ExpirationMinutes"]).Returns("-1"); // Expired immediately
        
        var expiredTokenService = new JwtTokenService(shortExpirationConfig.Object);
        var expiredToken = expiredTokenService.GenerateToken("user", "test@example.com", new List<string>());

        // Act
        var result = _sut.ValidateToken(expiredToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        const string userId = "test-user-id";
        const string email = "test@example.com";
        var roles = new List<string> { "User" };
        
        var token = _sut.GenerateToken(userId, email, roles);

        // Act
        var result = _sut.GetUserIdFromToken(token);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        const string invalidToken = "invalid.jwt.token";

        // Act
        var result = _sut.GetUserIdFromToken(invalidToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMissingConfiguration_ShouldUseDefaultValues()
    {
        // Arrange
        var emptyConfig = new Mock<IConfiguration>();
        emptyConfig.Setup(x => x["JWT:SecretKey"]).Returns((string?)null);
        emptyConfig.Setup(x => x["JWT:Issuer"]).Returns((string?)null);
        emptyConfig.Setup(x => x["JWT:Audience"]).Returns((string?)null);
        emptyConfig.Setup(x => x["JWT:ExpirationMinutes"]).Returns((string?)null);

        // Act
        var service = new JwtTokenService(emptyConfig.Object);
        var token = service.GenerateToken("user", "test@example.com", new List<string>());

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        jsonToken.Issuer.Should().Be("StudyBridge");
        jsonToken.Audiences.Should().Contain("StudyBridge-Users");
        // Default expiration is 1440 minutes (24 hours)
        jsonToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(1440), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateToken_WithMultipleRoles_ShouldIncludeAllRoles()
    {
        // Arrange
        const string userId = "test-user-id";
        const string email = "test@example.com";
        var roles = new List<string> { "User", "Admin", "SuperAdmin", "ContentManager" };

        // Act
        var token = _sut.GenerateToken(userId, email, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var roleClaims = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(4);
        roleClaims.Select(c => c.Value).Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void ValidateToken_WithTokenFromDifferentService_ShouldReturnFalse()
    {
        // Arrange
        // Create a different service with different secret
        var differentConfig = new Mock<IConfiguration>();
        differentConfig.Setup(x => x["JWT:SecretKey"]).Returns("different-secret-key-that-is-also-32-characters-long");
        differentConfig.Setup(x => x["JWT:Issuer"]).Returns(TestIssuer);
        differentConfig.Setup(x => x["JWT:Audience"]).Returns(TestAudience);
        differentConfig.Setup(x => x["JWT:ExpirationMinutes"]).Returns("60");
        
        var differentService = new JwtTokenService(differentConfig.Object);
        var tokenFromDifferentService = differentService.GenerateToken("user", "test@example.com", new List<string>());

        // Act
        var result = _sut.ValidateToken(tokenFromDifferentService);

        // Assert
        result.Should().BeFalse();
    }
}
