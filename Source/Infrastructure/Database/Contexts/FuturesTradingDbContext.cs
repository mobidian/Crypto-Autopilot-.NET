using Application.Data.Entities;
using Application.Interfaces.Services.General;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Contexts;

public class FuturesTradingDbContext : AuditableDbContext
{
    private readonly string ConnectionString;

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
        SetFkRestrictOnDelete(modelBuilder);
    }
    private static void SetFkRestrictOnDelete(ModelBuilder modelBuilder)
    {
        modelBuilder.Model.GetEntityTypes()
                    .SelectMany(entity => entity.GetForeignKeys())
                    .Where(fk => !fk.IsOwnership && fk.DeleteBehavior != DeleteBehavior.Restrict)
                    .ToList()
                    .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);
    }

    public DbSet<CandlestickDbEntity> Candlesticks { get; set; }
    public DbSet<FuturesOrderDbEntity> FuturesOrders { get; set; }
}
