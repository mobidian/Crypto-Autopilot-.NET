using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;

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

    public async Task<IEnumerable<BinancePositionDetailsUsdt>> GetPositionsAsync(string currencyPair)
    {
        var callResult = await this.BinanceFuturesAccount.GetPositionInformationAsync(currencyPair);
        callResult.ThrowIfHasError(); 
        
        return callResult.Data.Where(x => x.Symbol == currencyPair.ToUpperInvariant() && x.IsolatedMargin != 0);
    }
    
    public async Task<BinancePositionDetailsUsdt?> GetPositionAsync(string currencyPair, PositionSide positionSide)
    {
        var callResult = await this.BinanceFuturesAccount.GetPositionInformationAsync(currencyPair);
        callResult.ThrowIfHasError();
        
        return callResult.Data.Where(x => x.Symbol == currencyPair.ToUpperInvariant() && x.PositionSide == positionSide && x.IsolatedMargin != 0).SingleOrDefault(defaultValue: null);
    }
}
