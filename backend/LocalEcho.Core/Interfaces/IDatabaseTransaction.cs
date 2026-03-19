namespace LocalEcho.Core.Interfaces;

public interface IDatabaseTransaction : IDisposable, IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}