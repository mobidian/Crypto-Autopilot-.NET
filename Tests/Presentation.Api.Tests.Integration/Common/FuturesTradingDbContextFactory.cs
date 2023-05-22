using Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

namespace Presentation.Api.Tests.Integration.Common;

public class FuturesTradingDbContextFactory
{
    private readonly DbContextOptions Options;
    public FuturesTradingDbContextFactory(DbContextOptions options) => this.Options = options;

    public FuturesTradingDbContext Create() => new FuturesTradingDbContext(this.Options);
}
