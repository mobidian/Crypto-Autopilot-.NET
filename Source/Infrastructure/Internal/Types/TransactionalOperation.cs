using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Internal.Types;

internal class TransactionalOperation : IDisposable, IAsyncDisposable
{
    private readonly IDbContextTransaction Transaction;
    public TransactionalOperation(IDbContextTransaction transaction) => this.Transaction = transaction;
    
    public void Dispose()
    {
        try
        {
            this.Transaction.Commit();
        }
        catch
        {
            this.Transaction.Rollback();
            throw;
        }
        finally
        {
            this.Transaction.Dispose();
        }
    }    
    public async ValueTask DisposeAsync()
    {
        try
        {
            await this.Transaction.CommitAsync();
        }
        catch
        {
            await this.Transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await this.Transaction.DisposeAsync();
        }
    }
}
