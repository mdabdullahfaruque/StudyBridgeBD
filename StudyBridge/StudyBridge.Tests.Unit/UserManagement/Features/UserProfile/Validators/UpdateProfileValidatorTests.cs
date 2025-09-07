using FluentAssertions;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.Tests.Unit.UserManagement.Features.UserProfile.Validators;

public class UpdateProfileValidatorTests
{
    private readonly UpdateProfile.Validator _sut;

    public UpdateProfileValidatorTests()
    {
        _sut = new UpdateProfile.Validator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldBeValid()
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "Valid Display Name",
            FirstName = "Valid",
            LastName = "Name",
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullDisplayName_ShouldBeInvalid(string? displayName)
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = displayName!,
            FirstName = "Valid",
            LastName = "Name"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfile.Request.DisplayName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Display name is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullFirstName_ShouldBeInvalid(string? firstName)
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "Valid Display Name",
            FirstName = firstName!,
            LastName = "Valid"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfile.Request.FirstName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullLastName_ShouldBeInvalid(string? lastName)
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "Valid Display Name",
            FirstName = "Valid",
            LastName = lastName!
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfile.Request.LastName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required");
    }

    [Fact]
    public void Validate_WithNullAvatarUrl_ShouldBeValid()
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "Valid Display Name",
            FirstName = "Valid",
            LastName = "Name",
            AvatarUrl = null
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyAvatarUrl_ShouldBeValid()
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "Valid Display Name",
            FirstName = "Valid",
            LastName = "Name",
            AvatarUrl = ""
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("https://example.com/avatar.jpg")]
    [InlineData("http://example.com/image.png")]
    [InlineData("https://cdn.example.com/user/123/profile.gif")]
    public void Validate_WithValidAvatarUrls_ShouldBeValid(string avatarUrl)
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "Valid Display Name",
            FirstName = "Valid",
            LastName = "Name",
            AvatarUrl = avatarUrl
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "",
            FirstName = "",
            LastName = "",
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfile.Request.DisplayName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfile.Request.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfile.Request.LastName));
    }

    [Fact]
    public void Validate_WithLongValidStrings_ShouldBeValid()
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = new string('A', 250),
            FirstName = new string('B', 100),
            LastName = new string('C', 100),
            AvatarUrl = "https://example.com/" + new string('d', 200) + ".jpg"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("ABC")]
    public void Validate_WithShortValidNames_ShouldBeValid(string name)
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = name,
            FirstName = name,
            LastName = name,
            AvatarUrl = null
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInNames_ShouldBeValid()
    {
        // Arrange
        var request = new UpdateProfile.Request
        {
            DisplayName = "José María O'Connor-Smith",
            FirstName = "José",
            LastName = "O'Connor-Smith",
            AvatarUrl = "https://example.com/avatar.jpg"
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
