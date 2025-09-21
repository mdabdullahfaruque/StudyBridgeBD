using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;

namespace StudyBridge.Infrastructure.Data;

public class DataSeederService
{
    public static async Task SeedAllDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        try
        {
            logger.LogInformation("Starting simplified menu-based data seeding...");
            
            await SeedSystemRolesAsync(serviceProvider);
            await SeedMenusAsync(serviceProvider);
            await SeedPublicMenusAsync(serviceProvider);
            await SeedRoleMenusAsync(serviceProvider);
            await SeedDefaultAdminUserAsync(serviceProvider);
            
            logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during data seeding");
            throw;
        }
    }
    
    public static Task SeedDefaultRolesAsync(IServiceProvider serviceProvider)
    {
        // Legacy method - redirect to comprehensive seeding
        return SeedAllDataAsync(serviceProvider);
    }

    private static async Task SeedSystemRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        logger.LogInformation("Seeding system roles...");

        var systemRoles = new[]
        {
            new { Name = "SuperAdmin", Description = "Full system access with all menus" },
            new { Name = "Admin", Description = "Administrative access with most menus" },
            new { Name = "Finance", Description = "Financial data access and management" },
            new { Name = "Accounts", Description = "Account management and billing" },
            new { Name = "ContentManager", Description = "Content creation and management" },
            new { Name = "User", Description = "Standard user with basic menu access" }
        };

        var existingRoles = await roleRepository.GetAllAsync();
        var existingRoleNames = existingRoles.Select(r => r.Name).ToHashSet();

        foreach (var roleData in systemRoles)
        {
            if (!existingRoleNames.Contains(roleData.Name))
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleData.Name,
                    Description = roleData.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await roleRepository.AddAsync(role);
                logger.LogInformation("Created role: {RoleName}", roleData.Name);
            }
        }
    }

    private static async Task SeedMenusAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        logger.LogInformation("Seeding admin menus...");

        // Parent menu definitions with proper routes
        var parentMenus = new[]
        {
            new { Name = "dashboard", DisplayName = "Dashboard", Icon = "pi pi-home", Route = "/admin/dashboard", SortOrder = 10 },
            new { Name = "content-management", DisplayName = "Content", Icon = "pi pi-file-edit", Route = "/admin/content", SortOrder = 20 },
            new { Name = "financial-management", DisplayName = "Financials", Icon = "pi pi-dollar", Route = "/admin/financials", SortOrder = 30 },
            new { Name = "system-management", DisplayName = "System", Icon = "pi pi-cog", Route = "/admin/system", SortOrder = 40 }
        };

        // Child menu definitions
        var childMenus = new[]
        {
            // System Management child menus
            new { Name = "user-management", DisplayName = "User Management", Icon = "pi pi-users", Route = "/admin/users", ParentName = "system-management", SortOrder = 10 },
            new { Name = "role-management", DisplayName = "Role Management", Icon = "pi pi-key", Route = "/admin/roles", ParentName = "system-management", SortOrder = 20 },
            new { Name = "menu-management", DisplayName = "Menu Management", Icon = "pi pi-list", Route = "/admin/menus", ParentName = "system-management", SortOrder = 25 },
            new { Name = "app-settings", DisplayName = "Settings", Icon = "pi pi-sliders-h", Route = "/admin/settings", ParentName = "system-management", SortOrder = 30 },
            new { Name = "audit-logs", DisplayName = "Audit Logs", Icon = "pi pi-history", Route = "/admin/audit-logs", ParentName = "system-management", SortOrder = 40 },
            
            // Content Management sub-menus
            new { Name = "content-vocabulary", DisplayName = "Vocabulary", Icon = "pi pi-book", Route = "/admin/vocabulary", ParentName = "content-management", SortOrder = 10 },
            new { Name = "content-categories", DisplayName = "Categories", Icon = "pi pi-tags", Route = "/admin/categories", ParentName = "content-management", SortOrder = 20 },
            
            // Financial Management sub-menus
            new { Name = "financials-overview", DisplayName = "Overview", Icon = "pi pi-chart-bar", Route = "/admin/financials/overview", ParentName = "financial-management", SortOrder = 10 },
            new { Name = "financials-subscriptions", DisplayName = "Subscriptions", Icon = "pi pi-credit-card", Route = "/admin/subscriptions", ParentName = "financial-management", SortOrder = 20 },
            new { Name = "financials-reports", DisplayName = "Reports", Icon = "pi pi-chart-line", Route = "/admin/reports", ParentName = "financial-management", SortOrder = 30 },
        };

        var createdMenus = new Dictionary<string, Menu>();

        // First pass: Create parent menus
        foreach (var menuData in parentMenus)
        {
            var existingMenu = await menuRepository.GetByNameAsync(menuData.Name);
            if (existingMenu == null)
            {
                var menu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Name = menuData.Name,
                    DisplayName = menuData.DisplayName,
                    Description = $"Access to {menuData.DisplayName} section",
                    Icon = menuData.Icon,
                    Route = string.IsNullOrEmpty(menuData.Route) ? null : menuData.Route,
                    SortOrder = menuData.SortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await menuRepository.AddAsync(menu);
                createdMenus[menuData.Name] = created;
                logger.LogInformation("Created parent menu: {MenuName}", menuData.Name);
            }
            else
            {
                createdMenus[menuData.Name] = existingMenu;
            }
        }

        // Second pass: Create child menus
        foreach (var menuData in childMenus)
        {
            var existingMenu = await menuRepository.GetByNameAsync(menuData.Name);
            if (existingMenu == null && createdMenus.ContainsKey(menuData.ParentName))
            {
                var parentMenu = createdMenus[menuData.ParentName];
                var menu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Name = menuData.Name,
                    DisplayName = menuData.DisplayName,
                    Description = $"Access to {menuData.DisplayName} functionality",
                    Icon = menuData.Icon,
                    Route = menuData.Route,
                    ParentMenuId = parentMenu.Id,
                    SortOrder = menuData.SortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await menuRepository.AddAsync(menu);
                logger.LogInformation("Created child menu: {MenuName} under {ParentName}", menuData.Name, menuData.ParentName);
            }
        }
    }

    private static async Task SeedPublicMenusAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        logger.LogInformation("Seeding public menus...");

        // Public menu definitions for regular users
        var publicMenus = new[]
        {
            new { Name = "public-dashboard", DisplayName = "Dashboard", Icon = "pi pi-home", Route = "/public/dashboard", SortOrder = 10 },
            new { Name = "public-vocabulary", DisplayName = "Vocabulary", Icon = "pi pi-book", Route = "/public/vocabulary", SortOrder = 20 },
            new { Name = "public-learning", DisplayName = "Learning", Icon = "pi pi-lightbulb", Route = "/public/learning", SortOrder = 30 }
        };

        foreach (var menuData in publicMenus)
        {
            var existingMenu = await menuRepository.GetByNameAsync(menuData.Name);
            if (existingMenu == null)
            {
                var menu = new Menu
                {
                    Id = Guid.NewGuid(),
                    Name = menuData.Name,
                    DisplayName = menuData.DisplayName,
                    Description = $"Access to {menuData.DisplayName} for regular users",
                    Icon = menuData.Icon,
                    Route = menuData.Route,
                    SortOrder = menuData.SortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await menuRepository.AddAsync(menu);
                logger.LogInformation("Created public menu: {MenuName}", menuData.Name);
            }
        }
    }

    private static async Task SeedRoleMenusAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        logger.LogInformation("Seeding role menu assignments...");

        var allRoles = await roleRepository.GetAllAsync();
        var allMenus = await menuRepository.GetAllAsync();
        var menuLookup = allMenus.ToDictionary(m => m.Name, m => m);

        var roleMenuMappings = new Dictionary<string, string[]>
        {
            ["SuperAdmin"] = allMenus.Select(m => m.Name).ToArray(), // All menus
            ["Admin"] = new[]
            {
                "dashboard", "content-management", "system-management",
                "user-management", "role-management", "menu-management", "app-settings", "audit-logs",
                "content-vocabulary", "content-categories"
            },
            ["Finance"] = new[]
            {
                "dashboard", "financial-management", "system-management", "user-management",
                "financials-overview", "financials-subscriptions", "financials-reports"
            },
            ["Accounts"] = new[]
            {
                "dashboard", "financial-management", "system-management", "user-management",
                "financials-overview"
            },
            ["ContentManager"] = new[]
            {
                "dashboard", "content-management",
                "content-vocabulary", "content-categories"
            },
            ["User"] = new[]
            {
                "public-dashboard", "public-vocabulary", "public-learning"
            }
        };

        foreach (var role in allRoles)
        {
            if (roleMenuMappings.TryGetValue(role.Name, out var menuNames))
            {
                // Check existing role-menu assignments
                var existingRoleMenus = await context.RoleMenus
                    .Where(rm => rm.RoleId == role.Id)
                    .Select(rm => rm.MenuId)
                    .ToListAsync();

                foreach (var menuName in menuNames)
                {
                    if (menuLookup.TryGetValue(menuName, out var menu) 
                        && !existingRoleMenus.Contains(menu.Id))
                    {
                        var roleMenu = new RoleMenu
                        {
                            Id = Guid.NewGuid(),
                            RoleId = role.Id,
                            MenuId = menu.Id,
                            GrantedAt = DateTime.UtcNow,
                            GrantedBy = null, // System seeded
                            IsActive = true
                        };

                        context.RoleMenus.Add(roleMenu);
                        logger.LogInformation("Assigned menu {MenuName} to role {RoleName}", 
                            menuName, role.Name);
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDefaultAdminUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        logger.LogInformation("Seeding default admin user...");

        const string adminEmail = "admin@studybridge.com";
        const string defaultPassword = "Admin@123456";

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (existingUser == null)
        {
            var adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                DisplayName = "System Administrator",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                LoginProvider = LoginProvider.Local,
                IsPublicUser = false, // Admin user - should access admin layout
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Hash password like Register API does
            var passwordHash = passwordHasher.HashPassword(new AppUser(), defaultPassword);
            adminUser.PasswordHash = passwordHash;

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Assign SuperAdmin role
            var allRoles = await roleRepository.GetAllAsync();
            var superAdminRole = allRoles.FirstOrDefault(r => r.Name == "SuperAdmin");
            if (superAdminRole != null)
            {
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = superAdminRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = null, // System
                    IsActive = true
                };

                await userRoleRepository.AddAsync(userRole);
                logger.LogInformation("Created default admin user: {Email} with SuperAdmin role", adminEmail);
            }
        }
        else
        {
            logger.LogInformation("Default admin user already exists: {Email}", adminEmail);
        }

        // Create test user with regular User role
        const string testUserEmail = "user@studybridge.com";
        const string testUserPassword = "User@123456";
        
        var existingTestUser = await context.Users.FirstOrDefaultAsync(u => u.Email == testUserEmail);
        if (existingTestUser == null)
        {
            var testUser = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = testUserEmail,
                DisplayName = "Test User",
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true,
                LoginProvider = LoginProvider.Local,
                IsPublicUser = true, // Regular user - should access public layout
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Hash password like Register API does
            var testPasswordHash = passwordHasher.HashPassword(new AppUser(), testUserPassword);
            testUser.PasswordHash = testPasswordHash;

            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            // Assign User role
            var allRoles2 = await roleRepository.GetAllAsync();
            var userRole = allRoles2.FirstOrDefault(r => r.Name == "User");
            if (userRole != null)
            {
                var testUserRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = testUser.Id,
                    RoleId = userRole.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = null, // System
                    IsActive = true
                };

                await userRoleRepository.AddAsync(testUserRole);
                logger.LogInformation("Created test user: {Email} with User role", testUserEmail);
            }
        }
        else
        {
            logger.LogInformation("Test user already exists: {Email}", testUserEmail);
        }
    }
}
