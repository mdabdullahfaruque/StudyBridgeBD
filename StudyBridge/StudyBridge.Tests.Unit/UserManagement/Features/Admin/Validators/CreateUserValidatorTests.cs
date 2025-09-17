using FluentAssertions;
using StudyBridge.UserManagement.Features.Admin;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Admin.Validators;

public class CreateUserValidatorTests
{
    private readonly CreateUser.Validator _sut;

    public CreateUserValidatorTests()
    {
        _sut = new CreateUser.Validator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user.domain.com")]
    public void Should_Have_Error_When_Email_Is_Invalid(string email)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = email,
            DisplayName = "Test User",
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.Email));
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("first.last@subdomain.example.org")]
    public void Should_Be_Valid_When_Email_Is_Valid(string email)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = email,
            DisplayName = "Test User",
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateUser.Command.Email));
    }

    [Fact]
    public void Should_Have_Error_When_Email_Exceeds_Max_Length()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // 262 characters total
        var command = new CreateUser.Command
        {
            Email = longEmail,
            DisplayName = "Test User",
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void Should_Have_Error_When_DisplayName_Is_Invalid(string displayName)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = displayName,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.DisplayName));
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("Valid Display Name")]
    [InlineData("John Doe")]
    public void Should_Be_Valid_When_DisplayName_Is_Valid(string displayName)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = displayName,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateUser.Command.DisplayName));
    }

    [Fact]
    public void Should_Have_Error_When_DisplayName_Exceeds_Max_Length()
    {
        // Arrange
        var longDisplayName = new string('A', 101); // 101 characters
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = longDisplayName,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.DisplayName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("nouppercaseletter123!")]
    [InlineData("NOLOWERCASELETTER123!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChars123")]
    public void Should_Have_Error_When_Password_Is_Invalid(string password)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            Password = password
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.Password));
    }

    [Theory]
    [InlineData("ValidPassword123!")]
    [InlineData("Complex@Pass1")]
    [InlineData("MySecure&Pass9")]
    public void Should_Be_Valid_When_Password_Is_Valid(string password)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            Password = password
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateUser.Command.Password));
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Exceeds_Max_Length()
    {
        // Arrange
        var longFirstName = new string('A', 51); // 51 characters
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = longFirstName,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.FirstName));
    }

    [Theory]
    [InlineData("John")]
    [InlineData("A")]
    [InlineData("Very-Long-But-Valid-First-Name-Under-Fifty-Chars")]
    public void Should_Be_Valid_When_FirstName_Is_Valid(string firstName)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = firstName,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateUser.Command.FirstName));
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Exceeds_Max_Length()
    {
        // Arrange
        var longLastName = new string('A', 51); // 51 characters
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            LastName = longLastName,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.LastName));
    }

    [Theory]
    [InlineData("Doe")]
    [InlineData("A")]
    [InlineData("Very-Long-But-Valid-Last-Name-Under-Fifty-Chars")]
    public void Should_Be_Valid_When_LastName_Is_Valid(string lastName)
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            LastName = lastName,
            Password = "ValidPassword123!"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateUser.Command.LastName));
    }

    [Fact]
    public void Should_Be_Valid_When_Optional_Fields_Are_Null()
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = null,
            LastName = null,
            Password = "ValidPassword123!",
            Roles = new List<string>()
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Be_Valid_When_Roles_Is_Empty_List()
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            Password = "ValidPassword123!",
            Roles = new List<string>()
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateUser.Command.Roles));
    }

    [Fact]
    public void Should_Be_Valid_When_All_Required_Fields_Are_Valid()
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            FirstName = "Test",
            LastName = "User",
            Password = "ValidPassword123!",
            Roles = new List<string> { "User", "Admin" }
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Fields_Invalid()
    {
        // Arrange
        var command = new CreateUser.Command
        {
            Email = "", // Invalid
            DisplayName = "", // Invalid
            Password = "weak", // Invalid
            FirstName = new string('A', 51), // Invalid - too long
            LastName = new string('B', 51) // Invalid - too long
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.DisplayName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.Password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUser.Command.LastName));
    }
}