using Infrastructure.Internal.Types;

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Internal.Extensions;

internal static class DatabaseFacadeExtensions
{
    public static TransactionalOperation BeginTransactionalOperation(this DatabaseFacade database) => new(database.BeginTransaction());
    public static async Task<TransactionalOperation> BeginTransactionalOperationAsync(this DatabaseFacade database) => new(await database.BeginTransactionAsync());
}