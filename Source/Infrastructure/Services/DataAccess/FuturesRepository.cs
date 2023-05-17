using Infrastructure.Database;

namespace Infrastructure.Services.DataAccess;

public abstract class FuturesRepository
{
    protected readonly FuturesTradingDbContext DbContext;

    protected FuturesRepository(FuturesTradingDbContext dbContext)
    {
        this.DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
}
