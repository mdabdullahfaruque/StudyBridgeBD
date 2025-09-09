using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class GoogleLogin
{
    public class Command : ICommand<Response>
    {
        public string IdToken { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string GoogleSub { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Google ID token is required");
                
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Valid email is required");
                
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required");
                
            RuleFor(x => x.GoogleSub)
                .NotEmpty().WithMessage("Google subject (sub) is required");
        }
    }

    public class Response
    {
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public List<string> Roles { get; init; } = new();
        public bool IsNewUser { get; init; }
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IApplicationDbContext context,
            IJwtTokenService jwtTokenService,
            IPermissionService permissionService,
            ILogger<Handler> logger)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            try
            {
                // In a real implementation, you would verify the Google ID token here
                // For now, we trust the command data that should come from token verification
                
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == command.Email || u.GoogleSub == command.GoogleSub, cancellationToken);

                AppUser user;
                bool isNewUser = false;

                if (existingUser == null)
                {
                    // Create new user
                    user = new AppUser
                    {
                        Email = command.Email,
                        DisplayName = command.DisplayName,
                        FirstName = command.FirstName,
                        LastName = command.LastName,
                        GoogleSub = command.GoogleSub,
                        AvatarUrl = command.AvatarUrl,
                        EmailConfirmed = true, // Google accounts are pre-verified
                        IsActive = true,
                        LoginProvider = LoginProvider.Google,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Assign default User role
                    await _permissionService.AssignRoleToUserAsync(
                        user.Id.ToString(), 
                        SystemRole.User, 
                        "Google OAuth");

                    isNewUser = true;
                    _logger.LogInformation("New Google user created: {Email} with GoogleSub: {GoogleSub}", command.Email, command.GoogleSub);
                }
                else
                {
                    user = existingUser;
                    
                    // Update user information from Google (in case profile data changed)
                    user.DisplayName = command.DisplayName;
                    user.FirstName = command.FirstName;
                    user.LastName = command.LastName;
                    user.AvatarUrl = command.AvatarUrl;
                    user.LastLoginAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                    
                    // Ensure Google users have correct LoginProvider and GoogleSub
                    if (user.LoginProvider != LoginProvider.Google)
                    {
                        user.LoginProvider = LoginProvider.Google;
                    }
                    if (string.IsNullOrEmpty(user.GoogleSub))
                    {
                        user.GoogleSub = command.GoogleSub;
                    }
                    
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("Existing Google user login: {Email} with GoogleSub: {GoogleSub}", command.Email, command.GoogleSub);
                }

                // Get user roles
                var roles = await _permissionService.GetUserRolesAsync(user.Id.ToString());
                var roleStrings = roles.Select(r => r.ToString()).ToList();

                // Generate JWT token
                var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, roleStrings);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                _logger.LogInformation("Google login successful for user: {Email}", command.Email);

                return new Response
                {
                    Token = token,
                    RefreshToken = string.Empty, // TODO: Implement refresh token
                    ExpiresAt = expiresAt,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    UserId = user.Id.ToString(),
                    Roles = roleStrings,
                    IsNewUser = isNewUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login with token: {IdToken}", command.IdToken);
                throw;
            }
        }
    }
}
