using Moq;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using StudyBridge.Infrastructure.Data;
using StudyBridge.Infrastructure.Repositories;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Tests.Unit.Infrastructure;

public class MenuRepositoryTests : IDisposable
{
    private readonly MenuRepository _sut;
    private readonly AppDbContext _context;

    public MenuRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _sut = new MenuRepository(_context);
    }

    [Fact]
    public void MenuRepository_ShouldBeInstantiated()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddMenu()
    {
        // Arrange
        var menu = CreateTestMenu("test", "Test Menu");

        // Act
        var result = await _sut.AddAsync(menu);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(menu.Id);
        
        var savedMenu = await _context.Menus.FindAsync(menu.Id);
        savedMenu.Should().NotBeNull();
    }

    private Menu CreateTestMenu(string name, string displayName, string? icon = null, Guid? parentId = null)
    {
        return new Menu
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            Icon = icon ?? "fas fa-home",
            Route = $"/{name}",
            MenuType = MenuType.Admin,
            ParentMenuId = parentId,
            SortOrder = 1,
            IsActive = true,
            HasCrudPermissions = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

public class PermissionRepositoryTests : IDisposable
{
    private readonly PermissionRepository _sut;
    private readonly AppDbContext _context;

    public PermissionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _sut = new PermissionRepository(_context);
    }

    [Fact]
    public void PermissionRepository_ShouldBeInstantiated()
    {
        // Assert
        _sut.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddPermission()
    {
        // Arrange
        var permission = CreateTestPermission("users.view", Guid.NewGuid());

        // Act
        var result = await _sut.AddAsync(permission);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(permission.Id);
        
        var savedPermission = await _context.Permissions.FindAsync(permission.Id);
        savedPermission.Should().NotBeNull();
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