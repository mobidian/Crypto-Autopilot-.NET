using Application.Interfaces.Services.General;

using Infrastructure.Database.Entities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

internal class FuturesTradingDbContext : DbContext
{
    private readonly string ConnectionString;
    private readonly IDateTimeProvider DateTimeProvider;

    public FuturesTradingDbContext(string connectionString, IDateTimeProvider dateTimeProvider)
    {
        this.ConnectionString = connectionString;
        this.DateTimeProvider = dateTimeProvider;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(this.ConnectionString);
    }


    public DbSet<CandlestickDbEntity> Candlesticks { get; set; }
    public DbSet<FuturesOrderDbEntity> FuturesOrders { get; set; }
}
