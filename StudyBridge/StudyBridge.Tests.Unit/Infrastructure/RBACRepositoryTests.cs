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
            .EnableSensitiveDataLogging()
            .Options;
        
        _context = new AppDbContext(options);
        // Ensure the database is created
        _context.Database.EnsureCreated();
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

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMenu()
    {
        // Arrange
        var menu = CreateTestMenu("test", "Test Menu");
        await _context.Menus.AddAsync(menu);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(menu.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(menu.Id);
        result.Name.Should().Be("test");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMenus()
    {
        // Arrange
        var menus = new[]
        {
            CreateTestMenu("menu1", "Menu 1"),
            CreateTestMenu("menu2", "Menu 2")
        };
        await _context.Menus.AddRangeAsync(menus);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Name == "menu1");
        result.Should().Contain(m => m.Name == "menu2");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMenu()
    {
        // Arrange
        var menu = CreateTestMenu("test", "Test Menu");
        await _context.Menus.AddAsync(menu);
        await _context.SaveChangesAsync();

        menu.DisplayName = "Updated Menu";

        // Act
        await _sut.UpdateAsync(menu);

        // Assert
        var updatedMenu = await _context.Menus.FindAsync(menu.Id);
        updatedMenu.Should().NotBeNull();
        updatedMenu!.DisplayName.Should().Be("Updated Menu");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteMenu()
    {
        // Arrange
        var menu = CreateTestMenu("test", "Test Menu");
        await _context.Menus.AddAsync(menu);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(menu.Id);

        // Assert
        var deletedMenu = await _context.Menus.FindAsync(menu.Id);
        deletedMenu.Should().NotBeNull();
        deletedMenu!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnMenu()
    {
        // Arrange
        var menu = CreateTestMenu("test-menu", "Test Menu");
        await _context.Menus.AddAsync(menu);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByNameAsync("test-menu");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("test-menu");
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
            .EnableSensitiveDataLogging()
            .Options;
        
        _context = new AppDbContext(options);
        // Ensure the database is created
        _context.Database.EnsureCreated();
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
        var menu = CreateTestMenu("users", "User Management");
        await _context.Menus.AddAsync(menu);
        await _context.SaveChangesAsync();
        
        var permission = CreateTestPermission("users.view", menu.Id);

        // Act
        var result = await _sut.AddAsync(permission);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(permission.Id);
        
        var savedPermission = await _context.Permissions.FindAsync(permission.Id);
        savedPermission.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldReturnPermission()
    {
        // Arrange
        var menu = CreateTestMenu("users", "User Management");
        await _context.Menus.AddAsync(menu);
        await _context.SaveChangesAsync();
        
        var permission = CreateTestPermission("users.create", menu.Id);
        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByKeyAsync("users.create");

        // Assert
        result.Should().NotBeNull();
        result!.PermissionKey.Should().Be("users.create");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPermissions()
    {
        // Arrange
        var menu = CreateTestMenu("users", "User Management");
        await _context.Menus.AddAsync(menu);
        await _context.SaveChangesAsync();
        
        var permissions = new[]
        {
            CreateTestPermission("users.view", menu.Id),
            CreateTestPermission("users.edit", menu.Id)
        };
        await _context.Permissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.PermissionKey == "users.view");
        result.Should().Contain(p => p.PermissionKey == "users.edit");
    }

    [Fact]
    public async Task GetByMenuIdAsync_ShouldReturnMenuPermissions()
    {
        // Arrange
        var menu1 = CreateTestMenu("users", "User Management");
        var menu2 = CreateTestMenu("posts", "Post Management");
        await _context.Menus.AddRangeAsync(menu1, menu2);
        await _context.SaveChangesAsync();
        
        var permissions = new[]
        {
            CreateTestPermission("users.view", menu1.Id),
            CreateTestPermission("users.edit", menu1.Id),
            CreateTestPermission("posts.view", menu2.Id)
        };
        await _context.Permissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByMenuIdAsync(menu1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.MenuId == menu1.Id);
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

    public void Dispose()
    {
        _context?.Dispose();
    }
}