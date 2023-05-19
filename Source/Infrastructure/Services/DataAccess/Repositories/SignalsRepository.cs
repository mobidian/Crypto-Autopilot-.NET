using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Models.Signals;

namespace Infrastructure.Services.DataAccess.Repositories;

public class SignalsRepository : ISignalsRepository
{
    public Task<bool> AddAsync(TradingSignal tradingSignal)
    {
        // // TODO SignalsRepository implementation // //
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TradingSignal>> GetAllAsync()
    {
        // // TODO SignalsRepository implementation // //
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TradingSignal>> GetAllWithInfoTypeAsync<TSignalInfo>() where TSignalInfo : SignalInfo
    {
        // // TODO SignalsRepository implementation // //
        throw new NotImplementedException();
    }

    public Task<TradingSignal> GetByContractAsync(string contract)
    {
        // // TODO SignalsRepository implementation // //
        throw new NotImplementedException();
    }

    public Task<TradingSignal> GetByContractWithInfoTypeAsync<TSignalInfo>(string contract) where TSignalInfo : SignalInfo
    {
        // // TODO SignalsRepository implementation // //
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Guid cryptoAutopilotId, TradingSignal updatedSignal)
    {
        // // TODO SignalsRepository implementation // //
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid cryptoAutopilotId)
    {
        // // TODO SignalsRepository implementation // //
        throw new NotImplementedException();
    }
}
