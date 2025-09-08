using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Application.Services;
using StudyBridge.UserManagement.Features.Authentication;
using StudyBridge.UserManagement.Features.UserProfile;
using StudyBridge.Shared.CQRS;

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

        // Register Authentication Features - Command Handlers
        services.AddScoped<ICommandHandler<Register.Command, Register.Response>, Register.Handler>();
        services.AddScoped<Register.Validator>();
        services.AddScoped<ICommandHandler<Login.Command, Login.Response>, Login.Handler>();
        services.AddScoped<Login.Validator>();
        services.AddScoped<ICommandHandler<GoogleLogin.Command, GoogleLogin.Response>, GoogleLogin.Handler>();
        services.AddScoped<GoogleLogin.Validator>();
        services.AddScoped<ICommandHandler<ChangePassword.Command, ChangePassword.Response>, ChangePassword.Handler>();
        services.AddScoped<ChangePassword.Validator>();

        // Register User Profile Features - Query and Command Handlers
        services.AddScoped<IQueryHandler<GetProfile.Query, GetProfile.Response>, GetProfile.Handler>();
        services.AddScoped<ICommandHandler<UpdateProfile.Command, UpdateProfile.Response>, UpdateProfile.Handler>();
        services.AddScoped<UpdateProfile.Validator>();

        // Register Application Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
