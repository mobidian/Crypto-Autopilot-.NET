using Application.Data.Entities;
using Application.Interfaces.Services.Trading.Binance;
using Application.Mapping;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Database.Internal;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Trading.Binance;

public class FuturesTradesDBService : IFuturesTradesDBService
{
    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext) => this.DbContext = dbContext;


    public async Task AddCandlestickAsync(Candlestick Candlestick)
    {
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));


        var CandlestickEntity = Candlestick.ToDbEntity();

        using var transaction = this.BeginTransaction();
        await this.AddCandlestickToDbAsync(CandlestickEntity);
    }
    public async Task AddFuturesOrdersAsync(Candlestick Candlestick, params BinanceFuturesOrder[] FuturesOrders)
    {
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));
        ValidateFuturesOrdersArray(Candlestick, FuturesOrders);
        var CandlestickEntity = this.GetCandlestickEntityFromDb(Candlestick) ?? Candlestick.ToDbEntity();


        using var transaction = this.BeginTransaction();

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
    private static void ValidateFuturesOrdersArray(Candlestick Candlestick, BinanceFuturesOrder[] FuturesOrders)
    {
        _ = FuturesOrders ?? throw new ArgumentNullException(nameof(FuturesOrders));

        foreach (var FuturesOrder in FuturesOrders)
        {
            _ = FuturesOrder ?? throw new ArgumentNullException(nameof(FuturesOrder));

            if (Candlestick.CurrencyPair != FuturesOrder.Symbol)
            {
                var innerException = new ArgumentException($"Cannot insert the specified candlestick and futures order since the FuturesOrder.Symbol ({FuturesOrder.Symbol}) does not match the Candlestick.CurrencyPair ({Candlestick.CurrencyPair}).", nameof(FuturesOrder.Symbol));
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

    public async Task<IEnumerable<BinanceFuturesOrder>> GetAllFuturesOrdersAsync()
    {
        return await this.DbContext.FuturesOrders
            .Include(x => x.Candlestick)
            .OrderBy(x => x.Candlestick.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }
    public async Task<IEnumerable<BinanceFuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair)
    {
        return await this.DbContext.FuturesOrders
            .Include(x => x.Candlestick)
            .Where(x => x.Candlestick.CurrencyPair == currencyPair)
            .OrderBy(x => x.Candlestick.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }


    /// <summary>
    /// Begins a new transaction and returns a <see cref="TransactionalOperation"/> object which wraps the transaction
    /// </summary>
    /// <returns></returns>
    private TransactionalOperation BeginTransaction() => new TransactionalOperation(this.DbContext.Database.BeginTransaction());
}
