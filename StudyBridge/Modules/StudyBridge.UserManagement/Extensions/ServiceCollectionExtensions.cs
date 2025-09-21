using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Application.Services;
using StudyBridge.UserManagement.Features.Admin;
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

        // Register Admin Features - Query and Command Handlers
        services.AddScoped<IQueryHandler<GetUsers.Query, GetUsers.Response>, GetUsers.Handler>();
        services.AddScoped<GetUsers.Validator>();
        services.AddScoped<IQueryHandler<GetUserById.Query, GetUserById.Response>, GetUserById.Handler>();
        services.AddScoped<GetUserById.Validator>();
        services.AddScoped<ICommandHandler<CreateUser.Command, CreateUser.Response>, CreateUser.Handler>();
        services.AddScoped<CreateUser.Validator>();
        services.AddScoped<IQueryHandler<GetRoles.Query, GetRoles.Response>, GetRoles.Handler>();
        
        // Register Role Management Features - Query and Command Handlers
        services.AddScoped<IQueryHandler<GetRoleById.Query, GetRoleById.Response>, GetRoleById.Handler>();
        services.AddScoped<GetRoleById.Validator>();
        services.AddScoped<ICommandHandler<CreateRole.Command, CreateRole.Response>, CreateRole.Handler>();
        services.AddScoped<CreateRole.Validator>();
        services.AddScoped<ICommandHandler<UpdateRole.Command, UpdateRole.Response>, UpdateRole.Handler>();
        services.AddScoped<UpdateRole.Validator>();
        services.AddScoped<ICommandHandler<DeleteRole.Command, DeleteRole.Response>, DeleteRole.Handler>();
        services.AddScoped<DeleteRole.Validator>();

        // Register Application Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
