using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class ChangePassword
{
    public class Command : ICommand<Response>
    {
        public string UserId { get; init; } = string.Empty;
        public string CurrentPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long");
        }
    }

    public class Response
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IApplicationDbContext context,
            IPasswordHashingService passwordHashingService,
            ILogger<Handler> logger)
        {
            _context = context;
            _passwordHashingService = passwordHashingService;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            try
            {
                var userId = Guid.Parse(command.UserId);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("Password change attempt for invalid user: {UserId}", command.UserId);
                    throw new UnauthorizedAccessException("User not found");
                }

                // Check if user has a password (not OAuth only)
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    throw new InvalidOperationException("Cannot change password for OAuth-only users");
                }

                // Verify current password
                if (!_passwordHashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Password change attempt with incorrect current password: {UserId}", command.UserId);
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }

                // Hash new password
                var newPasswordHash = _passwordHashingService.HashPassword(command.NewPassword);

                // Update user
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password changed successfully for user: {UserId}", command.UserId);

                return new Response
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw authorization exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw operation exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", command.UserId);
                return new Response
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                };
            }
        }
    }
}
