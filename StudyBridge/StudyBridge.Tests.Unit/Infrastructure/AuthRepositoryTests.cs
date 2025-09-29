using Moq;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using StudyBridge.Infrastructure.Data;
using StudyBridge.Infrastructure.Repositories;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Tests.Unit.Infrastructure;

public class RoleRepositoryTests : IDisposable
{
    private readonly RoleRepository _sut;
    private readonly AppDbContext _context;

    public RoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new RoleRepository(_context);
    }

    [Fact]
    public void RoleRepository_ShouldBeInstantiated()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnRole()
    {
        // Arrange
        var role = CreateTestRole(SystemRole.Admin);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.Name.Should().Be(role.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySystemRoleAsync_WithValidSystemRole_ShouldReturnRole()
    {
        // Arrange
        var role = CreateTestRole(SystemRole.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetBySystemRoleAsync(SystemRole.User);

        // Assert
        result.Should().NotBeNull();
        result!.SystemRole.Should().Be(SystemRole.User);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnActiveRolesOnly()
    {
        // Arrange
        var activeRole = CreateTestRole(SystemRole.Admin);
        var inactiveRole = CreateTestRole(SystemRole.User);
        inactiveRole.IsActive = false;
        
        await _context.Roles.AddRangeAsync(activeRole, inactiveRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(r => r.Id == activeRole.Id);
        result.Should().NotContain(r => r.Id == inactiveRole.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldAddRole()
    {
        // Arrange
        var role = CreateTestRole(SystemRole.User);

        // Act
        var result = await _sut.AddAsync(role);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(role.Id);
        
        var savedRole = await _context.Roles.FindAsync(role.Id);
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be(role.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRole()
    {
        // Arrange
        var role = CreateTestRole(SystemRole.Admin);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        
        var originalUpdateTime = role.UpdatedAt;
        role.Description = "Updated description";

        // Act
        await _sut.UpdateAsync(role);

        // Assert
        var updatedRole = await _context.Roles.FindAsync(role.Id);
        updatedRole.Should().NotBeNull();
        updatedRole!.Description.Should().Be("Updated description");
        updatedRole.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteRole()
    {
        // Arrange
        var role = CreateTestRole(SystemRole.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(role.Id);

        // Assert
        var deletedRole = await _context.Roles.FindAsync(role.Id);
        deletedRole.Should().NotBeNull();
        deletedRole!.IsActive.Should().BeFalse();
        deletedRole.UpdatedAt.Should().BeAfter(role.CreatedAt);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _sut.DeleteAsync(Guid.NewGuid());
        await act.Should().NotThrowAsync();
    }

    private Role CreateTestRole(SystemRole systemRole)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = systemRole.ToString(),
            Description = $"Test {systemRole} role",
            SystemRole = systemRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

public class UserRoleRepositoryTests : IDisposable
{
    private readonly UserRoleRepository _sut;
    private readonly AppDbContext _context;

    public UserRoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new UserRoleRepository(_context);
    }

    [Fact]
    public void UserRoleRepository_ShouldBeInstantiated()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnUserRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role1 = CreateTestRole(SystemRole.Admin);
        var role2 = CreateTestRole(SystemRole.User);
        
        await _context.Roles.AddRangeAsync(role1, role2);
        await _context.SaveChangesAsync();

        var userRole1 = CreateTestUserRole(userId, role1.Id);
        var userRole2 = CreateTestUserRole(userId, role2.Id);
        var otherUserRole = CreateTestUserRole(Guid.NewGuid(), role1.Id);
        
        await _context.UserRoles.AddRangeAsync(userRole1, userRole2, otherUserRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserRolesAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(ur => ur.RoleId == role1.Id);
        result.Should().Contain(ur => ur.RoleId == role2.Id);
        result.Should().NotContain(ur => ur.Id == otherUserRole.Id);
    }

    [Fact]
    public async Task GetUserRoleAsync_WithValidUserAndRole_ShouldReturnUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = CreateTestRole(SystemRole.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        var userRole = CreateTestUserRole(userId, role.Id);
        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserRoleAsync(userId, role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public async Task GetUserRoleAsync_WithInvalidUserOrRole_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetUserRoleAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = CreateTestRole(SystemRole.Admin);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        var userRole = CreateTestUserRole(userId, role.Id);

        // Act
        var result = await _sut.AddAsync(userRole);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userRole.Id);
        
        var savedUserRole = await _context.UserRoles.FindAsync(userRole.Id);
        savedUserRole.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = CreateTestRole(SystemRole.User);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        var userRole = CreateTestUserRole(userId, role.Id);
        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();
        
        var originalUpdateTime = userRole.UpdatedAt;
        userRole.IsActive = false;

        // Act
        await _sut.UpdateAsync(userRole);

        // Assert
        var updatedUserRole = await _context.UserRoles.FindAsync(userRole.Id);
        updatedUserRole.Should().NotBeNull();
        updatedUserRole!.IsActive.Should().BeFalse();
        updatedUserRole.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = CreateTestRole("ContentManager");
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        var userRole = CreateTestUserRole(userId, role.Id);
        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(userRole.Id);

        // Assert
        var deletedUserRole = await _context.UserRoles.FindAsync(userRole.Id);
        deletedUserRole.Should().NotBeNull();
        deletedUserRole!.IsActive.Should().BeFalse();
    }

    private Role CreateTestRole(SystemRole systemRole)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = systemRole.ToString(),
            Description = $"Test {systemRole} role",
            SystemRole = systemRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private UserRole CreateTestUserRole(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

public class RolePermissionRepositoryTests : IDisposable
{
    private readonly RolePermissionRepository _sut;
    private readonly AppDbContext _context;

    public RolePermissionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new RolePermissionRepository(_context);
    }

    [Fact]
    public void RolePermissionRepository_ShouldBeInstantiated()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRolePermissionsAsync_ShouldReturnRolePermissions()
    {
        // Arrange
        var role = CreateTestRole(SystemRole.Admin);
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();

        var rolePermission1 = CreateTestRolePermission(role.Id, Guid.NewGuid());
        var rolePermission2 = CreateTestRolePermission(role.Id, Guid.NewGuid());
        var otherRolePermission = CreateTestRolePermission(Guid.NewGuid(), Guid.NewGuid());
        
        await _context.RolePermissions.AddRangeAsync(rolePermission1, rolePermission2, otherRolePermission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetRolePermissionsAsync(role.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(rp => rp.Id == rolePermission1.Id);
        result.Should().Contain(rp => rp.Id == rolePermission2.Id);
        result.Should().NotContain(rp => rp.Id == otherRolePermission.Id);
    }

    [Fact]
    public async Task GetPermissionsByUserIdAsync_ShouldReturnUserPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = CreateTestRole(SystemRole.User);
        var menu = CreateTestMenu("users", "User Management");
        var permission1 = CreateTestPermission("users.view", menu.Id);
        var permission2 = CreateTestPermission("users.edit", menu.Id);
        
        await _context.Roles.AddAsync(role);
        await _context.Menus.AddAsync(menu);
        await _context.Permissions.AddRangeAsync(permission1, permission2);
        await _context.SaveChangesAsync();

        var userRole = CreateTestUserRole(userId, role.Id);
        await _context.UserRoles.AddAsync(userRole);
        
        var rolePermission1 = CreateTestRolePermission(role.Id, permission1.Id, true);
        var rolePermission2 = CreateTestRolePermission(role.Id, permission2.Id, false); // Not granted
        await _context.RolePermissions.AddRangeAsync(rolePermission1, rolePermission2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPermissionsByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == permission1.Id);
        result.Should().NotContain(p => p.Id == permission2.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldAddRolePermission()
    {
        // Arrange
        var rolePermission = CreateTestRolePermission(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _sut.AddAsync(rolePermission);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(rolePermission.Id);
        
        var savedRolePermission = await _context.RolePermissions.FindAsync(rolePermission.Id);
        savedRolePermission.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRolePermission()
    {
        // Arrange
        var rolePermission = CreateTestRolePermission(Guid.NewGuid(), Guid.NewGuid(), false);
        await _context.RolePermissions.AddAsync(rolePermission);
        await _context.SaveChangesAsync();
        
        var originalUpdateTime = rolePermission.UpdatedAt;
        rolePermission.IsGranted = true;

        // Act
        await _sut.UpdateAsync(rolePermission);

        // Assert
        var updatedRolePermission = await _context.RolePermissions.FindAsync(rolePermission.Id);
        updatedRolePermission.Should().NotBeNull();
        updatedRolePermission!.IsGranted.Should().BeTrue();
        updatedRolePermission.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRolePermission()
    {
        // Arrange
        var rolePermission = CreateTestRolePermission(Guid.NewGuid(), Guid.NewGuid());
        await _context.RolePermissions.AddAsync(rolePermission);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(rolePermission.Id);

        // Assert
        var deletedRolePermission = await _context.RolePermissions.FindAsync(rolePermission.Id);
        deletedRolePermission.Should().BeNull();
    }

    [Fact]
    public async Task DeleteByRoleIdAsync_ShouldRemoveAllRolePermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var rolePermission1 = CreateTestRolePermission(roleId, Guid.NewGuid());
        var rolePermission2 = CreateTestRolePermission(roleId, Guid.NewGuid());
        var otherRolePermission = CreateTestRolePermission(Guid.NewGuid(), Guid.NewGuid());
        
        await _context.RolePermissions.AddRangeAsync(rolePermission1, rolePermission2, otherRolePermission);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteByRoleIdAsync(roleId);

        // Assert
        var remainingRolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        remainingRolePermissions.Should().BeEmpty();
        
        var otherRolePermissionStillExists = await _context.RolePermissions.FindAsync(otherRolePermission.Id);
        otherRolePermissionStillExists.Should().NotBeNull();
    }

    private Role CreateTestRole(SystemRole systemRole)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = systemRole.ToString(),
            Description = $"Test {systemRole} role",
            SystemRole = systemRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private UserRole CreateTestUserRole(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private RolePermission CreateTestRolePermission(Guid roleId, Guid permissionId, bool isGranted = true)
    {
        return new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            IsGranted = isGranted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Menu CreateTestMenu(string name, string displayName)
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
            HasCrudPermissions = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Permission CreateTestPermission(string permissionKey, Guid menuId)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            PermissionKey = permissionKey,
            DisplayName = permissionKey.Replace(".", " ").Replace("_", " "),
            PermissionType = PermissionType.View,
            MenuId = menuId,
            IsActive = true,
            IsSystemPermission = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

public class UserSubscriptionRepositoryTests : IDisposable
{
    private readonly UserSubscriptionRepository _sut;
    private readonly AppDbContext _context;

    public UserSubscriptionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new UserSubscriptionRepository(_context);
    }

    [Fact]
    public void UserSubscriptionRepository_ShouldBeInstantiated()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveSubscriptionAsync_WithActiveSubscription_ShouldReturnSubscription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeSubscription = CreateTestSubscription(userId, DateTime.UtcNow.AddDays(30), true);
        var expiredSubscription = CreateTestSubscription(userId, DateTime.UtcNow.AddDays(-5), true);
        var inactiveSubscription = CreateTestSubscription(userId, DateTime.UtcNow.AddDays(15), false);
        
        await _context.UserSubscriptions.AddRangeAsync(activeSubscription, expiredSubscription, inactiveSubscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetActiveSubscriptionAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeSubscription.Id);
    }

    [Fact]
    public async Task GetActiveSubscriptionAsync_WithNoActiveSubscription_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredSubscription = CreateTestSubscription(userId, DateTime.UtcNow.AddDays(-5), true);
        
        await _context.UserSubscriptions.AddAsync(expiredSubscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetActiveSubscriptionAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserSubscriptionsAsync_ShouldReturnAllUserSubscriptionsOrderedByCreatedDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var subscription1 = CreateTestSubscription(userId, DateTime.UtcNow.AddDays(30), true);
        subscription1.CreatedAt = DateTime.UtcNow.AddDays(-10);
        
        var subscription2 = CreateTestSubscription(userId, DateTime.UtcNow.AddDays(-5), false);
        subscription2.CreatedAt = DateTime.UtcNow.AddDays(-5);
        
        var otherUserSubscription = CreateTestSubscription(otherUserId, DateTime.UtcNow.AddDays(15), true);
        
        await _context.UserSubscriptions.AddRangeAsync(subscription1, subscription2, otherUserSubscription);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetUserSubscriptionsAsync(userId);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList[0].Id.Should().Be(subscription2.Id); // Most recent first
        resultList[1].Id.Should().Be(subscription1.Id);
        resultList.Should().NotContain(s => s.Id == otherUserSubscription.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSubscription()
    {
        // Arrange
        var subscription = CreateTestSubscription(Guid.NewGuid(), DateTime.UtcNow.AddDays(30), true);

        // Act
        var result = await _sut.AddAsync(subscription);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(subscription.Id);
        
        var savedSubscription = await _context.UserSubscriptions.FindAsync(subscription.Id);
        savedSubscription.Should().NotBeNull();
        savedSubscription!.SubscriptionType.Should().Be(subscription.SubscriptionType);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSubscription()
    {
        // Arrange
        var subscription = CreateTestSubscription(Guid.NewGuid(), DateTime.UtcNow.AddDays(30), true);
        await _context.UserSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
        
        var originalUpdateTime = subscription.UpdatedAt;
        subscription.IsActive = false;

        // Act
        await _sut.UpdateAsync(subscription);

        // Assert
        var updatedSubscription = await _context.UserSubscriptions.FindAsync(subscription.Id);
        updatedSubscription.Should().NotBeNull();
        updatedSubscription!.IsActive.Should().BeFalse();
        updatedSubscription.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSubscription()
    {
        // Arrange
        var subscription = CreateTestSubscription(Guid.NewGuid(), DateTime.UtcNow.AddDays(30), true);
        await _context.UserSubscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(subscription.Id);

        // Assert
        var deletedSubscription = await _context.UserSubscriptions.FindAsync(subscription.Id);
        deletedSubscription.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _sut.DeleteAsync(Guid.NewGuid());
        await act.Should().NotThrowAsync();
    }

    private UserSubscription CreateTestSubscription(Guid userId, DateTime endDate, bool isActive)
    {
        return new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubscriptionType = SubscriptionType.Premium,
            StartDate = DateTime.UtcNow,
            EndDate = endDate,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

public class UserProfileRepositoryTests : IDisposable
{
    private readonly UserProfileRepository _sut;
    private readonly AppDbContext _context;

    public UserProfileRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new UserProfileRepository(_context);
    }

    [Fact]
    public void UserProfileRepository_ShouldBeInstantiated()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = CreateTestProfile(userId);
        var otherProfile = CreateTestProfile(Guid.NewGuid());
        
        await _context.UserProfiles.AddRangeAsync(profile, otherProfile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FirstName.Should().Be(profile.FirstName);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithInvalidUserId_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddProfile()
    {
        // Arrange
        var profile = CreateTestProfile(Guid.NewGuid());

        // Act
        var result = await _sut.AddAsync(profile);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(profile.Id);
        
        var savedProfile = await _context.UserProfiles.FindAsync(profile.Id);
        savedProfile.Should().NotBeNull();
        savedProfile!.FirstName.Should().Be(profile.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProfile()
    {
        // Arrange
        var profile = CreateTestProfile(Guid.NewGuid());
        await _context.UserProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();
        
        var originalUpdateTime = profile.UpdatedAt;
        profile.FirstName = "Updated Name";

        // Act
        await _sut.UpdateAsync(profile);

        // Assert
        var updatedProfile = await _context.UserProfiles.FindAsync(profile.Id);
        updatedProfile.Should().NotBeNull();
        updatedProfile!.FirstName.Should().Be("Updated Name");
        updatedProfile.UpdatedAt.Should().BeAfter(originalUpdateTime);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProfile()
    {
        // Arrange
        var profile = CreateTestProfile(Guid.NewGuid());
        await _context.UserProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(profile.Id);

        // Assert
        var deletedProfile = await _context.UserProfiles.FindAsync(profile.Id);
        deletedProfile.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _sut.DeleteAsync(Guid.NewGuid());
        await act.Should().NotThrowAsync();
    }

    private UserProfile CreateTestProfile(Guid userId)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            PhoneNumber = "+1234567890",

            City = "Test City",
            Country = "Test Country",
            ProfilePictureUrl = "https://example.com/profile.jpg",
            Bio = "Test bio",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}