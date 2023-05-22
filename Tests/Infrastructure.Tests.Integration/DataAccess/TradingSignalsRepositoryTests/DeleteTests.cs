using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests;

public class DeleteTests : TradingSignalsRepositoryTestsBase
{
    public DeleteTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Delete_ShouldDeleteTradingSignal_WhenTradingSignalWithCryptoAutopilotIdExists()
    {
        // Arrange
        var tradingSignal = this.TradingSignalGenerator.Generate();
        await this.SUT.AddAsync(tradingSignal);

        // Act
        var result = await this.SUT.DeleteAsync(tradingSignal.CryptoAutopilotId);

        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task Delete_ShouldTHrow_WhenTradingSignalWithCryptoAutopilotIdDoesNotExist()
    {
        // Arrange
        var cryptoAutopilotId = Guid.NewGuid();

        // Act
        var func = async () => await this.SUT.DeleteAsync(cryptoAutopilotId);

        // Assert
        await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage($"No trading sigal with with cryptoAutopilotId '{cryptoAutopilotId}' was found in the database");
    }
}
