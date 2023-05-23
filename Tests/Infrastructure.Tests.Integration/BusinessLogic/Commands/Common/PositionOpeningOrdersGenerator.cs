using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.DataAccess.Extensions;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.Common;

public class PositionOpeningOrdersGenerator : AbstractFuturesDataGenerator
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        foreach (var positionSideRule in positionSideRules)
        {
            foreach (var orderSideRule in orderSideRules)
            {
                var order1 = this.FuturesOrdersGenerator.Generate($"default, {marketOrderRule}, {orderSideRule}, {positionSideRule}");
                var order2 = this.FuturesOrdersGenerator.Generate($"default, {limitFilledRule}, {orderSideRule}, {positionSideRule}");
                
                yield return new object[] { order1 };
                yield return new object[] { order2 };
            }
        }
    }
}
