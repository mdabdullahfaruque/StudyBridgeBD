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
            UserId = Guid.NewGuid()
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldBeInvalid()
    {
        // Arrange
        var query = new GetProfile.Query
        {
            UserId = Guid.Empty
        };        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetProfile.Query.UserId));
        result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
    }

    [Fact]
    public void Validate_WithValidGuid_ShouldBeValid()
    {
        // Arrange
        var query = new GetProfile.Query
        {
            UserId = new Guid("12345678-1234-1234-1234-123456789012")
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
