using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Application.Contracts.Services;
using StudyBridge.Shared.CQRS;
using System.ComponentModel.DataAnnotations;

namespace StudyBridge.UserManagement.Features.Authentication;

public static class ChangePassword
{
    public class Request
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
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

    public class Command : ICommand<Response>
    {
        public string UserId { get; init; } = string.Empty;
        public string CurrentPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;

        public Command(string userId, Request request)
        {
            UserId = userId;
            CurrentPassword = request.CurrentPassword;
            NewPassword = request.NewPassword;
        }
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

        public async Task<Response> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = Guid.Parse(request.UserId);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("Password change attempt for invalid user: {UserId}", request.UserId);
                    return new Response
                    {
                        Success = false,
                        Message = "User not found or not a local user"
                    };
                }

                // Verify current password
                if (!_passwordHashingService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Password change attempt with incorrect current password: {UserId}", request.UserId);
                    return new Response
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                // Hash new password
                var newPasswordHash = _passwordHashingService.HashPassword(request.NewPassword);

                // Update user
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);
                
                return new Response
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", request.UserId);
                return new Response
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                };
            }
        }
    }
}
