using Application.Extensions;
using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models;

namespace Infrastructure.Services.Bybit;

public class BybitFuturesAccountDataProvider : IBybitFuturesAccountDataProvider
{
    private readonly IBybitRestClientUsdPerpetualApiAccount FuturesAccount;

    public BybitFuturesAccountDataProvider(IBybitRestClientUsdPerpetualApiAccount futuresAccount)
    {
        this.FuturesAccount = futuresAccount;
    }


    public async Task<IEnumerable<ByBitApiKeyInfo>> GetAllApiKeysInfoAsync()
    {
        var callResult = await this.FuturesAccount.GetApiKeyInfoAsync();
        callResult.ThrowIfHasError();

        return callResult.Data;
    }
    public async Task<ByBitApiKeyInfo> GetApiKeyInfoAsync(string publicKey)
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
