using FluentAssertions;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.Tests.Unit.UserManagement.Features.UserProfile.Validators;

public class GetProfileValidatorTests
{
    private readonly GetProfile.Validator _sut;

    public GetProfileValidatorTests()
    {
        _sut = new GetProfile.Validator();
    }

    [Fact]
    public void Validate_WithValidUserId_ShouldBeValid()
    {
        // Arrange
        var query = new GetProfile.Query
        {
            UserId = Guid.NewGuid().ToString()
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullUserId_ShouldBeInvalid(string? userId)
    {
        // Arrange
        var query = new GetProfile.Query
        {
            UserId = userId!
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetProfile.Query.UserId));
        result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
    }

    [Fact]
    public void Validate_WithValidGuidString_ShouldBeValid()
    {
        // Arrange
        var query = new GetProfile.Query
        {
            UserId = "12345678-1234-1234-1234-123456789012"
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
