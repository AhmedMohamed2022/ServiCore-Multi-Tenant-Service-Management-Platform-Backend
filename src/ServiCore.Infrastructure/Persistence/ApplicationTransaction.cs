using Microsoft.EntityFrameworkCore.Storage;
using ServiCore.Application.Common.Interfaces;

namespace ServiCore.Infrastructure.Persistence;

public class ApplicationTransaction : IApplicationTransaction
{
    private readonly IDbContextTransaction _transaction;

    public ApplicationTransaction(
        IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(
        CancellationToken cancellationToken = default)
        => _transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(
        CancellationToken cancellationToken = default)
        => _transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync()
        => _transaction.DisposeAsync();
}