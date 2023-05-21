using Application.Data.Mapping;

using Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests;

public class UpdateTests : TradingSignalsRepositoryTestsBase
{
    [Test]
    public async Task Update_ShouldUpdateTradingSignal_WhenTradingSignalExistsAndUpdatedSignalIsValid()
    {
        // Arrange
        var tradingSignal = this.TradingSignalGenerator.Generate();
        await this.SUT.AddAsync(tradingSignal);
        var updatedSignal = this.TradingSignalGenerator.Clone().RuleFor(x => x.CryptoAutopilotId, tradingSignal.CryptoAutopilotId).Generate();

        // Act
        var result = await this.SUT.UpdateAsync(updatedSignal);

        // Assert
        result.Should().BeTrue();
        this.ArrangeAssertDbContext.TradingSignals.Single().ToDomainObject().Should().BeEquivalentTo(updatedSignal);
    }
    
    [Test]
    public async Task Update_ShouldUpdateTradingSignal_WhenTradingSignalDoesNotExist()
    {
        // Arrange
        var updatedSignal = this.TradingSignalGenerator.Generate();

        // Act
        var func = async () => await this.SUT.UpdateAsync(updatedSignal);

        // Assert
        await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage($"Could not find futures order with cryptoAutopilotId == {updatedSignal.CryptoAutopilotId}");
    }
}
