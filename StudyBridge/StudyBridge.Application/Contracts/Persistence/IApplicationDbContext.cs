using Microsoft.EntityFrameworkCore;
using StudyBridge.Domain.Entities;

namespace StudyBridge.Application.Contracts.Persistence;

public interface IApplicationDbContext
{
    DbSet<AppUser> Users { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
