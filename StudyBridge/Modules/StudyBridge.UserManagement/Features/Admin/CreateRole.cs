using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class CreateRole
{
    public class Command : ICommand<Response>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<string> MenuIds { get; set; } = new();
    }

    public class Validator : AbstractValidator<Command>
    {
        private readonly IApplicationDbContext _context;

        public Validator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MinimumLength(2).WithMessage("Role name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters")
                .MustAsync(BeUniqueRoleName).WithMessage("Role name already exists");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleForEach(x => x.MenuIds)
                .MustAsync(BeValidMenuId).WithMessage("Invalid menu ID");
        }

        private async Task<bool> BeUniqueRoleName(string name, CancellationToken cancellationToken)
        {
            return !await _context.Roles
                .AnyAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        private async Task<bool> BeValidMenuId(string menuId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(menuId, out var id))
                return false;

            return await _context.Menus
                .AnyAsync(m => m.Id == id && m.IsActive, cancellationToken);
        }
    }

    public class Response
    {
        public RoleDto Role { get; set; } = new();
        public string Message { get; set; } = "Role created successfully";
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MenuDto> Menus { get; set; } = new();
    }

    public class MenuDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Route { get; set; }
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(IApplicationDbContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new role: {RoleName}", request.Name);

                // Create the role entity
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Roles.Add(role);

                // Add role menus
                if (request.MenuIds.Any())
                {
                    var validMenuIds = request.MenuIds
                        .Where(id => Guid.TryParse(id, out _))
                        .Select(id => Guid.Parse(id))
                        .ToList();

                    var existingMenus = await _context.Menus
                        .Where(m => validMenuIds.Contains(m.Id) && m.IsActive)
                        .ToListAsync(cancellationToken);

                    foreach (var menu in existingMenus)
                    {
                        var roleMenu = new RoleMenu
                        {
                            Id = Guid.NewGuid(),
                            RoleId = role.Id,
                            MenuId = menu.Id,
                            IsActive = true,
                            GrantedAt = DateTime.UtcNow,
                            GrantedBy = "System" // TODO: Get current user ID from context
                        };

                        _context.RoleMenus.Add(roleMenu);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Load the created role with menus for response
                var createdRole = await _context.Roles
                    .Include(r => r.RoleMenus.Where(rm => rm.IsActive))
                        .ThenInclude(rm => rm.Menu)
                    .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);

                if (createdRole == null)
                {
                    throw new InvalidOperationException("Failed to retrieve created role");
                }

                var response = new Response
                {
                    Role = new RoleDto
                    {
                        Id = createdRole.Id.ToString(),
                        Name = createdRole.Name,
                        Description = createdRole.Description,
                        IsActive = createdRole.IsActive,
                        CreatedAt = createdRole.CreatedAt,
                        Menus = createdRole.RoleMenus
                            .Where(rm => rm.IsActive && rm.Menu.IsActive)
                            .Select(rm => new MenuDto
                            {
                                Id = rm.Menu.Id.ToString(),
                                Name = rm.Menu.Name,
                                DisplayName = rm.Menu.DisplayName,
                                Description = rm.Menu.Description ?? string.Empty,
                                Icon = rm.Menu.Icon,
                                Route = rm.Menu.Route
                            }).ToList()
                    }
                };

                _logger.LogInformation("Successfully created role {RoleName} with ID {RoleId}", 
                    createdRole.Name, createdRole.Id);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", request.Name);
                throw;
            }
        }
    }
}