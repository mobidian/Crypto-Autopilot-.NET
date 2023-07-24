using System.Reflection;

using Application.Strategies;

using Bogus;

using Bybit.Net.Enums;

using Domain.Models.Common;

using Infrastructure;

using Tests.Integration.Common.Fakers;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

public abstract class FakersClass : FuturesDataFakersClass
{
    protected readonly Faker<IStrategyEngine> StrategyEnginesGenerator = new Faker<IStrategyEngine>()
        .CustomInstantiator(f =>
        {
            var types = typeof(IInfrastructureMarker).Assembly.DefinedTypes.Where(typeInfo => !typeInfo.IsInterface && !typeInfo.IsAbstract && typeof(IStrategyEngine).IsAssignableFrom(typeInfo));
            var type = f.PickRandom(types);

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var parameters = new object[] { Guid.NewGuid(), new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code), f.PickRandom<KlineInterval>() };
            return (IStrategyEngine)Activator.CreateInstance(type, flags, null, parameters, null)!;
        });


    protected static CurrencyPair GetRandomCurrencyPairExcept(Faker f, CurrencyPair currencyPair)
    {
        CurrencyPair newCurrencyPair;
        do
        {
            newCurrencyPair = new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code);
        }
        while (newCurrencyPair == currencyPair);

        return newCurrencyPair;
    }
}
