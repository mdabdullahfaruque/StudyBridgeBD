using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;

namespace StudyBridge.Infrastructure.Data;

public class DataSeederService
{
    public static async Task SeedDefaultRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        try
        {
            // SuperAdmin Role
            await CreateRoleIfNotExists(permissionService, logger, "Super Administrator", SystemRole.SuperAdmin, new[]
            {
                Permission.ViewUsers, Permission.CreateUsers, Permission.EditUsers, Permission.DeleteUsers, Permission.ManageUserRoles,
                Permission.ViewContent, Permission.CreateContent, Permission.EditContent, Permission.DeleteContent, Permission.PublishContent,
                Permission.ViewFinancials, Permission.ManagePayments, Permission.ViewReports, Permission.ManageSubscriptions, Permission.ManageRefunds,
                Permission.ViewSystemLogs, Permission.ManageSystemSettings, Permission.ViewAnalytics, Permission.ManageBackups,
                Permission.ManageVocabularyModule, Permission.ManageIeltsModule, Permission.ManagePteModule, Permission.ManageGreModule, Permission.ManageHigherStudiesModule,
                Permission.ManageAdministrators, Permission.SystemMaintenance
            });

            // Admin Role
            await CreateRoleIfNotExists(permissionService, logger, "Administrator", SystemRole.Admin, new[]
            {
                Permission.ViewUsers, Permission.CreateUsers, Permission.EditUsers, Permission.ManageUserRoles,
                Permission.ViewContent, Permission.CreateContent, Permission.EditContent, Permission.DeleteContent, Permission.PublishContent,
                Permission.ViewFinancials, Permission.ViewReports, Permission.ManageSubscriptions,
                Permission.ViewSystemLogs, Permission.ViewAnalytics,
                Permission.ManageVocabularyModule, Permission.ManageIeltsModule, Permission.ManagePteModule, Permission.ManageGreModule, Permission.ManageHigherStudiesModule
            });

            // Finance Role
            await CreateRoleIfNotExists(permissionService, logger, "Finance Manager", SystemRole.Finance, new[]
            {
                Permission.ViewUsers,
                Permission.ViewFinancials, Permission.ManagePayments, Permission.ViewReports, Permission.ManageSubscriptions, Permission.ManageRefunds,
                Permission.ViewAnalytics
            });

            // Accounts Role
            await CreateRoleIfNotExists(permissionService, logger, "Accounts Manager", SystemRole.Accounts, new[]
            {
                Permission.ViewUsers,
                Permission.ViewFinancials, Permission.ViewReports, Permission.ManageSubscriptions
            });

            // Content Manager Role
            await CreateRoleIfNotExists(permissionService, logger, "Content Manager", SystemRole.ContentManager, new[]
            {
                Permission.ViewUsers,
                Permission.ViewContent, Permission.CreateContent, Permission.EditContent, Permission.PublishContent,
                Permission.ManageVocabularyModule, Permission.ManageIeltsModule, Permission.ManagePteModule, Permission.ManageGreModule, Permission.ManageHigherStudiesModule
            });

            // User Role (Default)
            await CreateRoleIfNotExists(permissionService, logger, "User", SystemRole.User, new[]
            {
                Permission.ViewContent
            });

            logger.LogInformation("Default roles seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding default roles");
        }
    }

    private static async Task CreateRoleIfNotExists(
        IPermissionService permissionService, 
        ILogger logger, 
        string roleName, 
        SystemRole systemRole, 
        Permission[] permissions)
    {
        try
        {
            var success = await permissionService.CreateRoleAsync(roleName, systemRole, permissions);
            if (success)
            {
                logger.LogInformation("Created role: {RoleName}", roleName);
            }
            else
            {
                logger.LogInformation("Role already exists: {RoleName}", roleName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating role: {RoleName}", roleName);
        }
    }
}
