using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class GoogleLogin
{
    public class Command : ICommand<Response>
    {
        public string IdToken { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Google ID token is required");
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
                        // TODO: Verify Google ID token
            // For now, we'll implement a basic flow

            try
            {
                // In a real implementation, you would:
                // 1. Verify the Google ID token with Google's servers
                // 2. Extract user information from the token
                // 3. Check if user exists or create new user
                
                // Placeholder implementation
                var email = "user@example.com"; // Extract from token
                var displayName = "Google User"; // Extract from token
                
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

                AppUser user;
                bool isNewUser = false;

                if (existingUser == null)
                {
                    // Create new user
                    user = new AppUser
                    {
                        Email = email,
                        DisplayName = displayName,
                        FirstName = displayName.Split(' ').FirstOrDefault() ?? "",
                        LastName = displayName.Split(' ').LastOrDefault() ?? "",
                        EmailConfirmed = true, // Google accounts are pre-verified
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Assign default User role
                    await _permissionService.AssignRoleToUserAsync(
                        user.Id.ToString(), 
                        SystemRole.User, 
                        "Google OAuth");

                    isNewUser = true;
                }
                else
                {
                    user = existingUser;
                    user.LastLoginAt = DateTime.UtcNow;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Get user roles
                var roles = await _permissionService.GetUserRolesAsync(user.Id.ToString());
                var roleStrings = roles.Select(r => r.ToString()).ToList();

                // Generate JWT token
                var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, roleStrings);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                _logger.LogInformation("Google login successful for user: {Email}", email);

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
