using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;
using System.ComponentModel.DataAnnotations;

namespace StudyBridge.UserManagement.Features.UserProfile;

public static class UpdateProfile
{
    public class Request
    {
        [Required(ErrorMessage = "Display name is required")]
        [MinLength(2, ErrorMessage = "Display name must be at least 2 characters long")]
        public string DisplayName { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "First name is required")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
        public string FirstName { get; init; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
        public string LastName { get; init; } = string.Empty;
        
        public string? AvatarUrl { get; init; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MinimumLength(2).WithMessage("Display name must be at least 2 characters long");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters long");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters long");

            RuleFor(x => x.AvatarUrl)
                .Must(BeValidUrl).WithMessage("Avatar URL must be a valid URL")
                .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
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
        public string DisplayName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }

        public Command(string userId, Request request)
        {
            UserId = userId;
            DisplayName = request.DisplayName;
            FirstName = request.FirstName;
            LastName = request.LastName;
            AvatarUrl = request.AvatarUrl;
        }
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IApplicationDbContext context,
            ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = Guid.Parse(request.UserId);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("Profile update attempt for non-existent user: {UserId}", request.UserId);
                    return new Response
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Update user profile
                user.DisplayName = request.DisplayName;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.AvatarUrl = request.AvatarUrl;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User profile updated successfully: {UserId}", request.UserId);
                
                return new Response
                {
                    Success = true,
                    Message = "Profile updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {UserId}", request.UserId);
                return new Response
                {
                    Success = false,
                    Message = "An error occurred while updating profile"
                };
            }
        }
    }
}
