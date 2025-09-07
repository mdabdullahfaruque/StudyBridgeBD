using FluentAssertions;
using StudyBridge.Tests.Unit.TestData;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Authentication.Validators;

public class RegisterValidatorTests
{
    private readonly Register.Validator _sut;

    public RegisterValidatorTests()
    {
        _sut = new Register.Validator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldBeValid()
    {
        // Arrange
        var command = TestDataBuilder.Commands.Authentication.ValidRegisterCommand();

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
        var command = new Register.Command
        {
            Email = email,
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "John Doe"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldBeInvalid()
    {
        // Arrange
        var command = new Register.Command
        {
            Email = null!,
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "John Doe"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.Email));
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
        var command = new Register.Command
        {
            Email = email,
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "John Doe"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Please enter a valid email address");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyPassword_ShouldBeInvalid(string password)
    {
        // Arrange
        var command = new Register.Command
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "John Doe"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("weak")]
    [InlineData("a")]
    public void Validate_WithShortPassword_ShouldBeInvalid(string password)
    {
        // Arrange
        var command = new Register.Command
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            DisplayName = "John Doe"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 6 characters long");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void Validate_WithInvalidFirstName_ShouldBeInvalid(string firstName)
    {
        // Arrange
        var command = new Register.Command
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            FirstName = firstName,
            LastName = "Doe",
            DisplayName = "John Doe"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.FirstName));
        
        if (string.IsNullOrWhiteSpace(firstName))
        {
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required");
        }
        else
        {
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name must be at least 2 characters long");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("D")]
    public void Validate_WithInvalidLastName_ShouldBeInvalid(string lastName)
    {
        // Arrange
        var command = new Register.Command
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = lastName,
            DisplayName = "John Doe"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.LastName));
        
        if (string.IsNullOrWhiteSpace(lastName))
        {
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required");
        }
        else
        {
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name must be at least 2 characters long");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyDisplayName_ShouldBeInvalid(string displayName)
    {
        // Arrange
        var command = new Register.Command
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe",
            DisplayName = displayName
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.DisplayName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name is required");
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new Register.Command
        {
            Email = "invalid-email",
            Password = "weak",
            FirstName = "",
            LastName = "",
            DisplayName = ""
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
        
        // Should have errors for all invalid fields
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.Password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.LastName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Register.Command.DisplayName));
    }

    [Fact]
    public void Validate_WithValidLongNames_ShouldBeValid()
    {
        // Arrange
        var command = new Register.Command
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            FirstName = "VeryLongFirstNameThatShouldStillBeValid",
            LastName = "VeryLongLastNameThatShouldStillBeValid",
            DisplayName = "Very Long Display Name That Should Still Be Valid"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
