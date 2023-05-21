using Application.Data.Mapping;

using Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests;

public class AddTests : TradingSignalsRepositoryTestsBase
{
    [Test]
    public async Task Add_ShouldAddSignal_WhenSignalDoesNotExist()
    {
        // Arrange
        var tradingSignal = this.TradingSignalGenerator.Generate();

        // Act
        var result = await this.SUT.AddAsync(tradingSignal);

        // Assert
        result.Should().BeTrue();
        this.ArrangeAssertDbContext.TradingSignals.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(new[] { tradingSignal });
    }

    [Test]
    public async Task Add_ShouldThrow_WhenSignalWithCryptoAutopilotIdAlreadyExists()
    {
        // Arrange
        var tradingSignal = this.TradingSignalGenerator.Generate();
        await this.SUT.AddAsync(tradingSignal);

        var cryptoAutopilotId = tradingSignal.CryptoAutopilotId;
        var tradingSignalWithSameId = this.TradingSignalGenerator.Clone().RuleFor(x => x.CryptoAutopilotId, cryptoAutopilotId).Generate();


        // Act
        var func = async () => await this.SUT.AddAsync(tradingSignalWithSameId);


        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerExceptionExactly<SqlException>()
            .WithMessage($"""Cannot insert duplicate key row in object 'dbo.TradingSignals' with unique index 'IX_TradingSignals_CryptoAutopilotId'. The duplicate key value is ({cryptoAutopilotId}).""");
    }
}
