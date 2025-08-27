using StudyBridge.UserManagement.Domain.Entities;

namespace StudyBridge.UserManagement.Domain.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByGoogleSubAsync(string googleSub, CancellationToken cancellationToken = default);
    Task<AppUser> CreateAsync(AppUser user, CancellationToken cancellationToken = default);
    Task<AppUser> UpdateAsync(AppUser user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
}
