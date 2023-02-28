using Application.Interfaces.Services.Trading;

using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading;

public class BinanceFuturesAccountDataProvider : IBinanceFuturesAccountDataProvider
{
    private readonly IBinanceClientUsdFuturesApiAccount BinanceFuturesAccount;
    
    public BinanceFuturesAccountDataProvider(IBinanceClientUsdFuturesApiAccount binanceFuturesAccount)
    {
        this.BinanceFuturesAccount = binanceFuturesAccount ?? throw new ArgumentNullException(nameof(binanceFuturesAccount));
    }
    
    public async Task<decimal> GetEquityAsync(string currencyPair)
    {
        var callResult = await this.BinanceFuturesAccount.GetAccountInfoAsync();
        callResult.ThrowIfHasError("Could not get the account information");
        
        var asset = callResult.Data.Assets.Where(binanceAsset => currencyPair.EndsWith(binanceAsset.Asset)).Single();
        return asset.AvailableBalance;
    }
}
