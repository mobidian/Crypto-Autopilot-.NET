namespace Tests.Integration.Common.DataGenerators;

public class LimitOrdersGenerator : AbstractFuturesDataGenerator
{
    private static readonly List<string> ruleSets =
        positionSideRules.SelectMany(
            positionSideRule => orderSideRules.Select(
                orderSideRule => $"{limitOrderNotFilledRule}, {orderSideRule}, {positionSideRule}")).ToList();

    public static IEnumerable<object[]> GetRuleSetsAsObjectArrays() => ruleSets.Select(ruleSet => new[] { ruleSet });


    public override IEnumerator<object[]> GetEnumerator()
    {
        foreach (var ruleSet in ruleSets)
        {
            yield return new[] { this.FuturesOrdersGenerator.Generate($"default, {ruleSet}") };
        }
    }
}
