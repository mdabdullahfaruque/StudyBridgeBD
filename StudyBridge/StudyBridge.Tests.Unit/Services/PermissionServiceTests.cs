using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using FluentAssertions;

namespace StudyBridge.Tests.Unit.Services;

public class PermissionServiceTests
{
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IUserRoleRepository> _mockUserRoleRepository;
    private readonly Mock<IRolePermissionRepository> _mockRolePermissionRepository;
    private readonly Mock<IMenuRepository> _mockMenuRepository;
    private readonly Mock<IPermissionRepository> _mockPermissionRepository;
    private readonly Mock<ILogger<PermissionService>> _mockLogger;
    private readonly PermissionService _sut;

    public PermissionServiceTests()
    {
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUserRoleRepository = new Mock<IUserRoleRepository>();
        _mockRolePermissionRepository = new Mock<IRolePermissionRepository>();
        _mockMenuRepository = new Mock<IMenuRepository>();
        _mockPermissionRepository = new Mock<IPermissionRepository>();
        _mockLogger = new Mock<ILogger<PermissionService>>();
        
        _sut = new PermissionService(
            _mockRoleRepository.Object,
            _mockUserRoleRepository.Object,
            _mockRolePermissionRepository.Object,
            _mockMenuRepository.Object,
            _mockPermissionRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task HasPermissionAsync_WithPermissionKey_WhenPermissionNotFound_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const string permissionKey = "invalid.permission";

        _mockPermissionRepository
            .Setup(x => x.GetByKeyAsync(permissionKey))
            .ReturnsAsync((Permission?)null);

        // Act
        var result = await _sut.HasPermissionAsync(userId, permissionKey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WithPermissionKey_WhenPermissionExists_ShouldCheckUserPermissions()
    {
        // Arrange
        const string userId = "test-user-id";
        const string permissionKey = "users.view";
        var permission = CreateTestPermission(permissionKey);
        var userPermissions = new List<Permission> { permission };

        _mockPermissionRepository
            .Setup(x => x.GetByKeyAsync(permissionKey))
            .ReturnsAsync(permission);

        _mockPermissionRepository
            .Setup(x => x.GetUserPermissionsAsync(userId))
            .ReturnsAsync(userPermissions);

        // Act
        var result = await _sut.HasPermissionAsync(userId, permissionKey);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ShouldReturnPermissions()
    {
        // Arrange
        const string userId = "test-user-id";
        var permissions = new List<Permission>
        {
            CreateTestPermission("users.view"),
            CreateTestPermission("users.create")
        };

        _mockPermissionRepository
            .Setup(x => x.GetUserPermissionsAsync(userId))
            .ReturnsAsync(permissions);

        // Act
        var result = await _sut.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.PermissionKey == "users.view");
        result.Should().Contain(p => p.PermissionKey == "users.create");
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenRoleExists_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const SystemRole role = SystemRole.Admin;
        const string assignedBy = "system-admin";
        var roleEntity = CreateTestRole(role);

        _mockRoleRepository
            .Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync(roleEntity);

        _mockUserRoleRepository
            .Setup(x => x.GetUserRoleAsync(userId, roleEntity.Id))
            .ReturnsAsync((UserRole?)null);

        _mockUserRoleRepository
            .Setup(x => x.AddAsync(It.IsAny<UserRole>()))
            .ReturnsAsync((UserRole ur) => ur);

        // Act
        var result = await _sut.AssignRoleToUserAsync(userId, role, assignedBy);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserMenusAsync_ShouldReturnMenus()
    {
        // Arrange
        const string userId = "test-user-id";
        var menus = new List<Menu>
        {
            CreateTestMenu("dashboard", "Dashboard"),
            CreateTestMenu("users", "Users")
        };

        _mockMenuRepository
            .Setup(x => x.GetUserMenusAsync(userId))
            .ReturnsAsync(menus);

        // Act
        var result = await _sut.GetUserMenusAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Name == "dashboard");
    }

    [Fact]
    public async Task GetPermissionByKeyAsync_WhenExists_ShouldReturnPermission()
    {
        // Arrange
        const string permissionKey = "users.view";
        var permission = CreateTestPermission(permissionKey);

        _mockPermissionRepository
            .Setup(x => x.GetByKeyAsync(permissionKey))
            .ReturnsAsync(permission);

        // Act
        var result = await _sut.GetPermissionByKeyAsync(permissionKey);

        // Assert
        result.Should().NotBeNull();
        result!.PermissionKey.Should().Be(permissionKey);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenRoleNotFound_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const SystemRole role = SystemRole.Admin;
        const string assignedBy = "system-admin";

        _mockRoleRepository
            .Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _sut.AssignRoleToUserAsync(userId, role, assignedBy);

        // Assert
        result.Should().BeFalse();
    }

    private static Permission CreateTestPermission(string permissionKey)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            PermissionKey = permissionKey,
            DisplayName = permissionKey.Replace(".", " "),
            PermissionType = PermissionType.View,
            MenuId = Guid.NewGuid(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Role CreateTestRole(SystemRole systemRole)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = systemRole.ToString(),
            SystemRole = systemRole,
            Description = $"Test role for {systemRole}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Menu CreateTestMenu(string name, string displayName)
    {
        return new Menu
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            Icon = "fas fa-home",
            Route = $"/{name}",
            MenuType = MenuType.Admin,
            SortOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
