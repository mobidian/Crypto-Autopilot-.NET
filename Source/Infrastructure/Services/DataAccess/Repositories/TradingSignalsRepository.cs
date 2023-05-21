using Application.Data.Mapping;
using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Models.Signals;

using Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.DataAccess.Repositories;

public class TradingSignalsRepository : ITradingSignalsRepository
{
    private readonly FuturesTradingDbContext DbContext;

    public TradingSignalsRepository(FuturesTradingDbContext dbContext)
    {
        this.DbContext = dbContext;
    }


    public async Task<bool> AddAsync(TradingSignal tradingSignal, Guid? bybitOrderId = null, Guid? positionCryptoAutopilotId = null)
    {
        var entity = tradingSignal.ToDbEntity();
        if (bybitOrderId is not null)
        {
            var orderDbEntity = await this.DbContext.FuturesOrders.FirstAsync(x => x.BybitID == bybitOrderId);
            entity.OrderId = orderDbEntity.Id;
        }
        if (positionCryptoAutopilotId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.FirstAsync(x => x.CryptoAutopilotId == positionCryptoAutopilotId);
            entity.PositionId = positionDbEntity.Id;
        }


        await this.DbContext.TradingSignals.AddAsync(entity);
        return await this.DbContext.SaveChangesAsync() == 1;
    }

    public async Task<TradingSignal?> GetByCryptoAutopilotIdAsync(Guid cryptoAutopilotId)
    {
        var entity = await this.DbContext.TradingSignals.FirstOrDefaultAsync(x => x.CryptoAutopilotId == cryptoAutopilotId);
        return entity?.ToDomainObject();
    }
    public Task<IEnumerable<TradingSignal>> GetByContractAsync(string contract)
    {
        var entities = this.DbContext.TradingSignals.Where(x => x.CurrencyPair == contract).AsEnumerable();
        return Task.FromResult(entities.Select(x => x.ToDomainObject()));
    }
    public Task<IEnumerable<TradingSignal>> GetByContractWithInfoTypeAsync<TSignalInfo>(string contract) where TSignalInfo : SignalInfo
    {
        var entities = this.DbContext.TradingSignals.Where(x => x.CurrencyPair == contract).AsEnumerable().Where(x => x.Info is TSignalInfo);
        return Task.FromResult(entities.Select(x => x.ToDomainObject()));
    }
    public Task<IEnumerable<TradingSignal>> GetAllAsync()
    {
        var signals = this.DbContext.TradingSignals.AsEnumerable().Select(x => x.ToDomainObject());
        return Task.FromResult(signals);
    }
    public Task<IEnumerable<TradingSignal>> GetAllWithInfoTypeAsync<TSignalInfo>() where TSignalInfo : SignalInfo
    {
        var entities = this.DbContext.TradingSignals.AsEnumerable().Where(x => x.Info is TSignalInfo);
        return Task.FromResult(entities.Select(x => x.ToDomainObject()));
    }

    public async Task<bool> UpdateAsync(Guid cryptoAutopilotId, TradingSignal updatedSignal, Guid? bybitOrderId = null, Guid? positionCryptoAutopilotId = null)
    {
        var entity = await this.DbContext.TradingSignals.FirstOrDefaultAsync(x => x.CryptoAutopilotId ==  cryptoAutopilotId) ?? throw new DbUpdateException($"Could not find futures order with cryptoAutopilotId == {cryptoAutopilotId}");
        if (bybitOrderId is not null)
        {
            var orderDbEntity = await this.DbContext.FuturesOrders.FirstAsync(x => x.BybitID == bybitOrderId);
            entity.OrderId = orderDbEntity.Id;
        }
        if (positionCryptoAutopilotId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.FirstAsync(x => x.CryptoAutopilotId == positionCryptoAutopilotId);
            entity.PositionId = positionDbEntity.Id;
        }

        entity.Source = updatedSignal.Source;
        entity.CurrencyPair = updatedSignal.CurrencyPair.Name;
        entity.Time = updatedSignal.Time;
        entity.Info = updatedSignal.Info;
        
        return await this.DbContext.SaveChangesAsync() == 1;
    }

    public async Task<bool> DeleteAsync(params Guid[] cryptoAutopilotIds)
    {
        foreach (var cryptoAutopilotId in cryptoAutopilotIds)
            if (await this.DbContext.TradingSignals.FirstOrDefaultAsync(x => x.CryptoAutopilotId == cryptoAutopilotId) is null)
                throw new DbUpdateException($"No order with bybitID {cryptoAutopilotId} was found in the database");
        
        var orders = this.DbContext.TradingSignals.Where(x => cryptoAutopilotIds.Contains(x.CryptoAutopilotId));
        this.DbContext.TradingSignals.RemoveRange(orders);
        return await this.DbContext.SaveChangesAsync() == cryptoAutopilotIds.Length;
    }
}
