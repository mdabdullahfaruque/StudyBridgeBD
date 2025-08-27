using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBridge.UserManagement.Domain.Repositories;
using StudyBridge.UserManagement.Infrastructure;
using StudyBridge.UserManagement.Application.Services;
using StudyBridge.UserManagement.Application.Handlers;
using StudyBridge.Shared.CQRS;
using Microsoft.EntityFrameworkCore;

namespace StudyBridge.UserManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserManagementModule<TContext>(this IServiceCollection services, IConfiguration configuration)
        where TContext : DbContext
    {
        // HttpClient for Google Auth
        services.AddHttpClient<IGoogleAuthService, GoogleAuthService>();
        
        // Repositories
        services.AddScoped<IUserRepository, UserRepository<TContext>>();
        
        // Services
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IJwtService, JwtService>();
        
        // Handlers
        services.AddScoped<ICommandHandler<Application.Commands.GoogleLoginCommand, Application.DTOs.LoginResponse>, GoogleLoginCommandHandler>();
        services.AddScoped<ICommandHandler<Application.Commands.RegisterCommand, Application.DTOs.LoginResponse>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<Application.Commands.LoginCommand, Application.DTOs.LoginResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<Application.Commands.ChangePasswordCommand, bool>, ChangePasswordCommandHandler>();
        
        return services;
    }
}
