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
        this.ValidateEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ValidateEntries();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    #region Validation enforcing methods
    private void ValidateEntries()
    {
        try
        {
            this.ValidateUpdatedOrders();
            this.ValidateUpdatedPositions();

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
    private void ValidateUpdatedOrders()
    {
        var updatedOrders = this.ChangeTracker
                        .Entries<FuturesOrderDbEntity>()
                        .Where(x => x.State == EntityState.Modified)
                        .Select(x => x.Entity);
        
        foreach (var order in updatedOrders)
        {
            var position = this.FuturesPositions.Find(order.PositionId);
            
            if (position is not null && position.Side != order.PositionSide)
                throw new DbUpdateException($"The new order position side property value does not match the side property value of the related position.");
        }
    }
    private void ValidateUpdatedPositions()
    {
        var updatedPositions = this.ChangeTracker
                        .Entries<FuturesPositionDbEntity>()
                        .Where(x => x.State == EntityState.Modified)
                        .Select(x => x.Entity);
        
        foreach (var position in updatedPositions)
        {
            var order = position.FuturesOrders!.FirstOrDefault();

            if (order is not null && position.Side != order.PositionSide)
                throw new DbUpdateException($"The new position side property value does not match the position side property value of the related orders.");
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
