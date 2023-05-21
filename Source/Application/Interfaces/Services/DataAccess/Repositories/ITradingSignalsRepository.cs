using Domain.Models.Signals;

namespace Application.Interfaces.Services.DataAccess.Repositories;

public interface ITradingSignalsRepository
{
    public Task<bool> AddAsync(TradingSignal tradingSignal, Guid? bybitOrderId = null, Guid? positionCryptoAutopilotId = null);
    
    public Task<TradingSignal?> GetByCryptoAutopilotIdAsync(Guid cryptoAutopilotId);
    public Task<IEnumerable<TradingSignal>> GetByContractAsync(string contract);
    public Task<IEnumerable<TradingSignal>> GetByContractWithInfoTypeAsync<TSignalInfo>(string contract) where TSignalInfo : SignalInfo;
    public Task<IEnumerable<TradingSignal>> GetAllAsync();
    public Task<IEnumerable<TradingSignal>> GetAllWithInfoTypeAsync<TSignalInfo>() where TSignalInfo : SignalInfo;

    public Task<bool> UpdateAsync(Guid cryptoAutopilotId, TradingSignal updatedSignal, Guid? bybitOrderId = null, Guid? positionCryptoAutopilotId = null);

    public Task<bool> DeleteAsync(params Guid[] cryptoAutopilotIds);
}
