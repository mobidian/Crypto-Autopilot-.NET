using Application.Data.Entities.Signals;

using Domain.Models.Signals;

namespace Application.Data.Mapping;

public static class TradingSignalMappingExtensions
{
    public static TradingSignalDbEntity ToDbEntity(this TradingSignal signal) => new TradingSignalDbEntity
    {
        CryptoAutopilotId = signal.CryptoAutopilotId,
        Source = signal.Source,
        Contract = signal.Contract,
        Time = signal.Time,
        Info = signal.Info.DeepClone(), // prevents unexpected mutations on TradingSignalDbEntity
    };
    
    public static TradingSignal ToDomainObject(this TradingSignalDbEntity entity) => new TradingSignal
    {
        CryptoAutopilotId = entity.CryptoAutopilotId,
        Source = entity.Source,
        Contract = entity.Contract,
        Time = entity.Time,
        Info = entity.Info.DeepClone(), // prevents unexpected mutations on TradingSignal
    };
}
