using Application.Interfaces.Services.Trading.BybitExchange;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading.BybitExchange;

public class BybitFuturesAccountDataProvider : IBybitFuturesAccountDataProvider
{
    private readonly IBybitClientUsdPerpetualApiAccount FuturesAccount;

    public BybitFuturesAccountDataProvider(IBybitClientUsdPerpetualApiAccount futuresAccount)
    {
        this.FuturesAccount = futuresAccount;
    }

    
    public async Task<IEnumerable<ByBitApiKeyInfo>> GetAllApiKeysInfoAsync()
    {
        var callResult = await this.FuturesAccount.GetApiKeyInfoAsync();
        callResult.ThrowIfHasError();

        return callResult.Data;
    }
    public async Task<ByBitApiKeyInfo> GetApiKeyInfoAsync(string? publicKey = null)
    {
        var apiKeys = await this.GetAllApiKeysInfoAsync();
        return apiKeys.Single(x => x.Apikey == publicKey);
    }

    
    public async Task<BybitBalance> GetAssetBalanceAsync(string asset)
    {
        var callReuslt = await this.FuturesAccount.GetBalancesAsync(asset);
        callReuslt.ThrowIfHasError();
        return callReuslt.Data.Single().Value;
    }
    
    
    public async Task<BybitPositionUsd?> GetPositionAsync(string asset, PositionSide positionSide)
    {
        var callReuslt = await this.FuturesAccount.GetPositionAsync(asset);
        callReuslt.ThrowIfHasError();

        var position = callReuslt.Data.Single(x => x.Side == positionSide);

        if (position.EntryPrice == 0)
            return null;

        return position;
    }
}
