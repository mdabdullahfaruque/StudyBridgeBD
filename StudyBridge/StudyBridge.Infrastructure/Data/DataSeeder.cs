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
            logger.LogInformation("Starting comprehensive data seeding...");
            
            await SeedSystemRolesAsync(serviceProvider);
            await SeedMenusAsync(serviceProvider);
            await SeedPublicMenusAsync(serviceProvider);
            await SeedPermissionsAsync(serviceProvider);
            await SeedRolePermissionsAsync(serviceProvider);
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
            new { SystemRole = SystemRole.SuperAdmin, Name = "SuperAdmin", Description = "Full system access with all permissions" },
            new { SystemRole = SystemRole.Admin, Name = "Admin", Description = "Administrative access with most permissions" },
            new { SystemRole = SystemRole.Finance, Name = "Finance", Description = "Financial data access and management" },
            new { SystemRole = SystemRole.Accounts, Name = "Accounts", Description = "Account management and billing" },
            new { SystemRole = SystemRole.ContentManager, Name = "ContentManager", Description = "Content creation and management" },
            new { SystemRole = SystemRole.User, Name = "User", Description = "Standard user with basic permissions" }
        };

        foreach (var roleData in systemRoles)
        {
            var existingRole = await roleRepository.GetBySystemRoleAsync(roleData.SystemRole);
            if (existingRole == null)
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    SystemRole = roleData.SystemRole,
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

        // Parent menu definitions
        var parentMenus = new[]
        {
            new { Name = "dashboard", DisplayName = "Dashboard", Icon = "pi pi-home", Route = "/admin/dashboard", SortOrder = 10, HasCrud = false },
            new { Name = "user-management", DisplayName = "User Management", Icon = "pi pi-users", Route = "", SortOrder = 20, HasCrud = true },
            new { Name = "role-management", DisplayName = "Role Management", Icon = "pi pi-key", Route = "", SortOrder = 30, HasCrud = true },
            new { Name = "permission-management", DisplayName = "Permissions", Icon = "pi pi-shield", Route = "", SortOrder = 40, HasCrud = true },
            new { Name = "content-management", DisplayName = "Content", Icon = "pi pi-file-edit", Route = "", SortOrder = 50, HasCrud = true },
            new { Name = "financial-management", DisplayName = "Financials", Icon = "pi pi-dollar", Route = "", SortOrder = 60, HasCrud = true },
            new { Name = "system-management", DisplayName = "System", Icon = "pi pi-cog", Route = "", SortOrder = 70, HasCrud = false }
        };

        // Child menu definitions
        var childMenus = new[]
        {
            // User Management sub-menus
            new { Name = "users-list", DisplayName = "All Users", Icon = "pi pi-list", Route = "/admin/users", ParentName = "user-management", SortOrder = 10, HasCrud = false },
            new { Name = "users-create", DisplayName = "Add User", Icon = "pi pi-plus", Route = "/admin/users/create", ParentName = "user-management", SortOrder = 20, HasCrud = false },
            new { Name = "users-roles", DisplayName = "User Roles", Icon = "pi pi-key", Route = "/admin/users/roles", ParentName = "user-management", SortOrder = 30, HasCrud = false },
            
            // Role Management sub-menus
            new { Name = "roles-list", DisplayName = "All Roles", Icon = "pi pi-list", Route = "/admin/roles", ParentName = "role-management", SortOrder = 10, HasCrud = false },
            new { Name = "roles-create", DisplayName = "Create Role", Icon = "pi pi-plus", Route = "/admin/roles/create", ParentName = "role-management", SortOrder = 20, HasCrud = false },
            
            // Permission Management sub-menus  
            new { Name = "permissions-list", DisplayName = "All Permissions", Icon = "pi pi-list", Route = "/admin/permissions", ParentName = "permission-management", SortOrder = 10, HasCrud = false },
            new { Name = "permissions-create", DisplayName = "Create Permission", Icon = "pi pi-plus", Route = "/admin/permissions/create", ParentName = "permission-management", SortOrder = 20, HasCrud = false },
            
            // Content Management sub-menus
            new { Name = "content-vocabulary", DisplayName = "Vocabulary", Icon = "pi pi-book", Route = "/admin/content/vocabulary", ParentName = "content-management", SortOrder = 10, HasCrud = false },
            new { Name = "content-categories", DisplayName = "Categories", Icon = "pi pi-tags", Route = "/admin/content/categories", ParentName = "content-management", SortOrder = 20, HasCrud = false },
            
            // Financial Management sub-menus
            new { Name = "financials-overview", DisplayName = "Overview", Icon = "pi pi-chart-bar", Route = "/admin/financials", ParentName = "financial-management", SortOrder = 10, HasCrud = false },
            new { Name = "financials-subscriptions", DisplayName = "Subscriptions", Icon = "pi pi-credit-card", Route = "/admin/financials/subscriptions", ParentName = "financial-management", SortOrder = 20, HasCrud = false },
            new { Name = "financials-reports", DisplayName = "Reports", Icon = "pi pi-chart-line", Route = "/admin/financials/reports", ParentName = "financial-management", SortOrder = 30, HasCrud = false },
            
            // System Management sub-menus
            new { Name = "system-settings", DisplayName = "Settings", Icon = "pi pi-sliders-h", Route = "/admin/system/settings", ParentName = "system-management", SortOrder = 10, HasCrud = false },
            new { Name = "system-logs", DisplayName = "Logs", Icon = "pi pi-file", Route = "/admin/system/logs", ParentName = "system-management", SortOrder = 20, HasCrud = false },
            new { Name = "system-analytics", DisplayName = "Analytics", Icon = "pi pi-chart-pie", Route = "/admin/system/analytics", ParentName = "system-management", SortOrder = 30, HasCrud = false }
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
                    Icon = menuData.Icon,
                    Route = string.IsNullOrEmpty(menuData.Route) ? null : menuData.Route,
                    MenuType = MenuType.Admin,
                    SortOrder = menuData.SortOrder,
                    IsActive = true,
                    HasCrudPermissions = menuData.HasCrud,
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
                    Icon = menuData.Icon,
                    Route = menuData.Route,
                    MenuType = MenuType.Admin,
                    ParentMenuId = parentMenu.Id,
                    SortOrder = menuData.SortOrder,
                    IsActive = true,
                    HasCrudPermissions = menuData.HasCrud,
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
                    Icon = menuData.Icon,
                    Route = menuData.Route,
                    MenuType = MenuType.Public,
                    SortOrder = menuData.SortOrder,
                    IsActive = true,
                    HasCrudPermissions = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await menuRepository.AddAsync(menu);
                logger.LogInformation("Created public menu: {MenuName}", menuData.Name);
            }
        }
    }

    private static async Task SeedPermissionsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        logger.LogInformation("Seeding permissions...");

        var allMenus = await menuRepository.GetAllAsync();
        var menuLookup = allMenus.ToDictionary(m => m.Name, m => m);

        var permissions = new[]
        {
            // Dashboard permissions
            new { MenuName = "dashboard", PermissionType = PermissionType.View, Key = "dashboard.view", DisplayName = "View Dashboard", Description = "Access to main dashboard" },
            
            // User Management permissions
            new { MenuName = "user-management", PermissionType = PermissionType.View, Key = "users.view", DisplayName = "View Users", Description = "View user listings and details" },
            new { MenuName = "user-management", PermissionType = PermissionType.Create, Key = "users.create", DisplayName = "Create Users", Description = "Create new users" },
            new { MenuName = "user-management", PermissionType = PermissionType.Edit, Key = "users.edit", DisplayName = "Edit Users", Description = "Modify user information" },
            new { MenuName = "user-management", PermissionType = PermissionType.Delete, Key = "users.delete", DisplayName = "Delete Users", Description = "Remove users from system" },
            new { MenuName = "user-management", PermissionType = PermissionType.Manage, Key = "users.manage", DisplayName = "Manage Users", Description = "Full user management access" },
            
            // Role Management permissions
            new { MenuName = "role-management", PermissionType = PermissionType.View, Key = "roles.view", DisplayName = "View Roles", Description = "View role listings and details" },
            new { MenuName = "role-management", PermissionType = PermissionType.Create, Key = "roles.create", DisplayName = "Create Roles", Description = "Create new roles" },
            new { MenuName = "role-management", PermissionType = PermissionType.Edit, Key = "roles.edit", DisplayName = "Edit Roles", Description = "Modify role settings" },
            new { MenuName = "role-management", PermissionType = PermissionType.Delete, Key = "roles.delete", DisplayName = "Delete Roles", Description = "Remove roles from system" },
            new { MenuName = "role-management", PermissionType = PermissionType.Manage, Key = "roles.manage", DisplayName = "Manage Roles", Description = "Full role management access" },
            
            // Permission Management permissions
            new { MenuName = "permission-management", PermissionType = PermissionType.View, Key = "permissions.view", DisplayName = "View Permissions", Description = "View permission listings" },
            new { MenuName = "permission-management", PermissionType = PermissionType.Create, Key = "permissions.create", DisplayName = "Create Permissions", Description = "Create new permissions" },
            new { MenuName = "permission-management", PermissionType = PermissionType.Edit, Key = "permissions.edit", DisplayName = "Edit Permissions", Description = "Modify permissions" },
            new { MenuName = "permission-management", PermissionType = PermissionType.Delete, Key = "permissions.delete", DisplayName = "Delete Permissions", Description = "Remove permissions" },
            new { MenuName = "permission-management", PermissionType = PermissionType.Manage, Key = "permissions.manage", DisplayName = "Manage Permissions", Description = "Full permission management" },
            
            // Content Management permissions
            new { MenuName = "content-management", PermissionType = PermissionType.View, Key = "content.view", DisplayName = "View Content", Description = "View content items" },
            new { MenuName = "content-management", PermissionType = PermissionType.Create, Key = "content.create", DisplayName = "Create Content", Description = "Create new content" },
            new { MenuName = "content-management", PermissionType = PermissionType.Edit, Key = "content.edit", DisplayName = "Edit Content", Description = "Modify content items" },
            new { MenuName = "content-management", PermissionType = PermissionType.Delete, Key = "content.delete", DisplayName = "Delete Content", Description = "Remove content items" },
            new { MenuName = "content-management", PermissionType = PermissionType.Manage, Key = "content.manage", DisplayName = "Manage Content", Description = "Full content management" },
            
            // Financial Management permissions
            new { MenuName = "financial-management", PermissionType = PermissionType.View, Key = "financials.view", DisplayName = "View Financials", Description = "View financial data" },
            new { MenuName = "financial-management", PermissionType = PermissionType.Manage, Key = "financials.manage", DisplayName = "Manage Financials", Description = "Full financial management" },
            
            // System Management permissions
            new { MenuName = "system-management", PermissionType = PermissionType.View, Key = "system.view", DisplayName = "View System", Description = "View system information" },
            new { MenuName = "system-management", PermissionType = PermissionType.Manage, Key = "system.manage", DisplayName = "Manage System", Description = "System administration" },
            new { MenuName = "system-management", PermissionType = PermissionType.Execute, Key = "system.logs", DisplayName = "View Logs", Description = "Access system logs" },
            
            // Analytics and Reports
            new { MenuName = "system-management", PermissionType = PermissionType.View, Key = "analytics.view", DisplayName = "View Analytics", Description = "Access to analytics and reports" },
            new { MenuName = "financial-management", PermissionType = PermissionType.View, Key = "reports.view", DisplayName = "View Reports", Description = "Access to financial reports" },
            
            // Public Menu permissions for regular users
            new { MenuName = "public-dashboard", PermissionType = PermissionType.View, Key = "public.dashboard", DisplayName = "View Public Dashboard", Description = "Access to user dashboard" },
            new { MenuName = "public-vocabulary", PermissionType = PermissionType.View, Key = "public.vocabulary", DisplayName = "View Vocabulary", Description = "Access to vocabulary learning" },
            new { MenuName = "public-learning", PermissionType = PermissionType.View, Key = "public.learning", DisplayName = "Access Learning", Description = "Access to learning modules" }
        };

        foreach (var permData in permissions)
        {
            if (menuLookup.TryGetValue(permData.MenuName, out var menu))
            {
                var existingPermission = await permissionRepository.GetByKeyAsync(permData.Key);
                if (existingPermission == null)
                {
                    var permission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        MenuId = menu.Id,
                        PermissionType = permData.PermissionType,
                        PermissionKey = permData.Key,
                        DisplayName = permData.DisplayName,
                        Description = permData.Description,
                        IsActive = true,
                        IsSystemPermission = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await permissionRepository.AddAsync(permission);
                    logger.LogInformation("Created permission: {PermissionKey}", permData.Key);
                }
            }
        }
    }

    private static async Task SeedRolePermissionsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        var rolePermissionRepository = scope.ServiceProvider.GetRequiredService<IRolePermissionRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        logger.LogInformation("Seeding role permissions...");

        var allRoles = await roleRepository.GetAllAsync();
        var allPermissions = await permissionRepository.GetAllAsync();
        var permissionLookup = allPermissions.ToDictionary(p => p.PermissionKey, p => p);

        var rolePermissionMappings = new Dictionary<SystemRole, string[]>
        {
            [SystemRole.SuperAdmin] = allPermissions.Select(p => p.PermissionKey).ToArray(),
            [SystemRole.Admin] = new[]
            {
                "dashboard.view", "users.view", "users.create", "users.edit", "users.delete",
                "roles.view", "roles.create", "roles.edit", "permissions.view",
                "content.view", "content.create", "content.edit", "content.delete",
                "system.view", "reports.view"
            },
            [SystemRole.Finance] = new[]
            {
                "dashboard.view", "users.view", "financials.view", "financials.manage", "reports.view"
            },
            [SystemRole.Accounts] = new[]
            {
                "dashboard.view", "users.view", "financials.view"
            },
            [SystemRole.ContentManager] = new[]
            {
                "dashboard.view", "users.view", "content.view", "content.create", "content.edit", "content.delete"
            },
            [SystemRole.User] = new[]
            {
                "dashboard.view", "public.dashboard", "public.vocabulary", "public.learning"
            }
        };

        foreach (var role in allRoles)
        {
            if (rolePermissionMappings.TryGetValue(role.SystemRole, out var permissionKeys))
            {
                var existingRolePermissions = await rolePermissionRepository.GetRolePermissionsAsync(role.Id);
                var existingPermissionIds = existingRolePermissions.Select(rp => rp.PermissionId).ToHashSet();

                foreach (var permissionKey in permissionKeys)
                {
                    if (permissionLookup.TryGetValue(permissionKey, out var permission) 
                        && !existingPermissionIds.Contains(permission.Id))
                    {
                        var rolePermission = new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = role.Id,
                            PermissionId = permission.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await rolePermissionRepository.AddAsync(rolePermission);
                        logger.LogInformation("Assigned permission {PermissionKey} to role {RoleName}", 
                            permissionKey, role.Name);
                    }
                }
            }
        }
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
            var superAdminRole = await roleRepository.GetBySystemRoleAsync(SystemRole.SuperAdmin);
            if (superAdminRole != null)
            {
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = superAdminRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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
            var userRole = await roleRepository.GetBySystemRoleAsync(SystemRole.User);
            if (userRole != null)
            {
                var testUserRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = testUser.Id,
                    RoleId = userRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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
