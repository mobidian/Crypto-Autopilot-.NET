namespace Infrastructure.Tests.Integration.DataAccess.Extensions;

public static class EnumRuleSetsExtensions
{
    /// <summary>
    /// Converts the provided <see cref="Enum"/> value to a unique rule set name by concatenating the <see cref="Enum"/> type's full name and the <see cref="Enum"/> value as a <see cref="string"/>
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> type</typeparam>
    /// <param name="enumValue">The <see cref="Enum"/> value</param>
    /// <returns>A <see cref="string"/> representing the unique rule set name</returns>
    public static string ToRuleSetName<T>(this T enumValue) where T : Enum => $"{typeof(T).FullName}.{enumValue}";
}
