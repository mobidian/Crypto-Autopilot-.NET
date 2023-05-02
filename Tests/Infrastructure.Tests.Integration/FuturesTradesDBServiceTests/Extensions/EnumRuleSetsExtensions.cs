namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

public static class EnumRuleSetsExtensions
{
    public static string ToRuleSetName<T>(this T enumValue) where T : Enum => $"{typeof(T).FullName}.{enumValue}";
}
