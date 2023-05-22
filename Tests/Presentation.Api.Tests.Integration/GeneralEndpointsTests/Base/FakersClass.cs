﻿using System.Reflection;

using Application.Interfaces.Services;

using Bogus;

using Bybit.Net.Enums;

using Domain.Models.Common;
using Domain.Models.Futures;

using Infrastructure;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;
[Collection("DatabaseFixture")]
public abstract class FakersClass
{
    private const int precision = 4;

    protected readonly Faker<Candlestick> CandlestickGenerator = new Faker<Candlestick>()
        .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Recent(365))
        .RuleFor(c => c.Open, f => Math.Round(f.Random.Decimal(1000, 1500), precision))
        .RuleFor(c => c.High, (f, c) => Math.Round(f.Random.Decimal(c.Open, c.Open + 100), precision))
        .RuleFor(c => c.Low, (f, c) => Math.Round(f.Random.Decimal(c.Open - 100, c.Open), precision))
        .RuleFor(c => c.Close, (f, c) => Math.Round(f.Random.Decimal(1000, 1500), precision))
        .RuleFor(c => c.Volume, f => Math.Round(f.Random.Decimal(100000, 300000), precision));

    protected readonly Faker<CurrencyPair> CurrencyPairGenerator = new Faker<CurrencyPair>()
        .CustomInstantiator(f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code));

    protected readonly Faker<FuturesOrder> FuturesOrderGenerator = new Faker<FuturesOrder>()
        .RuleFor(o => o.BybitID, f => Guid.NewGuid())
        .RuleFor(o => o.CreateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.UpdateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.Side, f => f.Random.Enum<OrderSide>())
        .RuleFor(o => o.PositionSide, f => f.Random.Enum<PositionSide>())
        .RuleFor(o => o.Type, f => f.Random.Enum<OrderType>())
        .RuleFor(o => o.Price, f => Math.Round(f.Random.Decimal(0, 1000), precision))
        .RuleFor(o => o.Quantity, f => Math.Round(f.Random.Decimal(0, 10), precision))
        .RuleFor(o => o.StopLoss, f => Math.Round(f.Random.Decimal(0, 1000), precision))
        .RuleFor(o => o.TakeProfit, f => Math.Round(f.Random.Decimal(0, 1000), precision))
        .RuleFor(o => o.TimeInForce, f => f.Random.Enum<TimeInForce>())
        .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatus>());

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