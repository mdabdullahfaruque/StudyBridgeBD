using FluentAssertions;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Authentication.Validators;

public class ChangePasswordValidatorTests
{
    private readonly ChangePassword.Validator _sut;

    public ChangePasswordValidatorTests()
    {
        _sut = new ChangePassword.Validator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidChangePasswordCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyCurrentPassword_ShouldBeInvalid(string currentPassword)
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = currentPassword,
            NewPassword = "NewValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePassword.Command.CurrentPassword));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Current password is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyNewPassword_ShouldBeInvalid(string newPassword)
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = newPassword
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePassword.Command.NewPassword));
        result.Errors.Should().Contain(e => e.ErrorMessage == "New password is required");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("weak")]
    [InlineData("a")]
    [InlineData("short")]
    public void Validate_WithShortNewPassword_ShouldBeInvalid(string newPassword)
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = newPassword
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePassword.Command.NewPassword));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 6 characters long");
    }

    [Fact]
    public void Validate_WithSameCurrentAndNewPassword_ShouldBeValid()
    {
        // Arrange - Note: The current validator doesn't check for password difference
        var samePassword = "SamePassword123!";
        var command = new ChangePassword.Command
        {
            CurrentPassword = samePassword,
            NewPassword = samePassword
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue("Current validator doesn't enforce password difference");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "",
            NewPassword = "weak"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
        
        // Should have errors for all invalid fields
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePassword.Command.CurrentPassword));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePassword.Command.NewPassword));
    }

    [Fact]
    public void Validate_WithValidLongPasswords_ShouldBeValid()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "VeryLongCurrentPasswordThatShouldStillBeValid123!@#",
            NewPassword = "VeryLongNewPasswordThatShouldStillBeValid456$%^"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMinimumValidPasswordLength_ShouldBeValid()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "123456", // Exactly 6 characters
            NewPassword = "654321"     // Exactly 6 characters, different from current
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithCaseSensitivePasswordComparison_ShouldBeValid()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "Password123!",
            NewPassword = "password123!" // Different case
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue("Password comparison should be case-sensitive");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInPasswords_ShouldBeValid()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "Current!@#$%^&*()123",
            NewPassword = "New!@#$%^&*()456"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithUnicodeCharactersInPasswords_ShouldBeValid()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "пароль123", // Cyrillic characters
            NewPassword = "пароль456"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyBothPasswords_ShouldReturnBothErrors()
    {
        // Arrange
        var command = new ChangePassword.Command
        {
            CurrentPassword = "",
            NewPassword = ""
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePassword.Command.CurrentPassword));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePassword.Command.NewPassword));
    }
}
