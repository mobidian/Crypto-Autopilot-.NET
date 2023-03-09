using System.Text;

using Application.Data.Mapping;

using Bogus;

using CryptoExchange.Net.CommonObjects;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class AddFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    [TestCaseSource(nameof(GetOrdersThatShouldNotHaveForeignKey))]
    public async Task AddFuturesOrdersWithoutPositionGuid_ShouldAddFuturesOrders_WhenAllOrdersShouldNotPointToPosition(string ruleSets)
    {
        // Arrange
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {ruleSets}");
        
        // Act
        await this.SUT.AddFuturesOrdersAsync(orders);

        // Assert
        this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }
    
    [Test]
    [TestCaseSource(nameof(GetOrdersThatShouldHaveForeignKey))]
    public async Task AddFuturesOrdersWithoutPositionGuid_ShouldThrow_WhenAllOrdersRequirePosition(string ruleSets)
    {
        // Arrange
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {ruleSets}");
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders);

        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerException<ArgumentException>().WithMessage("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.");
    }

    
    [Test]
    [TestCaseSource(nameof(GetOrdersThatShouldHaveForeignKey))]
    public async Task AddFuturesOrderWithPositionGuid_ShouldAddFuturesOrders_WhenAllFuturesOrdersRequirePosition(string ruleSets)
    {
        // Arrange
        var orders = this.FuturesOrderGenerator.Generate(1, $"default, {ruleSets}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {ruleSets.Split(',').Last().Trim()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        // Act
        await this.SUT.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);
        
        // Assert
        this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }
    
    [Test]
    [TestCaseSource(nameof(GetOrdersThatShouldNotHaveForeignKey))]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenAllFuturesOrdersShouldNotPointToPosition(string ruleSets)
    {
        // Arrange
        var orders = this.FuturesOrderGenerator.Generate(1, $"default, {ruleSets}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {ruleSets.Split(',').Last().Trim()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);

        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
             .WithInnerException<ArgumentException>().WithMessage("Only a created market order or a filled limit order can be associated with a position.");
    }


    [Test]
    public async Task AddFuturesOrder_ShouldThrow_WhenInputIsInconsistent()
    {
        // Arrange
        var orders1 = GetOrdersThatShouldHaveForeignKey().Select(ruleset => this.FuturesOrderGenerator.Generate($"default, {ruleset}"));
        var orders2 = GetOrdersThatShouldNotHaveForeignKey().Select(ruleset => this.FuturesOrderGenerator.Generate($"default, {ruleset}"));
        var orders = orders1.ToList();
        orders.AddRange(orders2);

        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders);

        // Assert
        var sb = new StringBuilder();
        sb.Append("Some of the specified orders can be associated with a position while some cannot. ");
        sb.Append("All the specified orders need to have the same requirements in terms of beeing associated with a position to add them in the database at once.");
        await func.Should().ThrowExactlyAsync<ArgumentException>().WithMessage($"{sb} (Parameter 'futuresOrders')");
    }
}
