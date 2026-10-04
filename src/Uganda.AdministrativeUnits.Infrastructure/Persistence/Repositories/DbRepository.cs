using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Uganda.AdministrativeUnits.Application.Abstractions;

namespace Uganda.AdministrativeUnits.Infrastructure.Persistence.Repositories;

public sealed class DbRepository : IDbRepository
{
    private readonly AdministrativeUnitsDbContext _dbContext;

    public DbRepository(AdministrativeUnitsDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class => _dbContext.Set<TEntity>();

    public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : class =>
        _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public void ClearChangeTracker() => _dbContext.ChangeTracker.Clear();

    public DbContext GetDbContext() => _dbContext;
}
