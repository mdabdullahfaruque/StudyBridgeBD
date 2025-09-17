using FluentAssertions;
using StudyBridge.UserManagement.Features.Admin;

namespace StudyBridge.Tests.Unit.UserManagement.Features.Admin.Validators;

public class GetUsersValidatorTests
{
    private readonly GetUsers.Validator _sut;

    public GetUsersValidatorTests()
    {
        _sut = new GetUsers.Validator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_Have_Error_When_PageNumber_Is_Invalid(int pageNumber)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUsers.Query.PageNumber));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Should_Be_Valid_When_PageNumber_Is_Valid(int pageNumber)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000)]
    public void Should_Have_Error_When_PageSize_Is_Invalid(int pageSize)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUsers.Query.PageSize));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_Be_Valid_When_PageSize_Is_Valid(int pageSize)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.PageSize));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("INVALID")]
    [InlineData("up")]
    [InlineData("down")]
    [InlineData("ascending")]
    [InlineData("descending")]
    public void Should_Have_Error_When_SortDirection_Is_Invalid(string sortDirection)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            SortDirection = sortDirection
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUsers.Query.SortDirection));
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    [InlineData("ASC")]
    [InlineData("DESC")]
    [InlineData("Asc")]
    [InlineData("Desc")]
    public void Should_Be_Valid_When_SortDirection_Is_Valid(string sortDirection)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            SortDirection = sortDirection
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.SortDirection));
    }

    [Fact]
    public void Should_Be_Valid_When_SortDirection_Is_Null()
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            SortDirection = null
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.SortDirection));
    }

    [Fact]
    public void Should_Be_Valid_When_Optional_Fields_Are_Null()
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = null,
            Role = null,
            IsActive = null,
            SortBy = null,
            SortDirection = null
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Be_Valid_When_All_Fields_Are_Valid()
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 2,
            PageSize = 25,
            SearchTerm = "john@example.com",
            Role = "Admin",
            IsActive = true,
            SortBy = "Email",
            SortDirection = "asc"
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Fields_Invalid()
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 0, // Invalid
            PageSize = 200, // Invalid
            SortDirection = "invalid" // Invalid
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUsers.Query.PageNumber));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUsers.Query.PageSize));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetUsers.Query.SortDirection));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("john")]
    [InlineData("admin@example.com")]
    [InlineData("Test User Name")]
    public void Should_Be_Valid_When_SearchTerm_Has_Any_Value(string searchTerm)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.SearchTerm));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    [InlineData("Manager")]
    [InlineData("")]
    public void Should_Be_Valid_When_Role_Has_Any_Value(string role)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            Role = role
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.Role));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Be_Valid_When_IsActive_Has_Boolean_Value(bool isActive)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            IsActive = isActive
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.IsActive));
    }

    [Theory]
    [InlineData("Email")]
    [InlineData("DisplayName")]
    [InlineData("CreatedAt")]
    [InlineData("")]
    public void Should_Be_Valid_When_SortBy_Has_Any_Value(string sortBy)
    {
        // Arrange
        var query = new GetUsers.Query
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = sortBy
        };

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GetUsers.Query.SortBy));
    }
}