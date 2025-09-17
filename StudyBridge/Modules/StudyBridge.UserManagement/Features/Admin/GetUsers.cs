using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class GetUsers
{
    public class Query : IQuery<Response>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortDirection { get; set; } = "desc";
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.SortDirection)
                .Must(x => x == null || x.ToLower() == "asc" || x.ToLower() == "desc")
                .WithMessage("Sort direction must be 'asc' or 'desc'");
        }
    }

    public class Response
    {
        public PaginatedResult<UserDto> Users { get; set; } = new();
        public string Message { get; set; } = "Users retrieved successfully";
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<UserSubscriptionDto> Subscriptions { get; set; } = new();
    }

    public class UserSubscriptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
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
                _logger.LogInformation("Getting users with page {PageNumber}, size {PageSize}", 
                    request.PageNumber, request.PageSize);

                var query = _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .Include(u => u.UserSubscriptions.Where(s => s.IsActive))
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchLower = request.SearchTerm.ToLower();
                    query = query.Where(u => 
                        u.Email.ToLower().Contains(searchLower) ||
                        u.DisplayName.ToLower().Contains(searchLower) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(searchLower)));
                }

                if (request.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == request.IsActive.Value);
                }

                if (!string.IsNullOrEmpty(request.Role))
                {
                    query = query.Where(u => u.UserRoles.Any(ur => 
                        ur.Role.Name.ToLower() == request.Role.ToLower()));
                }

                // Apply sorting
                query = ApplySorting(query, request.SortBy, request.SortDirection);

                // Get total count before pagination
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination
                var users = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(u => new UserDto
                    {
                        Id = u.Id.ToString(),
                        Email = u.Email,
                        DisplayName = u.DisplayName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        IsActive = u.IsActive,
                        EmailConfirmed = u.EmailConfirmed,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                        Subscriptions = u.UserSubscriptions
                            .Where(s => s.IsActive)
                            .Select(s => new UserSubscriptionDto
                            {
                                Id = s.Id.ToString(),
                                Plan = s.SubscriptionType.ToString(),
                                Status = s.Status.ToString(),
                                StartDate = s.StartDate,
                                EndDate = s.EndDate,
                                IsActive = s.IsActive
                            }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                var paginatedResult = new PaginatedResult<UserDto>(
                    users, 
                    totalCount, 
                    request.PageNumber, 
                    request.PageSize);

                _logger.LogInformation("Retrieved {Count} users out of {TotalCount}", 
                    users.Count, totalCount);

                return new Response { Users = paginatedResult };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                throw;
            }
        }

        private static IQueryable<AppUser> ApplySorting(IQueryable<AppUser> query, string? sortBy, string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.ToLower() switch
            {
                "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "displayname" => isDescending ? query.OrderByDescending(u => u.DisplayName) : query.OrderBy(u => u.DisplayName),
                "firstname" => isDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => isDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "isactive" => isDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
                "lastloginat" => isDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
                _ => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };
        }
    }
}