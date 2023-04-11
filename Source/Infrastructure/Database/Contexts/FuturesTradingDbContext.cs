using Application.Data.Entities;
using Application.Data.Validation;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Contexts;

public class FuturesTradingDbContext : DbContext
{
    private readonly FuturesOrderDbEntityValidator FuturesOrderValidator = new();
    private readonly FuturesPositionDbEntityValidator FuturesPositionValidator = new();

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

    
    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ValidateEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ValidateEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    private void ValidateEntities()
    {
        var ordersEntries = this.ChangeTracker.Entries<FuturesOrderDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
        foreach (var orderEntry in ordersEntries)
            this.FuturesOrderValidator.ValidateAndThrow(orderEntry.Entity);
        
        var positionsEntries = this.ChangeTracker.Entries<FuturesPositionDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
        foreach (var positionEntry in positionsEntries)
            this.FuturesPositionValidator.ValidateAndThrow(positionEntry.Entity);
    }




    public DbSet<FuturesOrderDbEntity> FuturesOrders { get; set; }
    public DbSet<FuturesPositionDbEntity> FuturesPositions { get; set; }
}
