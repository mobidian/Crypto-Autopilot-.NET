using Bybit.Net.Enums;

using Bybit.Net.Objects.Models;

namespace Application.Interfaces.Services.Bybit;

public interface IBybitFuturesAccountDataProvider
{
    public Task<IEnumerable<ByBitApiKeyInfo>> GetAllApiKeysInfoAsync();
    public Task<ByBitApiKeyInfo> GetApiKeyInfoAsync(string publicKey);

    public Task<BybitBalance> GetAssetBalanceAsync(string asset);

    public Task<BybitPositionUsd?> GetPositionAsync(string asset, PositionSide positionSide);
}
