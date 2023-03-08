using Application.Data.Mapping;
using Application.Interfaces.Services;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Database.Internal;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class FuturesTradesDBService : IFuturesTradesDBService
{
    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext) => this.DbContext = dbContext;



    public async Task AddFuturesOrdersAsync(params FuturesOrder[] FuturesOrders)
    {
        _ = FuturesOrders ?? throw new ArgumentNullException(nameof(FuturesOrders));
        

        using var transaction = await this.BeginTransactionAsync();
        
        var futuresOrderDbEntities = FuturesOrders.Select(o => o.ToDbEntity());
        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.SaveChangesAsync();
    }

    
    public async Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync()
    {
        return await this.DbContext.FuturesOrders
            .OrderBy(x => x.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }
    public async Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair)
    {
        return await this.DbContext.FuturesOrders
            .Where(x => x.CurrencyPair == currencyPair)
            .OrderBy(x => x.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }


    public async Task UpdateFuturesOrderAsync(Guid uniqueID, FuturesOrder newFuturesOrderValue)
    {
        _ = newFuturesOrderValue ?? throw new ArgumentNullException(nameof(newFuturesOrderValue));


        using var transaction = await this.BeginTransactionAsync();

        var dbEntity = await this.DbContext.FuturesOrders.Where(x => x.UniqueID == uniqueID).SingleOrDefaultAsync() ?? throw new DbUpdateException($"Could not find futures order with uniqueID == {uniqueID}");
        dbEntity.UniqueID = newFuturesOrderValue.UniqueID;
        dbEntity.CreateTime = newFuturesOrderValue.CreateTime;
        dbEntity.UpdateTime = newFuturesOrderValue.UpdateTime;
        dbEntity.Side = newFuturesOrderValue.Side;
        dbEntity.PositionSide = newFuturesOrderValue.PositionSide;
        dbEntity.Type = newFuturesOrderValue.Type;
        dbEntity.Price = newFuturesOrderValue.Price;
        dbEntity.Quantity = newFuturesOrderValue.Quantity;
        dbEntity.StopLoss = newFuturesOrderValue.StopLoss;
        dbEntity.TakeProfit = newFuturesOrderValue.TakeProfit;
        dbEntity.TimeInForce = newFuturesOrderValue.TimeInForce;
        dbEntity.Status = newFuturesOrderValue.Status;

        await this.DbContext.SaveChangesAsync();
    }



    /// <summary>
    /// Begins a new transaction and returns a <see cref="TransactionalOperation"/> object which wraps the transaction
    /// </summary>
    /// <returns></returns>
    private async Task<TransactionalOperation> BeginTransactionAsync() => new TransactionalOperation(await this.DbContext.Database.BeginTransactionAsync());
}
