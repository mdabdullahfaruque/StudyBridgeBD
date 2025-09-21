using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class GetRoles
{
    public class Query : IQuery<Response>
    {
        public bool IncludeInactive { get; set; } = false;
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            // No specific validation needed for this simple query
        }
    }

    public class Response
    {
        public List<RoleDto> Roles { get; set; } = new();
        public string Message { get; set; } = "Roles retrieved successfully";
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MenuDto> Menus { get; set; } = new();
        public int UserCount { get; set; }
    }

    public class MenuDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
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
                _logger.LogInformation("Getting roles, includeInactive: {IncludeInactive}", request.IncludeInactive);

                var query = _context.Roles
                    .Include(r => r.RoleMenus.Where(rm => rm.IsActive))
                        .ThenInclude(rm => rm.Menu)
                    .AsQueryable();

                if (!request.IncludeInactive)
                {
                    query = query.Where(r => r.IsActive);
                }

                var roles = await query
                    .OrderBy(r => r.Name)
                    .Select(r => new RoleDto
                    {
                        Id = r.Id.ToString(),
                        Name = r.Name,
                        Description = r.Description ?? string.Empty,
                        IsActive = r.IsActive,
                        CreatedAt = r.CreatedAt,
                        UserCount = r.UserRoles.Count(ur => ur.User.IsActive),
                        Menus = r.RoleMenus
                            .Where(rm => rm.IsActive && rm.Menu.IsActive)
                            .Select(rm => new MenuDto
                            {
                                Id = rm.Menu.Id.ToString(),
                                Name = rm.Menu.Name,
                                DisplayName = rm.Menu.DisplayName,
                                Description = rm.Menu.Description ?? string.Empty,
                                IsGranted = rm.IsActive
                            }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} roles", roles.Count);

                return new Response { Roles = roles };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                throw;
            }
        }
    }
}