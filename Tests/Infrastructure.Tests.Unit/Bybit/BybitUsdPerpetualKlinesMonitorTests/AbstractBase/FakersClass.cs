using Bogus;

using Bybit.Net.Enums;


namespace Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests.AbstractBase;

public abstract class FakersClass
{
    protected readonly Faker Faker = new Faker();

    protected readonly Faker<string> CurrencyPairFaker = new Faker<string>()
        .CustomInstantiator(f => $"{f.Finance.Currency().Code}{f.Finance.Currency().Code}");


    protected (string currencyPair, KlineInterval timeframe) GetRandomContractIdentifier()
    {
        var currencyPair = this.CurrencyPairFaker.Generate();
        var timeframe = this.Faker.PickRandom<KlineInterval>();
        return (currencyPair, timeframe);
    }
    protected (string currencyPair, KlineInterval timeframe) GetRandomContractIdentifierExcept(IEnumerable<(string currencyPair, KlineInterval timeframe)> values)
    {
        (string currencyPair, KlineInterval timeframe) contractIdentifier;
        do
        {
            contractIdentifier = this.GetRandomContractIdentifier();
        } while (values.Contains(contractIdentifier));

        return contractIdentifier;
    }
    protected List<(string currencyPair, KlineInterval timeframe)> GetRandomContractIdentifiers(int n)
    {
        var list = new List<(string currencyPair, KlineInterval timeframe)>();

        for (var i = 0; i < n; i++)
            list.Add(this.GetRandomContractIdentifier());

        return list;
    }
}