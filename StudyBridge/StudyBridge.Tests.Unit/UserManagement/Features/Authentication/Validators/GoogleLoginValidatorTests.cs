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
            IdToken = idToken
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.IdToken));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Google ID token is required");
    }

    [Fact]
    public void Validate_WithValidIdToken_ShouldBeValid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJhY2NvdW50cy5nb29nbGUuY29tIiwiYXVkIjoieW91ci1jbGllbnQtaWQuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJqb2huLmRvZUBleGFtcGxlLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlLCJpYXQiOjE1MTYyMzkwMjIsImV4cCI6MTUxNjI0MjYyMn0"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithShortIdToken_ShouldBeValid()
    {
        // Arrange - Note: Validator only checks for null/empty, not format or length
        var command = new GoogleLogin.Command
        {
            IdToken = "short-token"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue("Validator should only check for required field, not format");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithVeryLongIdToken_ShouldBeValid()
    {
        // Arrange
        var longToken = new string('a', 2000); // Very long token
        var command = new GoogleLogin.Command
        {
            IdToken = longToken
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInToken_ShouldBeValid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "token.with-special_characters+and=numbers123/symbols!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithJwtFormatToken_ShouldBeValid()
    {
        // Arrange - Typical JWT format with header.payload.signature
        var command = new GoogleLogin.Command
        {
            IdToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.EkN-DOsnsuRjRO6BxXemmJDm3HbxrbRzXglbN2S4sOkopdU4IsDxTI8jO19W_A4K8ZPJijNLis4EZsHeY559a4DFOd50_OqgHs3PheClF_N6V7i5Lf5YH6K6o_wNmQR3Ug5qK7QoGvKaYlvZjKgFVYN2cK7Mk-gU7QJp4qHjA"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("token")]
    [InlineData("1")]
    [InlineData("a")]
    [InlineData("!@#$%^&*()")]
    public void Validate_WithAnyNonEmptyToken_ShouldBeValid(string token)
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = token
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue($"Token '{token}' should be valid as long as it's not empty");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithWhitespaceOnlyToken_ShouldBeInvalid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "   " // Only whitespace
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.IdToken));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Google ID token is required");
    }

    [Fact]
    public void Validate_WithTabAndNewlineToken_ShouldBeInvalid()
    {
        // Arrange
        var command = new GoogleLogin.Command
        {
            IdToken = "\t\n\r" // Tab, newline, carriage return
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GoogleLogin.Command.IdToken));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Google ID token is required");
    }
}
