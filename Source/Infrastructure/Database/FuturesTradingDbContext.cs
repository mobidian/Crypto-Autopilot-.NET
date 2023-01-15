using Application.Interfaces.Services.General;

using Infrastructure.Database.Entities;
using Infrastructure.Services.General;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class FuturesTradingDbContext : DbContext
{
    private readonly string ConnectionString;
    private readonly IDateTimeProvider DateTimeProvider;

    public FuturesTradingDbContext() : this("""Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=CryptoPilotTrades""", new DateTimeProvider()) { }
    public FuturesTradingDbContext(string connectionString, IDateTimeProvider dateTimeProvider)
    {
        this.ConnectionString = connectionString;
        this.DateTimeProvider = dateTimeProvider;
    }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(this.ConnectionString);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuturesTradingDbContext).Assembly, type => !type.IsInterface && !type.IsAbstract);
    }

    public DbSet<CandlestickDbEntity> Candlesticks { get; set; }
    public DbSet<FuturesOrderDbEntity> FuturesOrders { get; set; }
}
