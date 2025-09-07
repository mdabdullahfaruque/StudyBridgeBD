using Moq;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Tests.Unit.TestData;
using FluentAssertions;

namespace StudyBridge.Tests.Unit.Services;

public class PermissionServiceTests
{
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IUserRoleRepository> _mockUserRoleRepository;
    private readonly Mock<IRolePermissionRepository> _mockRolePermissionRepository;
    private readonly Mock<ILogger<PermissionService>> _mockLogger;
    private readonly PermissionService _sut;

    public PermissionServiceTests()
    {
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUserRoleRepository = new Mock<IUserRoleRepository>();
        _mockRolePermissionRepository = new Mock<IRolePermissionRepository>();
        _mockLogger = new Mock<ILogger<PermissionService>>();
        
        _sut = new PermissionService(
            _mockRoleRepository.Object,
            _mockUserRoleRepository.Object,
            _mockRolePermissionRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserHasPermission_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const Permission permission = Permission.ViewUsers;

        var userRoles = new List<UserRole> { TestDataBuilder.UserRoles.AdminUserRole(userId) };
        var rolePermissions = new List<RolePermission> { TestDataBuilder.RolePermissions.AdminViewUsers() };

        _mockUserRoleRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        _mockRolePermissionRepository.Setup(x => x.GetRolePermissionsAsync(TestDataBuilder.AdminRoleId))
            .ReturnsAsync(rolePermissions);

        _mockRoleRepository.Setup(x => x.GetByIdAsync(TestDataBuilder.AdminRoleId))
            .ReturnsAsync(TestDataBuilder.Roles.Admin());

        // Act
        var result = await _sut.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserDoesNotHavePermission_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const Permission permission = Permission.DeleteUsers;

        var userRoles = new List<UserRole> { TestDataBuilder.UserRoles.UserUserRole(userId) };
        var rolePermissions = new List<RolePermission> { TestDataBuilder.RolePermissions.UserViewContent() };

        _mockUserRoleRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        _mockRolePermissionRepository.Setup(x => x.GetRolePermissionsAsync(TestDataBuilder.UserRoleId))
            .ReturnsAsync(rolePermissions);

        _mockRoleRepository.Setup(x => x.GetByIdAsync(TestDataBuilder.UserRoleId))
            .ReturnsAsync(TestDataBuilder.Roles.User());

        // Act
        var result = await _sut.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenRoleExists_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const string assignedBy = "admin-user-id";
        const SystemRole role = SystemRole.Admin;

        var roleEntity = TestDataBuilder.Roles.Admin();

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync(roleEntity);

        _mockUserRoleRepository.Setup(x => x.GetUserRoleAsync(userId, roleEntity.Id))
            .ReturnsAsync((UserRole?)null);

        _mockUserRoleRepository.Setup(x => x.AddAsync(It.IsAny<UserRole>()))
            .ReturnsAsync((UserRole ur) => ur);

        // Act
        var result = await _sut.AssignRoleToUserAsync(userId, role, assignedBy);

        // Assert
        result.Should().BeTrue();
        _mockUserRoleRepository.Verify(x => x.AddAsync(It.Is<UserRole>(ur => 
            ur.UserId == userId && 
            ur.RoleId == roleEntity.Id && 
            ur.AssignedBy == assignedBy &&
            ur.IsActive)), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenRoleDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const string assignedBy = "admin-user-id";
        const SystemRole role = SystemRole.Admin;

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _sut.AssignRoleToUserAsync(userId, role, assignedBy);

        // Assert
        result.Should().BeFalse();
        _mockUserRoleRepository.Verify(x => x.AddAsync(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task GetUserRolesAsync_WhenUserHasRoles_ShouldReturnRoles()
    {
        // Arrange
        const string userId = "test-user-id";
        var userRoles = new List<UserRole> { TestDataBuilder.UserRoles.AdminUserRole(userId) };

        _mockUserRoleRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        _mockRoleRepository.Setup(x => x.GetByIdAsync(TestDataBuilder.AdminRoleId))
            .ReturnsAsync(TestDataBuilder.Roles.Admin());

        // Act
        var result = await _sut.GetUserRolesAsync(userId);

        // Assert
        result.Should().Contain(SystemRole.Admin);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WhenUserHasRole_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const SystemRole role = SystemRole.Admin;

        var roleEntity = TestDataBuilder.Roles.Admin();
        var userRole = TestDataBuilder.UserRoles.AdminUserRole(userId);

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync(roleEntity);

        _mockUserRoleRepository.Setup(x => x.GetUserRoleAsync(userId, roleEntity.Id))
            .ReturnsAsync(userRole);

        // Act
        var result = await _sut.RemoveRoleFromUserAsync(userId, role);

        // Assert
        result.Should().BeTrue();
        _mockUserRoleRepository.Verify(x => x.UpdateAsync(It.Is<UserRole>(ur => !ur.IsActive)), Times.Once);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WhenUserDoesNotHaveRole_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const SystemRole role = SystemRole.Admin;

        var roleEntity = TestDataBuilder.Roles.Admin();

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync(roleEntity);

        _mockUserRoleRepository.Setup(x => x.GetUserRoleAsync(userId, roleEntity.Id))
            .ReturnsAsync((UserRole?)null);

        // Act
        var result = await _sut.RemoveRoleFromUserAsync(userId, role);

        // Assert
        result.Should().BeFalse();
        _mockUserRoleRepository.Verify(x => x.UpdateAsync(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WhenRoleDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const SystemRole role = SystemRole.Admin;

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _sut.RemoveRoleFromUserAsync(userId, role);

        // Assert
        result.Should().BeFalse();
        _mockUserRoleRepository.Verify(x => x.UpdateAsync(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task CreateRoleAsync_WhenRoleDoesNotExist_ShouldReturnTrue()
    {
        // Arrange
        const string roleName = "Test Role";
        const SystemRole systemRole = SystemRole.Admin;
        var permissions = new List<Permission> { Permission.ViewUsers, Permission.CreateUsers };

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(systemRole))
            .ReturnsAsync((Role?)null);

        var createdRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            SystemRole = systemRole
        };

        _mockRoleRepository.Setup(x => x.AddAsync(It.IsAny<Role>()))
            .ReturnsAsync(createdRole);

        _mockRolePermissionRepository.Setup(x => x.AddAsync(It.IsAny<RolePermission>()))
            .ReturnsAsync((RolePermission rp) => rp);

        // Act
        var result = await _sut.CreateRoleAsync(roleName, systemRole, permissions);

        // Assert
        result.Should().BeTrue();
        _mockRoleRepository.Verify(x => x.AddAsync(It.Is<Role>(r => 
            r.Name == roleName && r.SystemRole == systemRole)), Times.Once);
        _mockRolePermissionRepository.Verify(x => x.AddAsync(It.IsAny<RolePermission>()), Times.Exactly(permissions.Count));
    }

    [Fact]
    public async Task CreateRoleAsync_WhenRoleAlreadyExists_ShouldReturnFalse()
    {
        // Arrange
        const string roleName = "Test Role";
        const SystemRole systemRole = SystemRole.Admin;
        var permissions = new List<Permission> { Permission.ViewUsers };

        var existingRole = TestDataBuilder.Roles.Admin();

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(systemRole))
            .ReturnsAsync(existingRole);

        // Act
        var result = await _sut.CreateRoleAsync(roleName, systemRole, permissions);

        // Assert
        result.Should().BeFalse();
        _mockRoleRepository.Verify(x => x.AddAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_WhenRoleExists_ShouldReturnTrue()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissions = new List<Permission> { Permission.ViewUsers, Permission.EditUsers };

        _mockRolePermissionRepository.Setup(x => x.DeleteByRoleIdAsync(roleId))
            .Returns(Task.CompletedTask);

        _mockRolePermissionRepository.Setup(x => x.AddAsync(It.IsAny<RolePermission>()))
            .ReturnsAsync((RolePermission rp) => rp);

        // Act
        var result = await _sut.UpdateRolePermissionsAsync(roleId, permissions);

        // Assert
        result.Should().BeTrue();
        _mockRolePermissionRepository.Verify(x => x.DeleteByRoleIdAsync(roleId), Times.Once);
        _mockRolePermissionRepository.Verify(x => x.AddAsync(It.IsAny<RolePermission>()), Times.Exactly(permissions.Count));
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WhenUserHasMultipleRoles_ShouldReturnAllPermissions()
    {
        // Arrange
        const string userId = "test-user-id";
        var userRoles = new List<UserRole> 
        { 
            TestDataBuilder.UserRoles.AdminUserRole(userId),
            TestDataBuilder.UserRoles.UserUserRole(userId)
        };
        var adminPermissions = new List<RolePermission> { TestDataBuilder.RolePermissions.AdminViewUsers() };
        var userPermissions = new List<RolePermission> { TestDataBuilder.RolePermissions.UserViewContent() };

        _mockUserRoleRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ReturnsAsync(userRoles);

        _mockRolePermissionRepository.Setup(x => x.GetRolePermissionsAsync(TestDataBuilder.AdminRoleId))
            .ReturnsAsync(adminPermissions);

        _mockRolePermissionRepository.Setup(x => x.GetRolePermissionsAsync(TestDataBuilder.UserRoleId))
            .ReturnsAsync(userPermissions);

        _mockRoleRepository.Setup(x => x.GetByIdAsync(TestDataBuilder.AdminRoleId))
            .ReturnsAsync(TestDataBuilder.Roles.Admin());

        _mockRoleRepository.Setup(x => x.GetByIdAsync(TestDataBuilder.UserRoleId))
            .ReturnsAsync(TestDataBuilder.Roles.User());

        // Act
        var result = await _sut.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().Contain(Permission.ViewUsers);
        result.Should().Contain(Permission.ViewContent);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WhenExceptionOccurs_ShouldReturnEmptyCollection()
    {
        // Arrange
        const string userId = "test-user-id";

        _mockUserRoleRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const Permission permission = Permission.ViewUsers;

        _mockUserRoleRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.HasPermissionAsync(userId, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserRolesAsync_WhenExceptionOccurs_ShouldReturnEmptyCollection()
    {
        // Arrange
        const string userId = "test-user-id";

        _mockUserRoleRepository.Setup(x => x.GetUserRolesAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.GetUserRolesAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "test-user-id";
        const string assignedBy = "admin-user-id";
        const SystemRole role = SystemRole.Admin;

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.AssignRoleToUserAsync(userId, role, assignedBy);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenUserAlreadyHasActiveRole_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "test-user-id";
        const string assignedBy = "admin-user-id";
        const SystemRole role = SystemRole.Admin;

        var roleEntity = TestDataBuilder.Roles.Admin();
        var existingUserRole = TestDataBuilder.UserRoles.AdminUserRole(userId);
        existingUserRole.IsActive = true;

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync(roleEntity);

        _mockUserRoleRepository.Setup(x => x.GetUserRoleAsync(userId, roleEntity.Id))
            .ReturnsAsync(existingUserRole);

        // Act
        var result = await _sut.AssignRoleToUserAsync(userId, role, assignedBy);

        // Assert
        result.Should().BeTrue();
        _mockUserRoleRepository.Verify(x => x.AddAsync(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenUserHasInactiveRole_ShouldReactivateRole()
    {
        // Arrange
        const string userId = "test-user-id";
        const string assignedBy = "admin-user-id";
        const SystemRole role = SystemRole.Admin;

        var roleEntity = TestDataBuilder.Roles.Admin();
        var existingUserRole = TestDataBuilder.UserRoles.AdminUserRole(userId);
        existingUserRole.IsActive = false;

        _mockRoleRepository.Setup(x => x.GetBySystemRoleAsync(role))
            .ReturnsAsync(roleEntity);

        _mockUserRoleRepository.Setup(x => x.GetUserRoleAsync(userId, roleEntity.Id))
            .ReturnsAsync(existingUserRole);

        // Act
        var result = await _sut.AssignRoleToUserAsync(userId, role, assignedBy);

        // Assert
        result.Should().BeTrue();
        existingUserRole.IsActive.Should().BeTrue();
        existingUserRole.AssignedBy.Should().Be(assignedBy);
        _mockUserRoleRepository.Verify(x => x.UpdateAsync(existingUserRole), Times.Once);
        _mockUserRoleRepository.Verify(x => x.AddAsync(It.IsAny<UserRole>()), Times.Never);
    }
}
