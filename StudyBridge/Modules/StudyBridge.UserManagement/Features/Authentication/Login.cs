using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Shared.CQRS;
using System.ComponentModel.DataAnnotations;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class Login
{
    public class Request
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Please enter a valid email address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(1).WithMessage("Password cannot be empty");
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
    }

    public class Command : ICommand<Response>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;

        public Command(Request request)
        {
            Email = request.Email;
            Password = request.Password;
        }
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IApplicationDbContext context,
            IPasswordHashingService passwordHashingService,
            IJwtTokenService jwtTokenService,
            IPermissionService permissionService,
            ILogger<Handler> logger)
        {
            _context = context;
            _passwordHashingService = passwordHashingService;
            _jwtTokenService = jwtTokenService;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email} - User not found or invalid credentials", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password
            if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email} - Invalid password", request.Email);
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
            var expiresAt = DateTime.UtcNow.AddHours(24); // Assuming 24-hour token validity

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return new Response
            {
                Token = token,
                RefreshToken = string.Empty, // TODO: Implement refresh token
                ExpiresAt = expiresAt,
                Email = user.Email,
                DisplayName = user.DisplayName,
                UserId = user.Id.ToString(),
                Roles = roleStrings
            };
        }
    }
}
