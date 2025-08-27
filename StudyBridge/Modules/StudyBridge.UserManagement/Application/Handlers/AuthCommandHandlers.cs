using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Application.Commands;
using StudyBridge.UserManagement.Application.DTOs;

namespace StudyBridge.UserManagement.Application.Handlers;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        IPermissionService permissionService,
        ILogger<RegisterCommandHandler> logger)
    {
        _context = context;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<LoginResponse> HandleAsync(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password
        var passwordHash = _passwordHashingService.HashPassword(request.Password);

        // Create user
        var user = new AppUser
        {
            Email = request.Email,
            DisplayName = request.DisplayName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHash,
            EmailConfirmed = false,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign default User role
        await _permissionService.AssignRoleToUserAsync(
            user.Id.ToString(), 
            SystemRole.User, 
            "System");

        // Generate JWT token
        var roles = await _permissionService.GetUserRolesAsync(user.Id.ToString());
        var roleStrings = roles.Select(r => r.ToString()).ToList();
        var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, roleStrings);

        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        return new LoginResponse
        {
            Token = token,
            Email = user.Email,
            DisplayName = user.DisplayName,
            UserId = user.Id.ToString(),
            Roles = roleStrings
        };
    }
}

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        IPermissionService permissionService,
        ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<LoginResponse> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Get user roles
        var roles = await _permissionService.GetUserRolesAsync(user.Id.ToString());
        var roleStrings = roles.Select(r => r.ToString()).ToList();

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, roleStrings);

        _logger.LogInformation("User logged in successfully: {Email}", request.Email);

        return new LoginResponse
        {
            Token = token,
            Email = user.Email,
            DisplayName = user.DisplayName,
            UserId = user.Id.ToString(),
            Roles = roleStrings
        };
    }
}

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHashingService passwordHashingService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _context = context;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("User not found or not a local user");
        }

        // Verify current password
        if (!_passwordHashingService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // Hash new password
        var newPasswordHash = _passwordHashingService.HashPassword(request.NewPassword);

        // Update user
        user.PasswordHash = newPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);
        return true;
    }
}
