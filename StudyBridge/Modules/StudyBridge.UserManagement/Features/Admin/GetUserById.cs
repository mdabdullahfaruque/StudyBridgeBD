using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class GetUserById
{
    public class Query : IQuery<Response>
    {
        public Guid UserId { get; set; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");
        }
    }

    public class Response
    {
        public GetUsers.UserDto? User { get; set; }
        public string Message { get; set; } = "User retrieved successfully";
    }

    public class Handler : IQueryHandler<Query, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(IApplicationDbContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Query request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting user by ID: {UserId}", request.UserId);

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Include(u => u.UserSubscriptions)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return new Response 
                    { 
                        User = null, 
                        Message = "User not found" 
                    };
                }

                var userDto = new GetUsers.UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    Subscriptions = user.UserSubscriptions
                        .Where(s => s.IsActive)
                        .Select(s => new GetUsers.UserSubscriptionDto
                        {
                            Id = s.Id.ToString(),
                            Plan = s.SubscriptionType.ToString(),
                            Status = s.Status.ToString(),
                            StartDate = s.StartDate,
                            EndDate = s.EndDate,
                            IsActive = s.IsActive
                        }).ToList()
                };

                _logger.LogInformation("Successfully retrieved user: {Email}", user.Email);

                return new Response { User = userDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", request.UserId);
                throw;
            }
        }
    }
}