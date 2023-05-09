using Application.Data.Mapping;
using Application.Interfaces.Services.DataAccess;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Internal.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.DataAccess;

public class FuturesOrdersRepository : FuturesRepository, IFuturesOrdersRepository
{
    public FuturesOrdersRepository(FuturesTradingDbContext dbContext) : base(dbContext) { }


    public async Task AddFuturesOrderAsync(FuturesOrder futuresOrder, Guid? positionId = null)
    {
        var entity = futuresOrder.ToDbEntity();
        if (positionId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).FirstAsync();
            entity.PositionId = positionDbEntity.Id;
        }

        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();

        await this.DbContext.FuturesOrders.AddAsync(entity);
        await this.DbContext.ValidateAndSaveChangesAsync(); // validates the relationships as well
    }
    public async Task AddFuturesOrdersAsync(IEnumerable<FuturesOrder> futuresOrders, Guid? positionId = null)
    {
        var futuresOrderDbEntities = futuresOrders.Select(x => x.ToDbEntity()).ToArray();
        if (positionId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).FirstAsync();

            foreach (var futuresOrderDbEntity in futuresOrderDbEntities)
                futuresOrderDbEntity.PositionId = positionDbEntity.Id;
        }


        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();

        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.ValidateAndSaveChangesAsync(); // validates the relationships as well
    }
    public async Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync()
    {
        var orders = this.DbContext.FuturesOrders
            .OrderBy(x => x.CurrencyPair)
            .ThenByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .AsEnumerable();

        return await Task.FromResult(orders);
    }
    public async Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair)
    {
        var orders = this.DbContext.FuturesOrders
            .Where(x => x.CurrencyPair == currencyPair)
            .OrderBy(x => x.CurrencyPair)
            .ThenByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .AsEnumerable();

        return await Task.FromResult(orders);
    }
    public async Task UpdateFuturesOrderAsync(Guid bybitID, FuturesOrder updatedFuturesOrder, Guid? positionId = null)
    {
        var dbEntity = await this.DbContext.FuturesOrders
            .Include(x => x.Position) // Include related position to be able to validate the relationship when saving the changes
            .Where(x => x.BybitID == bybitID)
            .FirstOrDefaultAsync() ?? throw new DbUpdateException($"Could not find futures order with uniqueID == {bybitID}");

        if (positionId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).FirstAsync();
            dbEntity.PositionId = positionDbEntity.Id;
        }


        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();

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

        await this.DbContext.ValidateAndSaveChangesAsync(); // validates the relationships as well
    }
    public async Task DeleteFuturesOrdersAsync(params Guid[] bybitIDs)
    {
        foreach (var bybitID in bybitIDs)
            if (await this.DbContext.FuturesOrders.FirstOrDefaultAsync(x => x.BybitID == bybitID) is null)
                throw new DbUpdateException($"No order with bybitID {bybitID} was found in the database");


        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();

        var orders = this.DbContext.FuturesOrders.Where(x => bybitIDs.Contains(x.BybitID));
        this.DbContext.FuturesOrders.RemoveRange(orders);
        await this.DbContext.ValidateAndSaveChangesAsync(); // validates the relationships as well
    }
}
