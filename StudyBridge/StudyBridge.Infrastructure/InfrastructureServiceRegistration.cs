using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBridge.Infrastructure.Extensions;
using StudyBridge.Infrastructure.Data;
using StudyBridge.Infrastructure.Repositories;
using StudyBridge.Infrastructure.Services;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Application.Services;
using Microsoft.AspNetCore.Identity;
using StudyBridge.Domain.Entities;

namespace StudyBridge.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        
        // Register application interfaces
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<AppDbContext>());

        // Register repositories
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // Register services
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        // Register ASP.NET Core Identity services (without Entity Framework stores)
        services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
        
        return services;
    }
}
