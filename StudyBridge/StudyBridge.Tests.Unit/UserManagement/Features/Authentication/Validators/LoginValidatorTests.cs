using FluentAssertions;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Authentication.Validators;

public class LoginValidatorTests
{
    private readonly Login.Validator _sut;

    public LoginValidatorTests()
    {
        _sut = new Login.Validator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidLoginCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidEmail_ShouldBeInvalid(string email)
    {
        // Arrange
        var command = new Login.Command
        {
            Email = email,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Login.Command.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user.domain.com")]
    public void Validate_WithInvalidEmailFormat_ShouldBeInvalid(string email)
    {
        // Arrange
        var command = new Login.Command
        {
            Email = email,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Login.Command.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Please enter a valid email address");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyPassword_ShouldBeInvalid(string password)
    {
        // Arrange
        var command = new Login.Command
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Login.Command.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new Login.Command
        {
            Email = "invalid-email",
            Password = ""
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3); // Email format + Password required + Password cannot be empty
        
        // Should have errors for all invalid fields
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Login.Command.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Login.Command.Password));
    }

    [Fact]
    public void Validate_WithValidInputs_ShouldAcceptAnyValidEmail()
    {
        // Arrange
        var validEmails = new[]
        {
            "test@example.com",
            "user.name@domain.co.uk",
            "firstname.lastname@subdomain.domain.com",
            "test123@test123.test",
            "test+tag@example.com"
        };

        foreach (var email in validEmails)
        {
            var command = new Login.Command
            {
                Email = email,
                Password = "ValidPassword"
            };

            // Act
            var result = _sut.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue($"Email {email} should be valid");
            result.Errors.Should().BeEmpty($"Email {email} should not have errors");
        }
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldStillBeValid()
    {
        // Arrange - Note: Login validator doesn't enforce password length like Register validator
        var command = new Login.Command
        {
            Email = "test@example.com",
            Password = "a"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue("Login validator should not enforce password length");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithLongCredentials_ShouldBeValid()
    {
        // Arrange
        var command = new Login.Command
        {
            Email = "very.long.email.address.that.should.still.be.valid@very-long-domain-name.com",
            Password = "VeryLongPasswordThatShouldStillBeValidForLoginPurposes"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM")]
    [InlineData("Test@Example.Com")]
    [InlineData("test@EXAMPLE.COM")]
    public void Validate_WithDifferentCaseEmails_ShouldBeValid(string email)
    {
        // Arrange
        var command = new Login.Command
        {
            Email = email,
            Password = "ValidPassword"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
