namespace Infrastructure.Tests.Integration.Common.DataGenerators;

public class PositionOpeningOrdersGenerator : AbstractFuturesDataGenerator
{
    private static readonly List<string> ruleSets =
        positionSideRules.SelectMany(
            positionSideRule => orderSideRules.SelectMany(
                orderSideRule => new[]
                {
                    $"{marketOrderRule}, {orderSideRule}, {positionSideRule}",
                    $"{limitFilledRule}, {orderSideRule}, {positionSideRule}"
                })).ToList();

    public static IEnumerable<object[]> GetRuleSetsAsObjectArrays() => ruleSets.Select(ruleSet => new[] { ruleSet });


    public override IEnumerator<object[]> GetEnumerator()
    {
        foreach (var ruleSet in ruleSets)
        {
            yield return new[] { this.FuturesOrdersGenerator.Generate($"default, {ruleSet}") };
        }
    }
}
