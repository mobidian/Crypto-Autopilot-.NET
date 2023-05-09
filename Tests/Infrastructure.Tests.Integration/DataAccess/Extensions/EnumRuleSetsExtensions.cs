namespace Infrastructure.Tests.Integration.DataAccess.Extensions;

public static class EnumRuleSetsExtensions
{
    /// <summary>
    /// Converts the provided enum value to a unique rule set name by concatenating the enum type's full name and the enum value as a string
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="enumValue">The enum value</param>
    /// <returns>A string representing the unique rule set name</returns>
    public static string ToRuleSetName<T>(this T enumValue) where T : Enum => $"{typeof(T).FullName}.{enumValue}";
}
