using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using System.Security.Claims;

namespace StudyBridge.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permissionKey;

    public RequirePermissionAttribute(string permissionKey)
    {
        _permissionKey = permissionKey;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        var hasPermission = await permissionService.HasPermissionAsync(userId, _permissionKey);
        
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly SystemRole[] _roles;

    public RequireRoleAttribute(params SystemRole[] roles)
    {
        _roles = roles;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>();

        var userRoles = await permissionService.GetUserRolesAsync(userId);
        
        var hasRole = _roles.Any(requiredRole => userRoles.Contains(requiredRole));
        
        if (!hasRole)
        {
            context.Result = new ForbidResult();
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireSubscriptionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly SubscriptionType _requiredType;
    private readonly bool _hasSpecificType;

    public RequireSubscriptionAttribute()
    {
        _hasSpecificType = false;
        _requiredType = SubscriptionType.Free;
    }

    public RequireSubscriptionAttribute(SubscriptionType requiredType)
    {
        _requiredType = requiredType;
        _hasSpecificType = true;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var subscriptionService = context.HttpContext.RequestServices
            .GetRequiredService<ISubscriptionService>();

        var hasActiveSubscription = await subscriptionService.IsSubscriptionActiveAsync(
            userId, _hasSpecificType ? _requiredType : null);
        
        if (!hasActiveSubscription)
        {
            context.Result = new ObjectResult(new { message = "Active subscription required" })
            {
                StatusCode = 402 // Payment Required
            };
        }
    }
}
