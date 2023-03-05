using Bybit.Net.Enums;

using Bybit.Net.Objects.Models;

namespace Application.Interfaces.Services.Trading.BybitExchange;

public interface IBybitFuturesAccountDataProvider
{
    public Task<IEnumerable<ByBitApiKeyInfo>> GetAllApiKeysInfoAsync();
    public Task<ByBitApiKeyInfo> GetApiKeyInfoAsync(string? publicKey = null);
    
    public Task<BybitBalance> GetAssetBalanceAsync(string asset);
    
    public Task<BybitPositionUsd?> GetPositionAsync(string asset, PositionSide positionSide);
}
