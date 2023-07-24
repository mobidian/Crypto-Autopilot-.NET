using Domain.Models.Futures;

using Tests.Integration.Common.Fakers;

namespace Tests.Integration.Common.DataGenerators;

/// <summary>
/// <para>
/// Generates a set of valid <see cref="FuturesPosition"/> instances along with their related <see cref="FuturesOrder"/> collections for use in test scenarios.
/// This class inherits from the <see cref="FuturesDataFakersClass"/> and implements <see cref="IEnumerable{T}"/> 
/// to provide a collection of valid futures positions and their associated futures orders.
/// </para>
/// <para>
/// All combinations of position sides and order sides are generated,
/// however, the <see cref="FuturesPosition.Side"/> always matches the <see cref="FuturesOrder.Side"/>, seeing as otherwise the combination wouldn't be valid.
/// </para>
/// </summary>
public class ValidPositionAndOrdersGenerator : AbstractFuturesDataGenerator
{
    public override IEnumerator<object[]> GetEnumerator()
    {
        foreach (var positionSideRule in positionSideRules)
        {
            foreach (var orderSideRule in orderSideRules)
            {
                var position1 = this.FuturesPositionsGenerator.Generate($"default, {positionSideRule}");
                var orders1 = this.FuturesOrdersGenerator.Generate(3, $"default, {marketOrderRule}, {orderSideRule}, {positionSideRule}");

                var position2 = this.FuturesPositionsGenerator.Generate($"default, {positionSideRule}");
                var orders2 = this.FuturesOrdersGenerator.Generate(3, $"default, {limitFilledRule}, {orderSideRule}, {positionSideRule}");

                yield return new object[] { position1, orders1 };
                yield return new object[] { position2, orders2 };
            }
        }
    }
}
