using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Application.Services;
using StudyBridge.UserManagement.Features.Authentication;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.UserManagement.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManagementModule<TDbContext>(
        this IServiceCollection services, 
        IConfiguration configuration)
        where TDbContext : DbContext, IApplicationDbContext
    {
        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<TDbContext>());

        // Register Authentication Features
        services.AddScoped<Register.Handler>();
        services.AddScoped<Register.Validator>();
        services.AddScoped<Login.Handler>();
        services.AddScoped<Login.Validator>();
        services.AddScoped<GoogleLogin.Handler>();
        services.AddScoped<GoogleLogin.Validator>();
        services.AddScoped<ChangePassword.Handler>();
        services.AddScoped<ChangePassword.Validator>();

        // Register User Profile Features
        services.AddScoped<GetProfile.Handler>();
        services.AddScoped<UpdateProfile.Handler>();
        services.AddScoped<UpdateProfile.Validator>();

        // Register Application Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
