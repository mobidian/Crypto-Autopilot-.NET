using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.DataAccess.Extensions;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.Common;

public class LimitOrdersGenerator : AbstractFuturesDataGenerator
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        foreach (var positionSideRule in positionSideRules)
        {
            foreach (var orderSideRule in orderSideRules)
            {
                var order = this.FuturesOrdersGenerator.Generate($"default, {limitOrderNotFilledRule}, {orderSideRule}, {positionSideRule}");
                yield return new object[] { order };
            }
        }
    }
}
