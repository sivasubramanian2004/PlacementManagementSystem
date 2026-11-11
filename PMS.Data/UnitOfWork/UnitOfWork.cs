using Microsoft.EntityFrameworkCore.Storage;

namespace PMS.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_transaction is not null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public Task<int> SaveChangesAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return _context.SaveChangesAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
            return;

        var transaction = _transaction;

        try
        {
            await transaction.CommitAsync();
        }
        finally
        {
            _transaction = null;
            await transaction.DisposeAsync();
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
            return;

        var transaction = _transaction;

        try
        {
            await transaction.RollbackAsync();
        }
        finally
        {
            _transaction = null;
            await transaction.DisposeAsync();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _transaction?.Dispose();
        _transaction = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}