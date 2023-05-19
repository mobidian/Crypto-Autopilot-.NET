using Domain.Models.Signals;

namespace Application.Interfaces.Services.DataAccess.Repositories;

public interface ISignalsRepository
{
    public Task<bool> AddAsync(TradingSignal tradingSignal);

    public Task<IEnumerable<TradingSignal>> GetAllAsync();
    public Task<IEnumerable<TradingSignal>> GetAllWithInfoTypeAsync<TSignalInfo>() where TSignalInfo : SignalInfo;

    public Task<TradingSignal> GetByContractAsync(string contract);
    public Task<TradingSignal> GetByContractWithInfoTypeAsync<TSignalInfo>(string contract) where TSignalInfo : SignalInfo;

    public Task<bool> UpdateAsync(Guid cryptoAutopilotId, TradingSignal updatedSignal);

    public Task<bool> DeleteAsync(Guid cryptoAutopilotId);
}
