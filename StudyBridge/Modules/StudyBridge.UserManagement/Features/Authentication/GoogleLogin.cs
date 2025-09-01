using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.CQRS;
using System.ComponentModel.DataAnnotations;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class GoogleLogin
{
    public class Request
    {
        [Required(ErrorMessage = "Google ID token is required")]
        public string IdToken { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
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

    public class Command : ICommand<Response>
    {
        public string IdToken { get; init; } = string.Empty;

        public Command(Request request)
        {
            IdToken = request.IdToken;
        }
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

        public async Task<Response> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            // TODO: Implement Google token validation
            // For now, this is a placeholder implementation
            // You would typically validate the Google ID token here using Google's client library
            
            // Extract email from token (this would be done after proper token validation)
            var email = "extracted-from-google-token@example.com"; // Placeholder
            var name = "Google User"; // Placeholder
            var googleSub = "google-sub-id"; // Placeholder

            // Check if user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            bool isNewUser = false;

            if (user == null)
            {
                // Create new user
                user = new AppUser
                {
                    Email = email,
                    DisplayName = name,
                    FirstName = name.Split(' ').FirstOrDefault() ?? "",
                    LastName = name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                    GoogleSub = googleSub,
                    EmailConfirmed = true, // Google emails are pre-verified
                    IsActive = true,
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
                    "Google");

                isNewUser = true;
                _logger.LogInformation("New Google user created: {Email}", email);
            }
            else
            {
                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                user.GoogleSub ??= googleSub; // Set Google sub if not already set
                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Get user roles
            var roles = await _permissionService.GetUserRolesAsync(user.Id.ToString());
            var roleStrings = roles.Select(r => r.ToString()).ToList();

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, roleStrings);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Assuming 24-hour token validity

            _logger.LogInformation("Google user logged in successfully: {Email}", email);

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
    }
}
