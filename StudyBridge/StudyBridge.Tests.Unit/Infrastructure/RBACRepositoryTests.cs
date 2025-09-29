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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
