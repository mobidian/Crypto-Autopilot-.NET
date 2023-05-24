using Application.Data.Entities.Futures;
using Application.Data.Entities.Signals;
using Application.Data.Validation;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.DataAccess.Database;

public class FuturesTradingDbContext : DbContext
{
    private readonly FuturesOrderDbEntityValidator FuturesOrderValidator = new();
    private readonly FuturesPositionDbEntityValidator FuturesPositionValidator = new();
    private readonly TradingSignalDbEntityValidator TradingSignalValidator = new();

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
        this.ValidateEntriesProperties();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ValidateEntriesProperties();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    #region Validation enforcing methods
    private void ValidateEntriesProperties()
    {
        try
        {
            this.ValidateOrderEntries();
            this.ValidatePositionEntries();
            this.ValidateTradingSignalEntries();
        }
        catch (Exception exception)
        {
            var message = "An error occurred while validating the entities. The database update operation cannot be performed.";
            throw new DbUpdateException(message, exception);
        }
    }
    private void ValidateOrderEntries()
    {
        var ordersEntries = this.ChangeTracker.Entries<FuturesOrderDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
        foreach (var orderEntry in ordersEntries)
            this.FuturesOrderValidator.ValidateAndThrow(orderEntry.Entity);
    }
    private void ValidatePositionEntries()
    {
        var positionsEntries = this.ChangeTracker.Entries<FuturesPositionDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
        foreach (var positionEntry in positionsEntries)
            this.FuturesPositionValidator.ValidateAndThrow(positionEntry.Entity);
    }
    private void ValidateTradingSignalEntries()
    {
        var signalsEntries = this.ChangeTracker.Entries<TradingSignalDbEntity>().Where(e => e.State is EntityState.Added or EntityState.Modified);
        foreach (var signalsEntry in signalsEntries)
            this.TradingSignalValidator.ValidateAndThrow(signalsEntry.Entity);
    }
    #endregion


    public DbSet<FuturesOrderDbEntity> FuturesOrders { get; set; }
    public DbSet<FuturesPositionDbEntity> FuturesPositions { get; set; }

    public DbSet<TradingSignalDbEntity> TradingSignals { get; set; }
}
