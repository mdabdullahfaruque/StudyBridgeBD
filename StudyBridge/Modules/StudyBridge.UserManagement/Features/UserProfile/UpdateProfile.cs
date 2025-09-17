using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;
using StudyBridge.Shared.Exceptions;

namespace StudyBridge.UserManagement.Features.UserProfile;

public static class UpdateProfile
{
    public class Request
    {
        public string DisplayName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required");

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
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Profile update attempt for non-existent user: {UserId}", request.UserId);
                throw new NotFoundException("User not found");
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
    }
}
