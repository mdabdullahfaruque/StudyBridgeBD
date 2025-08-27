using Microsoft.EntityFrameworkCore;
using StudyBridge.UserManagement.Domain.Entities;
using StudyBridge.UserManagement.Domain.Repositories;

namespace StudyBridge.UserManagement.Infrastructure;

public class UserRepository<TContext> : IUserRepository 
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly DbSet<AppUser> _users;

    public UserRepository(TContext context)
    {
        _context = context;
        _users = context.Set<AppUser>();
    }

    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<AppUser?> GetByGoogleSubAsync(string googleSub, CancellationToken cancellationToken = default)
    {
        return await _users.FirstOrDefaultAsync(u => u.GoogleSub == googleSub, cancellationToken);
    }

    public async Task<AppUser> CreateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        _users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<AppUser> UpdateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        _users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}
