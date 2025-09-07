using FluentAssertions;
using StudyBridge.Infrastructure.Services;

namespace StudyBridge.Tests.Unit.Infrastructure.Services;

public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _sut;

    public PasswordHashingServiceTests()
    {
        _sut = new PasswordHashingService();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        var hashedPassword = _sut.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Should().StartWith("$2a$12$"); // BCrypt format with work factor 12
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldReturnHash()
    {
        // Arrange
        const string password = "";

        // Act
        var hashedPassword = _sut.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$12$");
    }

    [Fact]
    public void HashPassword_WithLongPassword_ShouldReturnHash()
    {
        // Arrange
        var password = new string('A', 1000); // Very long password

        // Act
        var hashedPassword = _sut.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$12$");
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_ShouldReturnHash()
    {
        // Arrange
        const string password = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        var hashedPassword = _sut.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$12$");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "TestPassword123!";
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string wrongPassword = "WrongPassword456!";
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithCaseSensitivePassword_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string wrongCasePassword = "testpassword123!";
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(wrongCasePassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldWorkCorrectly()
    {
        // Arrange
        const string password = "";
        var hashedPassword = _sut.HashPassword(password);

        // Act
        var correctResult = _sut.VerifyPassword(password, hashedPassword);
        var incorrectResult = _sut.VerifyPassword("not empty", hashedPassword);

        // Assert
        correctResult.Should().BeTrue();
        incorrectResult.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string invalidHash = "invalid_hash_format";

        // Act
        var result = _sut.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        string? nullHash = null;

        // Act
        var result = _sut.VerifyPassword(password, nullHash!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string emptyHash = "";

        // Act
        var result = _sut.VerifyPassword(password, emptyHash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("Complex1Password!")]
    [InlineData("12345")]
    [InlineData("   spaces   ")]
    [InlineData("Unicode日本語パスワード")]
    [InlineData("emoji🔒🔑")]
    public void HashAndVerifyPassword_WithVariousPasswords_ShouldWorkCorrectly(string password)
    {
        // Act
        var hashedPassword = _sut.HashPassword(password);
        var verificationResult = _sut.VerifyPassword(password, hashedPassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        verificationResult.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_Performance_ShouldCompleteInReasonableTime()
    {
        // Arrange
        const string password = "TestPassword123!";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var hashedPassword = _sut.HashPassword(password);
        stopwatch.Stop();

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public void VerifyPassword_Performance_ShouldCompleteInReasonableTime()
    {
        // Arrange
        const string password = "TestPassword123!";
        var hashedPassword = _sut.HashPassword(password);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _sut.VerifyPassword(password, hashedPassword);
        stopwatch.Stop();

        // Assert
        result.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
    }
}
