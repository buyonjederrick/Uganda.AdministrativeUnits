using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Uganda.AdministrativeUnits.Application.Abstractions;

/// <summary>
/// Thin EF Core access abstraction used by application services (GovPay-style IDbRepository).
/// </summary>
public interface IDbRepository
{
    DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class;

    Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void ClearChangeTracker();

    DbContext GetDbContext();
}
