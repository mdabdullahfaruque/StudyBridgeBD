using FluentAssertions;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Authentication.Validators;

public class GoogleLoginValidatorTests
{
    private readonly GoogleLogin.Validator _sut;

    public GoogleLoginValidatorTests()
    {
        _sut = new GoogleLogin.Validator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidGoogleLoginCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyIdToken_ShouldBeInvalid(string idToken)
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = idToken,
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            GoogleSub = "12345678901234567890"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.IdToken));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Google ID token is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyEmail_ShouldBeInvalid(string email)
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "valid_token",
            Email = email,
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            GoogleSub = "12345678901234567890"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldBeInvalid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "valid_token",
            Email = "invalid-email",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            GoogleSub = "12345678901234567890"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Valid email is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyDisplayName_ShouldBeInvalid(string displayName)
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "valid_token",
            Email = "test@example.com",
            DisplayName = displayName,
            FirstName = "Test",
            LastName = "User",
            GoogleSub = "12345678901234567890"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.DisplayName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyGoogleSub_ShouldBeInvalid(string googleSub)
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "valid_token",
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            GoogleSub = googleSub
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.GoogleSub));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Google subject (sub) is required");
    }

    [Fact]
    public void Validate_WithValidEmailFormats_ShouldBeValid()
    {
        // Arrange & Act & Assert
        var validEmails = new[]
        {
            "user@example.com",
            "user.name@example.com",
            "user+tag@example.co.uk",
            "user123@test-domain.org"
        };

        foreach (var email in validEmails)
        {
            var command = new GoogleLogin.Command
            {
                IdToken = "valid_token",
                Email = email,
                DisplayName = "Test User",
                FirstName = "Test",
                LastName = "User",
                GoogleSub = "12345678901234567890"
            };

            var result = _sut.Validate(command);

            result.IsValid.Should().BeTrue($"Email '{email}' should be valid");
        }
    }

    [Fact]
    public void Validate_WithWhitespaceOnlyToken_ShouldBeInvalid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "   ", // Only whitespace
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            GoogleSub = "12345678901234567890"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.IdToken));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Google ID token is required");
    }

    [Fact]
    public void Validate_WithAllRequiredFields_ShouldBeValid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "valid_token_123",
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            GoogleSub = "12345678901234567890",
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithOptionalFieldsEmpty_ShouldBeValid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "valid_token_123",
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "", // Optional
            LastName = "", // Optional
            GoogleSub = "12345678901234567890",
            AvatarUrl = null // Optional
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
