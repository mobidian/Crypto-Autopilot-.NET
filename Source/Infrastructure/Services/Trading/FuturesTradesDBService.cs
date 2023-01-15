using Application.Data.Entities;
using Application.Interfaces.Services.Trading;
using Application.Mapping;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Database.Contexts;

namespace Infrastructure.Services.Trading;

internal class FuturesTradesDBService : IFuturesTradesDBService
{
    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext) => this.DbContext = dbContext;


    public async Task<bool> AddCandlestickAsync(Candlestick Candlestick)
    {
        var CandlestickEntity = Candlestick.ToDbEntity();

        try
        {
            using var transaction = await this.DbContext.Database.BeginTransactionAsync();
            await this.AddCandlestickToDbAsync(CandlestickEntity);
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, Candlestick Candlestick)
    {
        var CandlestickEntity = Candlestick.ToDbEntity();
        var FuturesOrderEntity = this.DbContext.FuturesOrders.SingleOrDefault(entity => entity.BinanceID == FuturesOrder.Id) ?? FuturesOrder.ToDbEntity();

        try
        {
            using var transaction = await this.DbContext.Database.BeginTransactionAsync();
            await this.AddCandlestickToDbAsync(CandlestickEntity);
            await this.AddFuturesOrderToDbAsync(FuturesOrderEntity);
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            return false;
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
