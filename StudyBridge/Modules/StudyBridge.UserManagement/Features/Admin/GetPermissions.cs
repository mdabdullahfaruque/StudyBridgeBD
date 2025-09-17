using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Shared.CQRS;

namespace StudyBridge.UserManagement.Features.Admin;

public static class GetPermissions
{
    public class Query : IQuery<Response>
    {
        public bool IncludeInactive { get; set; } = false;
        public string? MenuId { get; set; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.MenuId)
                .Must(BeValidGuidOrEmpty)
                .WithMessage("Menu ID must be a valid GUID or empty")
                .When(x => !string.IsNullOrEmpty(x.MenuId));
        }

        private static bool BeValidGuidOrEmpty(string? value)
        {
            return string.IsNullOrEmpty(value) || Guid.TryParse(value, out _);
        }
    }

    public class Response
    {
        public List<PermissionTreeNode> PermissionTree { get; set; } = new();
        public string Message { get; set; } = "Permissions retrieved successfully";
    }

    public class PermissionTreeNode
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSystemPermission { get; set; }
        public string? ParentId { get; set; }
        public List<PermissionTreeNode> Children { get; set; } = new();
        public PermissionData? Data { get; set; }
    }

    public class PermissionData
    {
        public string MenuId { get; set; } = string.Empty;
        public string MenuName { get; set; } = string.Empty;
        public string PermissionType { get; set; } = string.Empty;
        public int SortOrder { get; set; }
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
                _logger.LogInformation("Getting permissions tree, includeInactive: {IncludeInactive}", request.IncludeInactive);

                var query = _context.Permissions
                    .Include(p => p.Menu)
                    .AsQueryable();

                if (!request.IncludeInactive)
                {
                    query = query.Where(p => p.IsActive);
                }

                if (!string.IsNullOrEmpty(request.MenuId) && Guid.TryParse(request.MenuId, out var menuId))
                {
                    query = query.Where(p => p.MenuId == menuId);
                }

                var permissions = await query
                    .OrderBy(p => p.Menu.SortOrder)
                    .ThenBy(p => p.PermissionType)
                    .ThenBy(p => p.DisplayName)
                    .ToListAsync(cancellationToken);

                // Build permission tree grouped by menu
                var permissionTree = BuildPermissionTree(permissions);

                _logger.LogInformation("Retrieved {Count} permission nodes", permissionTree.Count);

                return new Response { PermissionTree = permissionTree };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                throw;
            }
        }

        private static List<PermissionTreeNode> BuildPermissionTree(List<Domain.Entities.Permission> permissions)
        {
            var menuGroups = permissions.GroupBy(p => p.Menu).ToList();
            var tree = new List<PermissionTreeNode>();

            foreach (var menuGroup in menuGroups)
            {
                var menu = menuGroup.Key;
                var menuPermissions = menuGroup.ToList();

                var menuNode = new PermissionTreeNode
                {
                    Id = menu.Id.ToString(),
                    Key = $"menu_{menu.Id}",
                    Label = menu.DisplayName,
                    Icon = menu.Icon,
                    Type = "menu",
                    Description = menu.Description ?? string.Empty,
                    IsActive = menu.IsActive,
                    IsSystemPermission = false,
                    Children = new List<PermissionTreeNode>(),
                    Data = new PermissionData
                    {
                        MenuId = menu.Id.ToString(),
                        MenuName = menu.Name,
                        PermissionType = "Menu",
                        SortOrder = menu.SortOrder
                    }
                };

                // Group permissions by type under each menu
                var permissionTypeGroups = menuPermissions.GroupBy(p => p.PermissionType);

                foreach (var typeGroup in permissionTypeGroups)
                {
                    var permissionType = typeGroup.Key;
                    var typePermissions = typeGroup.ToList();

                    if (typePermissions.Count == 1)
                    {
                        // Single permission, add directly under menu
                        var permission = typePermissions.First();
                        menuNode.Children.Add(CreatePermissionNode(permission, menuNode.Id));
                    }
                    else
                    {
                        // Multiple permissions of same type, group under type node
                        var typeNode = new PermissionTreeNode
                        {
                            Id = $"{menu.Id}_{permissionType}",
                            Key = $"type_{menu.Id}_{permissionType}",
                            Label = $"{permissionType} Operations",
                            Type = "permission_type",
                            Description = $"{permissionType} permissions for {menu.DisplayName}",
                            IsActive = typePermissions.All(p => p.IsActive),
                            IsSystemPermission = typePermissions.Any(p => p.IsSystemPermission),
                            ParentId = menuNode.Id,
                            Children = new List<PermissionTreeNode>(),
                            Data = new PermissionData
                            {
                                MenuId = menu.Id.ToString(),
                                MenuName = menu.Name,
                                PermissionType = permissionType.ToString(),
                                SortOrder = 0
                            }
                        };

                        foreach (var permission in typePermissions)
                        {
                            typeNode.Children.Add(CreatePermissionNode(permission, typeNode.Id));
                        }

                        menuNode.Children.Add(typeNode);
                    }
                }

                tree.Add(menuNode);
            }

            return tree.OrderBy(n => n.Data?.SortOrder ?? 0).ToList();
        }

        private static PermissionTreeNode CreatePermissionNode(Domain.Entities.Permission permission, string parentId)
        {
            return new PermissionTreeNode
            {
                Id = permission.Id.ToString(),
                Key = permission.PermissionKey,
                Label = permission.DisplayName,
                Type = "permission",
                Description = permission.Description ?? string.Empty,
                IsActive = permission.IsActive,
                IsSystemPermission = permission.IsSystemPermission,
                ParentId = parentId,
                Children = new List<PermissionTreeNode>(),
                Data = new PermissionData
                {
                    MenuId = permission.MenuId.ToString(),
                    MenuName = permission.Menu.Name,
                    PermissionType = permission.PermissionType.ToString(),
                    SortOrder = 0
                }
            };
        }
    }
}