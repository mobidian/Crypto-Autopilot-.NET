using Application.Data.Entities;
using Application.Interfaces.Services.Trading;
using Application.Mapping;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Database.Contexts;

namespace Infrastructure.Services.Trading;

public class FuturesTradesDBService : IFuturesTradesDBService
{
    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext) => this.DbContext = dbContext;


    public async Task AddCandlestickAsync(Candlestick Candlestick)
    {
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));

        
        var CandlestickEntity = Candlestick.ToDbEntity();

        using var transaction = await this.DbContext.Database.BeginTransactionAsync();
        await this.AddCandlestickToDbAsync(CandlestickEntity);
        await transaction.CommitAsync();
    }
    public async Task AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, Candlestick Candlestick)
    {
        _ = FuturesOrder ?? throw new ArgumentNullException(nameof(FuturesOrder));
        _ = Candlestick ?? throw new ArgumentNullException(nameof(Candlestick));


        var CandlestickEntity = this.DbContext.Candlesticks.SingleOrDefault(c => c.CurrencyPair == Candlestick.CurrencyPair && c.DateTime == Candlestick.Date) ?? Candlestick.ToDbEntity();
        var FuturesOrderEntity = FuturesOrder.ToDbEntity();
        FuturesOrderEntity.CandlestickId = CandlestickEntity.Id;

        using var transaction = await this.DbContext.Database.BeginTransactionAsync();
        await this.AddCandlestickToDbAsync(CandlestickEntity);
        await this.AddFuturesOrderToDbAsync(FuturesOrderEntity);
        await transaction.CommitAsync();
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
