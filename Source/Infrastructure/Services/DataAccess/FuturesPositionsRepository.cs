using Application.Data.Mapping;
using Application.Interfaces.Services.DataAccess;

using Domain.Models.Orders;

using Infrastructure.Database;
using Infrastructure.Internal.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.DataAccess;

public class FuturesPositionsRepository : FuturesRepository, IFuturesPositionsRepository
{
    public FuturesPositionsRepository(FuturesTradingDbContext dbContext) : base(dbContext) { }


    public async Task AddFuturesPositionAsync(FuturesPosition position, IEnumerable<FuturesOrder> futuresOrders)
    {
        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();

        var positionDbEntity = position.ToDbEntity();
        await this.DbContext.FuturesPositions.AddAsync(positionDbEntity);
        await this.DbContext.SaveChangesAsync();

        var futuresOrderDbEntities = futuresOrders.Select(x =>
        {
            var entity = x.ToDbEntity();
            entity.PositionId = positionDbEntity.Id;
            return entity;
        });
        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.ValidateAndSaveChangesAsync(); // validates the relationships as well
    }
    public async Task<IEnumerable<FuturesPosition>> GetAllFuturesPositionsAsync()
    {
        var positions = this.DbContext.FuturesPositions
            .OrderBy(x => x.CurrencyPair)
            .Select(x => x.ToDomainObject())
            .AsEnumerable();

        return await Task.FromResult(positions);
    }
    public async Task<IEnumerable<FuturesPosition>> GetFuturesPositionsByCurrencyPairAsync(string currencyPair)
    {
        var positions = this.DbContext.FuturesPositions
            .Where(x => x.CurrencyPair == currencyPair)
            .OrderBy(x => x.CurrencyPair)
            .Select(x => x.ToDomainObject())
            .AsEnumerable();

        return await Task.FromResult(positions);
    }
    public async Task UpdateFuturesPositionAsync(Guid positionId, FuturesPosition updatedPosition)
    {
        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();

        var positionDbEntity = await this.DbContext.FuturesPositions
            .Include(x => x.FuturesOrders) // Include related orders to be able to validate the relationship when saving the changes
            .Where(x => x.CryptoAutopilotId == positionId)
            .FirstOrDefaultAsync() ?? throw new DbUpdateException($"Did not find a position with crypto autopilot id {positionId}");

        positionDbEntity.CurrencyPair = updatedPosition.CurrencyPair.Name;
        positionDbEntity.Side = updatedPosition.Side;
        positionDbEntity.Margin = updatedPosition.Margin;
        positionDbEntity.Leverage = updatedPosition.Leverage;
        positionDbEntity.Quantity = updatedPosition.Quantity;
        positionDbEntity.EntryPrice = updatedPosition.EntryPrice;
        positionDbEntity.ExitPrice = updatedPosition.ExitPrice;

        await this.DbContext.ValidateAndSaveChangesAsync(); // validates the relationships as well
    }
}
