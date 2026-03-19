using LocalEcho.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace LocalEcho.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public async Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(ct);
        return new EfTransactionWrapper(transaction);
    }

    private class EfTransactionWrapper : IDatabaseTransaction
    {
        private readonly IDbContextTransaction _transaction;
        public EfTransactionWrapper(IDbContextTransaction transaction) => _transaction = transaction;
        public Task CommitAsync(CancellationToken ct = default) => _transaction.CommitAsync(ct);
        public Task RollbackAsync(CancellationToken ct = default) => _transaction.RollbackAsync(ct);
        public void Dispose() => _transaction.Dispose();
        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}