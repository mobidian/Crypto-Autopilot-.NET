using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class UpdateFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task UpdateFuturesOrder_ShouldUpdateFuturesOrder_WhenFuturesOrderExistsAsync()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, candlestick.CurrencyPair).GenerateBetween(1, 5);
        await this.InsertOneCandlestickAndMultipleFuturesOrdersAsync(candlestick, futuresOrders);
        
        // Act
        var uniqueID = futuresOrders[Random.Shared.Next(futuresOrders.Count)].UniqueID;
        var newFuturesOrderValue = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, candlestick.CurrencyPair).Generate();
        await this.SUT.UpdateFuturesOrderAsync(uniqueID, newFuturesOrderValue);

        // Assert
        (await this.SUT.GetAllFuturesOrdersAsync()).Should().ContainEquivalentOf(newFuturesOrderValue);
    }
    
    [Test]
    public async Task UpdateFuturesOrder_ShouldThrow_WhenFuturesOrderDoesNotExist()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var futuresOrder = this.FuturesOrderGenerator.Generate();
        
        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(guid, futuresOrder);

        // Assert
        await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage($"Could not find futures order with uniqueID == {guid}");
    }
}
