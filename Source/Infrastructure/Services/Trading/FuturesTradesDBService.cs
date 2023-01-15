using Application.Data.Entities;
using Application.Interfaces.Services.Trading;
using Application.Mapping;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Database.Contexts;

using Microsoft.EntityFrameworkCore.Storage;

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
    public async Task AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, Candlestick Candlestick)
    {
        _ = FuturesOrder ?? throw new ArgumentNullException(nameof(FuturesOrder));
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));


        using var transaction = await this.BeginTransactionAsync();

        var CandlestickEntity = this.GetCandlestickEntityFromDb(Candlestick) ?? Candlestick.ToDbEntity();
        if (CandlestickEntity.Id == 0)
            await this.AddCandlestickToDbAsync(CandlestickEntity);
        
        var FuturesOrderEntity = FuturesOrder.ToDbEntity();
        FuturesOrderEntity.CandlestickId = CandlestickEntity.Id;
        await this.AddFuturesOrderToDbAsync(FuturesOrderEntity);
    }
    
    private CandlestickDbEntity? GetCandlestickEntityFromDb(Candlestick Candlestick)
    {
        var uniqueIndex = (Candlestick.CurrencyPair.Base, Candlestick.CurrencyPair.Quote, Candlestick.Date);
        return this.DbContext.Candlesticks.SingleOrDefault(x => x.BaseCurrency == uniqueIndex.Base && x.QuoteCurrency == uniqueIndex.Quote && x.DateTime == uniqueIndex.Date);
    }

    private async Task<TransactionalOperation> BeginTransactionAsync() => new TransactionalOperation(await this.DbContext.Database.BeginTransactionAsync());
    internal class TransactionalOperation : IDisposable
    {
        private readonly IDbContextTransaction Transaction;
        public TransactionalOperation(IDbContextTransaction transaction) => this.Transaction = transaction;

        public void Dispose()
        {
            this.Transaction.Commit();
            this.Transaction.Dispose();
        }
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
}
