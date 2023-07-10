using Application.DataAccess.Repositories;
using Application.DataAccess.Services;

using Domain.Models.Futures;

using Infrastructure.DataAccess.Database;

using Infrastructure.Internal.Extensions;

using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.DataAccess.Services;

public class FuturesOperationsService : IFuturesOperationsService
{
    private readonly FuturesTradingDbContext DbContext;
    private readonly IFuturesPositionsRepository PositionsRepository;
    private readonly IFuturesOrdersRepository OrdersRepository;

    public FuturesOperationsService(FuturesTradingDbContext dbContext, IFuturesPositionsRepository positionsRepository, IFuturesOrdersRepository ordersRepository)
    {
        this.DbContext = dbContext;
        this.PositionsRepository = positionsRepository;
        this.OrdersRepository = ordersRepository;
    }


    public async Task AddFuturesPositionAndOrdersAsync(FuturesPosition position, IEnumerable<FuturesOrder> orders)
    {
        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();
        await this.PositionsRepository.AddAsync(position);
        await this.OrdersRepository.AddAsync(orders, position.CryptoAutopilotId);
    }
    public async Task UpdateFuturesPositionAndAddOrdersAsync(FuturesPosition updatedPosition, IEnumerable<FuturesOrder> newOrders)
    {
        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();
        await this.UpdatePositionAndInsertOrdersAsync(updatedPosition, newOrders);
    }
    public async Task UpdateFuturesPositionsAndAddTheirOrdersAsync(Dictionary<FuturesPosition, IEnumerable<FuturesOrder>> positionsOrders)
    {
        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();
        
        foreach (var positionOrder in positionsOrders)
        {
            var updatedPosition = positionOrder.Key;
            var newOrders = positionOrder.Value;
            await this.UpdatePositionAndInsertOrdersAsync(updatedPosition, newOrders);
        }
    }

    private async Task UpdatePositionAndInsertOrdersAsync(FuturesPosition updatedPosition, IEnumerable<FuturesOrder> newOrders)
    {
        await this.PositionsRepository.UpdateAsync(updatedPosition);

        if (!newOrders.IsNullOrEmpty())
            await this.OrdersRepository.AddAsync(newOrders, updatedPosition.CryptoAutopilotId);
    }
}
