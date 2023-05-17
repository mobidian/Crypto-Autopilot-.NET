using Application.Data.Entities.Orders;
using Application.Data.Validation;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

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
        this.ValidateProperties();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ValidateProperties();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public int ValidateAndSaveChanges() => this.ValidateAndSaveChanges(true);
    public int ValidateAndSaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ValidateProperties();
        this.ValidateRelationships();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public Task<int> ValidateAndSaveChangesAsync(CancellationToken cancellationToken = default) => this.ValidateAndSaveChangesAsync(true, cancellationToken);
    public Task<int> ValidateAndSaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ValidateProperties();
        this.ValidateRelationships();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateProperties()
    {
        try
        {
            var ordersEntries = this.ChangeTracker.Entries<FuturesOrderDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
            foreach (var orderEntry in ordersEntries)
                this.FuturesOrderValidator.ValidateAndThrow(orderEntry.Entity);

            var positionsEntries = this.ChangeTracker.Entries<FuturesPositionDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
            foreach (var positionEntry in positionsEntries)
                this.FuturesPositionValidator.ValidateAndThrow(positionEntry.Entity);
        }
        catch (Exception exception)
        {
            var message = "An error occurred while validating properties of the entities. The database update operation cannot be performed.";
            throw new DbUpdateException(message, exception);
        }
    }
    private void ValidateRelationships()
    {
        try
        {
            var ordersEntries = this.ChangeTracker.Entries<FuturesOrderDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
            foreach (var orderEntry in ordersEntries)
                this.FuturesOrderValidator.Validate(orderEntry.Entity, options => options.IncludeRuleSets("CheckFkRelationships").ThrowOnFailures());

            var positionsEntries = this.ChangeTracker.Entries<FuturesPositionDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
            foreach (var positionEntry in positionsEntries)
                this.FuturesPositionValidator.Validate(positionEntry.Entity, options => options.IncludeRuleSets("CheckFkRelationships").ThrowOnFailures());
        }
        catch (Exception exception)
        {
            var message = "An error occurred while validating relationships between entities. The database update operation cannot be performed.";
            throw new DbUpdateException(message, exception);
        }
    }



    public DbSet<FuturesOrderDbEntity> FuturesOrders { get; set; }
    public DbSet<FuturesPositionDbEntity> FuturesPositions { get; set; }
}
