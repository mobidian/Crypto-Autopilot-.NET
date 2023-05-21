using Domain.Models.Signals;

namespace Application.Interfaces.Services.DataAccess.Repositories;

public interface ITradingSignalsRepository
{
    public Task<bool> AddAsync(TradingSignal tradingSignal);
    public Task<bool> AddAsync(IEnumerable<TradingSignal> tradingSignals);

    public Task<TradingSignal?> GetByCryptoAutopilotIdAsync(Guid cryptoAutopilotId);
    public Task<IEnumerable<TradingSignal>> GetAllWithContractAsync(string contract);
    public Task<IEnumerable<TradingSignal>> GetAllAsync();

    public Task<bool> UpdateAsync(TradingSignal updatedSignal);

    public Task<bool> DeleteAsync(params Guid[] cryptoAutopilotIds);
}
