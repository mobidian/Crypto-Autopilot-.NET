using Application.Data.Mapping;
using Application.DataAccess.Repositories;

using Domain.Models.Signals;

using Infrastructure.DataAccess.Database;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess.Repositories;

public class TradingSignalsRepository : ITradingSignalsRepository
{
    private readonly FuturesTradingDbContext DbContext;

    public TradingSignalsRepository(FuturesTradingDbContext dbContext)
    {
        this.DbContext = dbContext;
    }


    public async Task<bool> AddAsync(TradingSignal tradingSignal)
    {
        var entity = tradingSignal.ToDbEntity();
        await this.DbContext.TradingSignals.AddAsync(entity);
        return await this.DbContext.SaveChangesAsync() == 1;
    }
    public async Task<bool> AddAsync(IEnumerable<TradingSignal> tradingSignals)
    {
        var entities = tradingSignals.Select(x => x.ToDbEntity());
        await this.DbContext.TradingSignals.AddRangeAsync(entities);
        return await this.DbContext.SaveChangesAsync() > 0;
    }

    public async Task<TradingSignal?> GetByCryptoAutopilotIdAsync(Guid cryptoAutopilotId)
    {
        var entity = await this.DbContext.TradingSignals.FirstOrDefaultAsync(x => x.CryptoAutopilotId == cryptoAutopilotId);
        return entity?.ToDomainObject();
    }
    public Task<IEnumerable<TradingSignal>> GetAllWithContractAsync(string contract)
    {
        var entities = this.DbContext.TradingSignals.Where(x => x.CurrencyPair == contract).AsEnumerable();
        return Task.FromResult(entities.Select(x => x.ToDomainObject()));
    }
    public Task<IEnumerable<TradingSignal>> GetAllAsync()
    {
        var signals = this.DbContext.TradingSignals.AsEnumerable().Select(x => x.ToDomainObject());
        return Task.FromResult(signals);
    }

    public async Task<bool> UpdateAsync(TradingSignal updatedSignal)
    {
        var cryptoAutopilotId = updatedSignal.CryptoAutopilotId;
        var entity = await this.DbContext.TradingSignals.FirstOrDefaultAsync(x => x.CryptoAutopilotId == cryptoAutopilotId) ?? throw new DbUpdateException($"Could not find futures order with cryptoAutopilotId == {cryptoAutopilotId}");

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
                throw new DbUpdateException($"No trading sigal with with cryptoAutopilotId '{cryptoAutopilotId}' was found in the database");

        var orders = this.DbContext.TradingSignals.Where(x => cryptoAutopilotIds.Contains(x.CryptoAutopilotId));
        this.DbContext.TradingSignals.RemoveRange(orders);
        return await this.DbContext.SaveChangesAsync() == cryptoAutopilotIds.Length;
    }
}
