using StudyBridge.Application.Contracts.Persistence;
using StudyBridge.Domain.Entities;
using StudyBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace StudyBridge.Infrastructure.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _context;

    public MenuRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Menu?> GetByIdAsync(Guid id)
    {
        return await _context.Menus
            .Include(m => m.ParentMenu)
            .Include(m => m.SubMenus)
            .Include(m => m.Permissions)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Menu?> GetByNameAsync(string name)
    {
        return await _context.Menus
            .Include(m => m.ParentMenu)
            .Include(m => m.SubMenus)
            .Include(m => m.Permissions)
            .FirstOrDefaultAsync(m => m.Name == name);
    }

    public async Task<IEnumerable<Menu>> GetAllAsync()
    {
        return await _context.Menus
            .Include(m => m.ParentMenu)
            .Include(m => m.SubMenus)
            .Include(m => m.Permissions)
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Menu>> GetByParentIdAsync(Guid? parentId)
    {
        return await _context.Menus
            .Include(m => m.SubMenus)
            .Include(m => m.Permissions)
            .Where(m => m.ParentMenuId == parentId && m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Menu>> GetMenuTreeAsync()
    {
        // Get root menus (no parent) with their full hierarchy
        return await _context.Menus
            .Include(m => m.SubMenus.Where(sm => sm.IsActive))
                .ThenInclude(sm => sm.SubMenus.Where(ssm => ssm.IsActive))
            .Include(m => m.Permissions.Where(p => p.IsActive))
            .Where(m => m.ParentMenuId == null && m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Menu>> GetUserMenusAsync(string userId)
    {
        // Get menus that the user has permission to access
        var userPermissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
                .ThenInclude(p => p.Menu)
            .Include(rp => rp.Role)
                .ThenInclude(r => r.UserRoles.Where(ur => ur.UserId == userId && ur.IsActive))
            .Where(rp => rp.IsGranted && 
                        rp.Role.UserRoles.Any(ur => ur.UserId == userId && ur.IsActive) &&
                        rp.Permission.IsActive &&
                        rp.Permission.Menu.IsActive)
            .Select(rp => rp.Permission.Menu)
            .Distinct()
            .ToListAsync();

        return userPermissions;
    }

    public async Task<Menu> AddAsync(Menu menu)
    {
        menu.CreatedAt = DateTime.UtcNow;
        menu.UpdatedAt = DateTime.UtcNow;
        
        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();
        return menu;
    }

    public async Task UpdateAsync(Menu menu)
    {
        menu.UpdatedAt = DateTime.UtcNow;
        
        _context.Menus.Update(menu);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var menu = await _context.Menus.FindAsync(id);
        if (menu != null)
        {
            menu.IsActive = false;
            menu.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
    {
        return await _context.Permissions
            .Include(p => p.Menu)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Permission?> GetByKeyAsync(string permissionKey)
    {
        return await _context.Permissions
            .Include(p => p.Menu)
            .FirstOrDefaultAsync(p => p.PermissionKey == permissionKey && p.IsActive);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .Include(p => p.Menu)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetByMenuIdAsync(Guid menuId)
    {
        return await _context.Permissions
            .Include(p => p.Menu)
            .Where(p => p.MenuId == menuId && p.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
                .ThenInclude(p => p.Menu)
            .Include(rp => rp.Role)
                .ThenInclude(r => r.UserRoles.Where(ur => ur.UserId == userId && ur.IsActive))
            .Where(rp => rp.IsGranted && 
                        rp.Role.UserRoles.Any(ur => ur.UserId == userId && ur.IsActive) &&
                        rp.Permission.IsActive)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Permission> AddAsync(Permission permission)
    {
        permission.CreatedAt = DateTime.UtcNow;
        permission.UpdatedAt = DateTime.UtcNow;
        
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    public async Task UpdateAsync(Permission permission)
    {
        permission.UpdatedAt = DateTime.UtcNow;
        
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission != null)
        {
            permission.IsActive = false;
            permission.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}