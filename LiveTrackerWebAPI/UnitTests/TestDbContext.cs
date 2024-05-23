using DB.Database;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnitTests;

public class TestDbContext : RaceTrackerDbContext
{
    public TestDbContext(DbContextOptions<RaceTrackerDbContext> options, IConfiguration configuration)
        : base(options, configuration)
    {
    }

    /// <summary>
    /// In memory database does not support transaction, must be override
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new TestDbContextTransaction());
    }
}

public class TestDbContextTransaction : IDbContextTransaction
{
    public Guid TransactionId => Guid.NewGuid();

    public void Dispose() { }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void Commit() { }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Rollback() { }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}