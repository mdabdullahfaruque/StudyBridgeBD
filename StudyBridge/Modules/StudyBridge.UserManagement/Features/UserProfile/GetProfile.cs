using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.UserProfile;

public static class GetProfile
{
    public class Query : IQuery<Response>
    {
        public string UserId { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }

    public class Response
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? LastLoginAt { get; init; }
        public bool IsActive { get; init; }
        public bool EmailConfirmed { get; init; }
    }

    public class Handler : IQueryHandler<Query, Response>
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

        public async Task<Response> HandleAsync(Query query, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(query.UserId);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User profile requested for non-existent user: {UserId}", query.UserId);
                throw new KeyNotFoundException($"User with ID {query.UserId} not found");
            }

            return new Response
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            };
        }
    }
}
