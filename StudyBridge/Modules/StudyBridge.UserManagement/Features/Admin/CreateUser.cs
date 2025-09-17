using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class CreateUser
{
    public class Command : ICommand<Response>
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Password { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Valid email address is required")
                .MaximumLength(255)
                .WithMessage("Email must not exceed 255 characters");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("Display name is required")
                .MinimumLength(2)
                .WithMessage("Display name must be at least 2 characters")
                .MaximumLength(100)
                .WithMessage("Display name must not exceed 100 characters");

            RuleFor(x => x.FirstName)
                .MaximumLength(50)
                .WithMessage("First name must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(50)
                .WithMessage("Last name must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character");

            RuleFor(x => x.Roles)
                .NotNull()
                .WithMessage("Roles list cannot be null");
        }
    }

    public class Response
    {
        public GetUsers.UserDto? User { get; set; }
        public string Message { get; set; } = "User created successfully";
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IApplicationDbContext context,
            IPasswordHasher<AppUser> passwordHasher,
            ILogger<Handler> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new user: {Email}", request.Email);

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

                if (existingUser != null)
                {
                    _logger.LogWarning("User already exists: {Email}", request.Email);
                    throw new InvalidOperationException("A user with this email address already exists");
                }

                // Create new user
                var user = new AppUser
                {
                    Email = request.Email.ToLower(),
                    DisplayName = request.DisplayName,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Hash password
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);

                // Assign roles if provided
                if (request.Roles.Any())
                {
                    await AssignRoles(user.Id, request.Roles, cancellationToken);
                }

                // Reload user with roles
                var createdUser = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Include(u => u.UserSubscriptions)
                    .FirstAsync(u => u.Id == user.Id, cancellationToken);

                var userDto = new GetUsers.UserDto
                {
                    Id = createdUser.Id.ToString(),
                    Email = createdUser.Email,
                    DisplayName = createdUser.DisplayName,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    IsActive = createdUser.IsActive,
                    EmailConfirmed = createdUser.EmailConfirmed,
                    CreatedAt = createdUser.CreatedAt,
                    LastLoginAt = createdUser.LastLoginAt,
                    Roles = createdUser.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    Subscriptions = new List<GetUsers.UserSubscriptionDto>()
                };

                _logger.LogInformation("Successfully created user: {UserId}", user.Id);

                return new Response { User = userDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", request.Email);
                throw;
            }
        }

        private async Task AssignRoles(Guid userId, List<string> roleNames, CancellationToken cancellationToken)
        {
            var roles = await _context.Roles
                .Where(r => roleNames.Contains(r.Name) && r.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var role in roles)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = "System" // Assigned during user creation
                };

                _context.UserRoles.Add(userRole);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}