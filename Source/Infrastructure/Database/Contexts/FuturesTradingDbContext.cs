using Application.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Contexts;

public class FuturesTradingDbContext : DbContext
{
    public FuturesTradingDbContext(DbContextOptions options) : base(options) { }
    
    
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
