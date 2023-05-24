using Application.Data.Mapping;
using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Models.Futures;

using Infrastructure.Services.DataAccess.Database;
using Infrastructure.Services.DataAccess.Repositories.Abstract;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.DataAccess.Repositories;

public class FuturesPositionsRepository : FuturesRepository, IFuturesPositionsRepository
{
    public FuturesPositionsRepository(FuturesTradingDbContext dbContext) : base(dbContext) { }


    public async Task AddFuturesPositionAsync(FuturesPosition position)
    {
        var positionDbEntity = position.ToDbEntity();
        await this.DbContext.FuturesPositions.AddAsync(positionDbEntity);
        await this.DbContext.SaveChangesAsync();
    }
    public async Task AddFuturesPositionsAsync(IEnumerable<FuturesPosition> positions)
    {
        var positionsDbEntities = positions.Select(x => x.ToDbEntity());
        await this.DbContext.FuturesPositions.AddRangeAsync(positionsDbEntities);
        await this.DbContext.SaveChangesAsync();
    }
    
    public async Task<FuturesPosition?> GetFuturesOrderByCryptoAutopilotId(Guid cryptoAutopilotId)
    {
        var positionDbEntity = await this.DbContext.FuturesPositions.FirstOrDefaultAsync(x => x.CryptoAutopilotId == cryptoAutopilotId);
        return positionDbEntity?.ToDomainObject();
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

        await this.DbContext.SaveChangesAsync();
    }

    public async Task DeleteFuturesPositionsAsync(params Guid[] cryptoAutopilotIDs)
    {
        foreach (var cryptoAutopilotID in cryptoAutopilotIDs)
            if (await this.DbContext.FuturesPositions.FirstOrDefaultAsync(x => x.CryptoAutopilotId == cryptoAutopilotID) is null)
                throw new DbUpdateException($"No order with bybitID {cryptoAutopilotID} was found in the database");

        var orders = this.DbContext.FuturesPositions.Where(x => cryptoAutopilotIDs.Contains(x.CryptoAutopilotId));
        this.DbContext.FuturesPositions.RemoveRange(orders);
        await this.DbContext.SaveChangesAsync();
    }
}
