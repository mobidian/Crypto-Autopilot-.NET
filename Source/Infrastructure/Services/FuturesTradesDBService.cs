using System.Text;

using Application.Data.Mapping;
using Application.Interfaces.Services;

using Bybit.Net.Enums;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Database.Internal;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class FuturesTradesDBService : IFuturesTradesDBService
{
    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext) => this.DbContext = dbContext;


    public async Task AddFuturesOrderAsync(FuturesOrder futuresOrder, Guid? positionId = null)
    {
        _ = futuresOrder ?? throw new ArgumentNullException(nameof(futuresOrder));
        if (OrderShouldBeAssignedToPosition(futuresOrder) && positionId is null)
        {
            var innerException = new ArgumentException("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.");
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }
        else if (!OrderShouldBeAssignedToPosition(futuresOrder) && positionId is not null)
        {
            var innerException = new ArgumentException("Only a created market order or a filled limit order can be associated with a position.");
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }


        using var transaction = await this.BeginTransactionAsync();

        var entity = futuresOrder.ToDbEntity();
        if (positionId is not null)
        {
            var position = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).SingleAsync();
            entity.PositionId = position.Id;
        }

        await this.DbContext.FuturesOrders.AddAsync(entity);
        await this.DbContext.SaveChangesAsync();
    }
    public async Task AddFuturesOrdersAsync(IEnumerable<FuturesOrder> futuresOrders, Guid? positionId = null)
    {
        _ = futuresOrders ?? throw new ArgumentNullException(nameof(futuresOrders));


        var allShouldBeAssignedPosition = futuresOrders.All(OrderShouldBeAssignedToPosition);
        var noneShouldBeAssignedPosition = futuresOrders.All(OrderShouldNotBeAssignedToPosition);
        
        // this check is here because all the specified orders should either be with a position or without a position 
        if (!allShouldBeAssignedPosition && !noneShouldBeAssignedPosition)
        {
            var sb = new StringBuilder();
            sb.Append("Some of the specified orders can be associated with a position while some cannot. ");
            sb.Append("All the specified orders need to have the same requirements in terms of beeing associated with a position to add them in the database at once.");
            throw new ArgumentException(sb.ToString(), nameof(futuresOrders));
        }
        
        if (allShouldBeAssignedPosition && positionId is null)
        {
            var innerException = new ArgumentException("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.");
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }
        else if (noneShouldBeAssignedPosition && positionId is not null)
        {
            var innerException = new ArgumentException("Only a created market order or a filled limit order can be associated with a position.");
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }
                


        using var transaction = await this.BeginTransactionAsync();

        var futuresOrderDbEntities = futuresOrders.Select(order => order.ToDbEntity());
        if (positionId is not null)
        {
            var position = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).SingleAsync();

            foreach (var item in futuresOrderDbEntities)
                item.PositionId = position.Id;
        }

        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.SaveChangesAsync();
    }
    public async Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync()
    {
        return await this.DbContext.FuturesOrders
            .OrderBy(x => x.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }
    public async Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair)
    {
        return await this.DbContext.FuturesOrders
            .Where(x => x.CurrencyPair == currencyPair)
            .OrderBy(x => x.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .ToListAsync();
    }
    public async Task UpdateFuturesOrderAsync(Guid bybitID, FuturesOrder updatedFuturesOrder, Guid? positionId = null)
    {
        _ = updatedFuturesOrder ?? throw new ArgumentNullException(nameof(updatedFuturesOrder));
        if (OrderShouldBeAssignedToPosition(updatedFuturesOrder) && positionId is null)
        {
            var innerException = new ArgumentException("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.");
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }
        else if (!OrderShouldBeAssignedToPosition(updatedFuturesOrder) && positionId is not null)
        {
            var innerException = new ArgumentException("Only a created market order or a filled limit order can be associated with a position.");
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }


        using var transaction = await this.BeginTransactionAsync();

        var dbEntity = await this.DbContext.FuturesOrders.Where(x => x.BybitID == bybitID).SingleOrDefaultAsync() ?? throw new DbUpdateException($"Could not find futures order with uniqueID == {bybitID}");
        if (positionId is not null)
        {
            var position = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).SingleAsync();
            dbEntity.PositionId = position.Id;
        }
        
        dbEntity.CurrencyPair = updatedFuturesOrder.CurrencyPair.Name;
        dbEntity.BybitID = updatedFuturesOrder.BybitID;
        dbEntity.CreateTime = updatedFuturesOrder.CreateTime;
        dbEntity.UpdateTime = updatedFuturesOrder.UpdateTime;
        dbEntity.Side = updatedFuturesOrder.Side;
        dbEntity.PositionSide = updatedFuturesOrder.PositionSide;
        dbEntity.Type = updatedFuturesOrder.Type;
        dbEntity.Price = updatedFuturesOrder.Price;
        dbEntity.Quantity = updatedFuturesOrder.Quantity;
        dbEntity.StopLoss = updatedFuturesOrder.StopLoss;
        dbEntity.TakeProfit = updatedFuturesOrder.TakeProfit;
        dbEntity.TimeInForce = updatedFuturesOrder.TimeInForce;
        dbEntity.Status = updatedFuturesOrder.Status;

        await this.DbContext.SaveChangesAsync();
    }
    public async Task DeleteFuturesOrderAsync(Guid bybitID)
    {
        using var _ = await this.BeginTransactionAsync();
        
        var order = await this.DbContext.FuturesOrders.Where(x => x.BybitID == bybitID).SingleOrDefaultAsync() ?? throw new DbUpdateException($"No order with bybitID {bybitID} was found in the database");
        this.DbContext.Remove(order);
        await this.DbContext.SaveChangesAsync();
    }



    private static bool OrderShouldBeAssignedToPosition(FuturesOrder futuresOrder)
    {
        var marketCreated = futuresOrder.Type == OrderType.Market && futuresOrder.Status == OrderStatus.Created;
        var limitFilled = futuresOrder.Type == OrderType.Market && futuresOrder.Status == OrderStatus.Filled;
        return marketCreated || limitFilled;
    }
    private static bool OrderShouldNotBeAssignedToPosition(FuturesOrder futuresOrder)
    {
        return !OrderShouldBeAssignedToPosition(futuresOrder);
    }

    private async Task<TransactionalOperation> BeginTransactionAsync()
        => new TransactionalOperation(await this.DbContext.Database.BeginTransactionAsync());
}
