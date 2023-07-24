using Bogus;

namespace Tests.Integration.Common.DataAccess.Extensions;

public static class RandomizerExtensions
{
    public static decimal Decimal(this Randomizer randomizer, decimal min, decimal max, int decimals) => Math.Round(randomizer.Decimal(min, max), decimals);
}
