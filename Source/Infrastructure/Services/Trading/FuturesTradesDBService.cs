using Application.Data.Entities;
using Application.Data.Mapping;
using Application.Interfaces.Services.Trading;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Database.Internal;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Trading;

public class FuturesTradesDBService : IFuturesTradesDBService
{
    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext) => this.DbContext = dbContext;


    public async Task AddCandlestickAsync(Candlestick Candlestick)
    {
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));


        var CandlestickEntity = Candlestick.ToDbEntity();
        
        using var transaction = await this.BeginTransactionAsync();
        await this.AddCandlestickToDbAsync(CandlestickEntity);
    }
    public async Task AddFuturesOrdersAsync(Candlestick Candlestick, params FuturesOrder[] FuturesOrders)
    {
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));
        ValidateFuturesOrdersArray(Candlestick, FuturesOrders);
        var CandlestickEntity = this.GetCandlestickEntityFromDb(Candlestick) ?? Candlestick.ToDbEntity();


        using var transaction = await this.BeginTransactionAsync();

        if (CandlestickEntity.Id == 0)
            await this.AddCandlestickToDbAsync(CandlestickEntity);

        var futuresOrderDbEntities = FuturesOrders.Select(o =>
        {
            var entity = o.ToDbEntity();
            entity.CandlestickId = CandlestickEntity.Id;
            return entity;
        });
        await this.AddFuturesOrdersToDbAsync(futuresOrderDbEntities);
    }
    private static void ValidateFuturesOrdersArray(Candlestick Candlestick, FuturesOrder[] FuturesOrders)
    {
        _ = FuturesOrders ?? throw new ArgumentNullException(nameof(FuturesOrders));

        foreach (var FuturesOrder in FuturesOrders)
        {
            _ = FuturesOrder ?? throw new ArgumentNullException(nameof(FuturesOrder));

            if (Candlestick.CurrencyPair != FuturesOrder.CurrencyPair)
            {
                var innerException = new ArgumentException($"Cannot insert the specified candlestick and futures order since the FuturesOrder.Symbol ({FuturesOrder.CurrencyPair}) does not match the Candlestick.CurrencyPair ({Candlestick.CurrencyPair}).", nameof(FuturesOrder.CurrencyPair));
                throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
            }
        }
    }
    private CandlestickDbEntity? GetCandlestickEntityFromDb(Candlestick Candlestick)
    {
        var uniqueIndex = (Candlestick.CurrencyPair, Candlestick.Date);
        return this.DbContext.Candlesticks.SingleOrDefault(x => x.CurrencyPair == uniqueIndex.CurrencyPair.Name && x.DateTime == uniqueIndex.Date);
    }
    private async Task AddCandlestickToDbAsync(CandlestickDbEntity candlestickDbEntity)
    {
        await this.DbContext.Candlesticks.AddAsync(candlestickDbEntity);
        await this.DbContext.SaveChangesAsync();
    }
    private async Task AddFuturesOrdersToDbAsync(IEnumerable<FuturesOrderDbEntity> futuresOrderDbEntities)
    {
        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.SaveChangesAsync();
    }

    
    public async Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync()
    {
        return await this.DbContext.Candlesticks
            .OrderBy(x => x.CurrencyPair)
            .ThenByDescending(x => x.DateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }
    public async Task<IEnumerable<Candlestick>> GetCandlesticksByCurrencyPairAsync(string currencyPair)
    {
        return await this.DbContext.Candlesticks
            .Where(x => x.CurrencyPair == currencyPair)
            .OrderBy(x => x.CurrencyPair)
            .ThenByDescending(x => x.DateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }

    public async Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync()
    {
        return await this.DbContext.FuturesOrders
            .Include(x => x.Candlestick)
            .OrderBy(x => x.Candlestick.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }
    public async Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair)
    {
        return await this.DbContext.FuturesOrders
            .Include(x => x.Candlestick)
            .Where(x => x.Candlestick.CurrencyPair == currencyPair)
            .OrderBy(x => x.Candlestick.CurrencyPair)
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
