using Application.Data.Entities;
using Application.Interfaces.Services.General;

using Infrastructure.Services.General;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Contexts;

public class FuturesTradingDbContext : AuditableDbContext
{
    private readonly string ConnectionString;

    public FuturesTradingDbContext() : this("""Data Source=(localdb)\MSSQLLocalDB; Initial Catalog=CryptoPilotTrades""", new DateTimeProvider()) { }
    public FuturesTradingDbContext(string connectionString, IDateTimeProvider dateTimeProvider) : base(dateTimeProvider)
    {
        this.ConnectionString = connectionString;
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
