using Application.Data.Entities;
using Application.Interfaces.Services.Trading;
using Application.Mapping;

using Binance.Net.Objects.Models.Futures;

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

        using var transaction = this.BeginTransaction();
        await this.AddCandlestickToDbAsync(CandlestickEntity);
    }
    public async Task AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, Candlestick Candlestick)
    {
        _ = FuturesOrder ?? throw new ArgumentNullException(nameof(FuturesOrder));
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));


        using var transaction = this.BeginTransaction();

        var CandlestickEntity = this.GetCandlestickEntityFromDb(Candlestick) ?? Candlestick.ToDbEntity();
        if (CandlestickEntity.Id == 0)
            await this.AddCandlestickToDbAsync(CandlestickEntity);
        
        var FuturesOrderEntity = FuturesOrder.ToDbEntity();
        FuturesOrderEntity.CandlestickId = CandlestickEntity.Id;
        await this.AddFuturesOrderToDbAsync(FuturesOrderEntity);
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
    private async Task AddFuturesOrderToDbAsync(FuturesOrderDbEntity futuresOrderDbEntity)
    {
        await this.DbContext.FuturesOrders.AddAsync(futuresOrderDbEntity);
        await this.DbContext.SaveChangesAsync();
    }


    public async Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync()
    {
        return await this.DbContext.Candlesticks.Select(x => x.ToDomainObject()).ToListAsync();
    }
    public async Task<IEnumerable<BinanceFuturesOrder>> GetAllFuturesOrdersAsync()
    {
        return await this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).ToListAsync();
    }


    /// <summary>
    /// Begins a new transaction and returns a <see cref="TransactionalOperation"/> object which wraps the transaction
    /// </summary>
    /// <returns></returns>
    private TransactionalOperation BeginTransaction() => new TransactionalOperation(this.DbContext.Database.BeginTransaction());
}
