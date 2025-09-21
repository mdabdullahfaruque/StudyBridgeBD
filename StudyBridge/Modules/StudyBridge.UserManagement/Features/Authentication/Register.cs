using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Domain.Entities;
using StudyBridge.Domain.Enums;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class Register
{
    public class Command : ICommand<Response>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
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
        public bool IsPublicUser { get; init; } = true;
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IJwtTokenService _tokenService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IApplicationDbContext context,
            IPasswordHasher<AppUser> passwordHasher,
            IJwtTokenService tokenService,
            ILogger<Handler> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", command.Email);
                throw new InvalidOperationException("User with this email already exists");
            }

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(new AppUser(), command.Password);

            // Create user
            var user = new AppUser
            {
                Email = command.Email,
                DisplayName = command.DisplayName,
                FirstName = command.FirstName,
                LastName = command.LastName,
                PasswordHash = passwordHash,
                EmailConfirmed = false,
                IsActive = true,
                LoginProvider = LoginProvider.Local,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Assign default User role (find a role named "User" or create logic as needed)
            var userRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "User" && r.IsActive, cancellationToken);
            
            if (userRole != null)
            {
                var assignment = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = userRole.Id,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = "System"
                };
                _context.UserRoles.Add(assignment);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Generate JWT token
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id && ur.IsActive && ur.Role.IsActive)
                .Select(ur => ur.Role.Name)
                .ToListAsync(cancellationToken);
            var token = _tokenService.GenerateToken(user.Id, user.Email, userRoles);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Assuming 24-hour token validity

            _logger.LogInformation("User registered successfully: {Email}", command.Email);

            return new Response
            {
                Token = token,
                RefreshToken = string.Empty, // TODO: Implement refresh token
                ExpiresAt = expiresAt,
                Email = user.Email,
                DisplayName = user.DisplayName,
                UserId = user.Id.ToString(),
                Roles = userRoles,
                RequiresEmailConfirmation = !user.EmailConfirmed,
                IsPublicUser = user.IsPublicUser
            };
        }
    }
}
