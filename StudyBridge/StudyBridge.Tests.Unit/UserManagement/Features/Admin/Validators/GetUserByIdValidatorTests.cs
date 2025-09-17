using FluentAssertions;
using StudyBridge.UserManagement.Features.Admin;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Admin.Validators;

public class GetUserByIdValidatorTests
{
    private readonly GetUserById.Validator _sut;

    public GetUserByIdValidatorTests()
    {
        _sut = new GetUserById.Validator();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var query = new GetUserById.Query
        {
            UserId = Guid.Empty
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUserById.Query.UserId));
    }

    [Fact]
    public void Should_Be_Valid_When_UserId_Is_Valid_Guid()
    {
        // Arrange
        var query = new GetUserById.Query
        {
            UserId = Guid.NewGuid()
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Be_Valid_When_UserId_Is_Non_Empty_Guid()
    {
        // Arrange
        var validGuid = new Guid("12345678-1234-1234-1234-123456789abc");
        var query = new GetUserById.Query
        {
            UserId = validGuid
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUserById.Query.UserId));
    }
}