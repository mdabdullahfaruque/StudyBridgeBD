using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;

namespace StudyBridge.Infrastructure.Data;

public class DataSeederService
{
    public static Task SeedDefaultRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeederService>>();

        try
        {
            // TODO: Implement new role seeding with new Permission entities
            // This will be implemented after database migration is complete
            logger.LogInformation("Role seeding temporarily disabled during RBAC refactoring");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding default roles");
        }
        
        return Task.CompletedTask;
    }
}
