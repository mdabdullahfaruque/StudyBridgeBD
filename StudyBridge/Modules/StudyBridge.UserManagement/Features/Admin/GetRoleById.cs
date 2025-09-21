using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class GetRoleById
{
    public class Query : IQuery<Response>
    {
        public string Id { get; set; } = string.Empty;
        public bool IncludeUsers { get; set; } = false;
    }

    public class Validator : AbstractValidator<Query>
    {
        private readonly IApplicationDbContext _context;

        public Validator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Role ID is required")
                .MustAsync(BeValidRoleId).WithMessage("Role not found");
        }

        private async Task<bool> BeValidRoleId(string id, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(id, out var roleId))
                return false;

            return await _context.Roles
                .AnyAsync(r => r.Id == roleId, cancellationToken);
        }
    }

    public class Response
    {
        public RoleDto Role { get; set; } = new();
        public string Message { get; set; } = "Role retrieved successfully";
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<MenuDto> Menus { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
        public int UserCount { get; set; }
    }

    public class MenuDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
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
                _logger.LogInformation("Getting role by ID: {RoleId}", request.Id);

                if (!Guid.TryParse(request.Id, out var roleId))
                {
                    throw new ArgumentException("Invalid role ID format");
                }

                var query = _context.Roles
                    .Include(r => r.RoleMenus.Where(rm => rm.IsActive))
                        .ThenInclude(rm => rm.Menu)
                    .AsQueryable();

                if (request.IncludeUsers)
                {
                    query = query.Include(r => r.UserRoles)
                        .ThenInclude(ur => ur.User);
                }

                var role = await query
                    .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

                if (role == null)
                {
                    throw new InvalidOperationException($"Role with ID {request.Id} not found");
                }

                var roleDto = new RoleDto
                {
                    Id = role.Id.ToString(),
                    Name = role.Name,
                    Description = role.Description,
                    IsActive = role.IsActive,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt,
                    UserCount = role.UserRoles.Count(ur => ur.User.IsActive),
                    Menus = role.RoleMenus
                        .Where(rm => rm.IsActive && rm.Menu.IsActive)
                        .Select(rm => new MenuDto
                        {
                            Id = rm.Menu.Id.ToString(),
                            Name = rm.Menu.Name,
                            DisplayName = rm.Menu.DisplayName,
                            Description = rm.Menu.Description ?? string.Empty,
                            IsActive = rm.Menu.IsActive
                        }).ToList(),
                    Users = request.IncludeUsers 
                        ? role.UserRoles
                            .Where(ur => ur.User.IsActive)
                            .Select(ur => new UserDto
                            {
                                Id = ur.User.Id.ToString(),
                                Email = ur.User.Email,
                                DisplayName = ur.User.DisplayName,
                                IsActive = ur.User.IsActive,
                                AssignedAt = ur.AssignedAt
                            }).ToList()
                        : new List<UserDto>()
                };

                _logger.LogInformation("Retrieved role {RoleName} with {MenuCount} menus and {UserCount} users", 
                    role.Name, roleDto.Menus.Count, roleDto.UserCount);

                return new Response { Role = roleDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role: {RoleId}", request.Id);
                throw;
            }
        }
    }
}