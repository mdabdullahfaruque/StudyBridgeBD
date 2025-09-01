using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.CQRS;
using System.ComponentModel.DataAnnotations;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class Register
{
    public class Request
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "First name is required")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
        public string FirstName { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
        public string LastName { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Display name is required")]
        public string DisplayName { get; init; } = string.Empty;
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
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters long");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters long");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required");
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
        public bool RequiresEmailConfirmation { get; init; }
    }

    public class Command : ICommand<Response>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;

        public Command(Request request)
        {
            Email = request.Email;
            Password = request.Password;
            FirstName = request.FirstName;
            LastName = request.LastName;
            DisplayName = request.DisplayName;
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
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
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
                "System");

            // Generate JWT token
            var roles = await _permissionService.GetUserRolesAsync(user.Id.ToString());
            var roleStrings = roles.Select(r => r.ToString()).ToList();
            var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Email, roleStrings);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Assuming 24-hour token validity

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return new Response
            {
                Token = token,
                RefreshToken = string.Empty, // TODO: Implement refresh token
                ExpiresAt = expiresAt,
                Email = user.Email,
                DisplayName = user.DisplayName,
                UserId = user.Id.ToString(),
                Roles = roleStrings,
                RequiresEmailConfirmation = !user.EmailConfirmed
            };
        }
    }
}
