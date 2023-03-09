using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class AddFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    [TestCaseSource(nameof(GetRuleSetsForLimitOrders))]
    public async Task AddFuturesOrderWithoutPositionGuid_ShouldAddFuturesOrder_WhenOrderShouldNotPointToPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        
        // Act
        await this.SUT.AddFuturesOrderAsync(order);

        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }
        
    [Test]
    [TestCaseSource(nameof(GetRuleSetsForMarketOrders))]
    public async Task AddFuturesOrderWithoutPositionGuid_ShouldThrow_WhenOrderRequiresPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerExceptionExactly<ArgumentException>().WithMessage("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.")
            .WithInnerExceptionExactly<FluentValidation.ValidationException>();
    }

    
    [Test]
    [TestCaseSource(nameof(GetRuleSetsForMarketOrders))]
    public async Task AddFuturesOrderWithPositionGuid_ShouldAddFuturesOrder_WhenOrderRequiresPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {ruleSets.Split(',').Last().Trim()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        // Act
        await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);

        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }

    [Test]
    [TestCaseSource(nameof(GetRuleSetsForLimitOrders))]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenOrderShouldNotPointToPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {ruleSets.Split(',').Last().Trim()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);
        
        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerExceptionExactly<ArgumentException>().WithMessage("Only a created market order or a filled limit order can be associated with a position.")
            .WithInnerExceptionExactly<FluentValidation.ValidationException>(); 
    }
}
