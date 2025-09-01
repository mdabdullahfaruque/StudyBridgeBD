using Microsoft.Extensions.DependencyInjection;
using StudyBridge.UserManagement.Features.Authentication;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.UserManagement.Features;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManagementFeatures(this IServiceCollection services)
    {
        // Authentication Features - using nested classes
        services.AddScoped<Login.Handler>();
        services.AddScoped<Login.Validator>();
        services.AddScoped<Register.Handler>();
        services.AddScoped<Register.Validator>();

        return services;
    }
}
