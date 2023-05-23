using Application.Interfaces.Services.DataAccess.Repositories;
using Application.Interfaces.Services.DataAccess.Services;

using Domain.Models.Futures;

using Infrastructure.Database;
using Infrastructure.Internal.Extensions;

using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.DataAccess.Services;

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
        await this.PositionsRepository.AddFuturesPositionAsync(position);
        await this.OrdersRepository.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);
    }

    public async Task UpdateFuturesPositionAndAddOrdersAsync(Guid cryptoAutopilotId, FuturesPosition updatedPosition, IEnumerable<FuturesOrder> newOrders)
    {
        var position = await this.PositionsRepository.GetFuturesOrderByCryptoAutopilotId(cryptoAutopilotId) ?? throw new ArgumentException($"There wasn't any position with cryptoAutopilotId '{cryptoAutopilotId}' in the database", nameof(cryptoAutopilotId));
        var positionCryptoAutopilotId = position.CryptoAutopilotId;
        
        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();
        await this.PositionsRepository.UpdateFuturesPositionAsync(positionCryptoAutopilotId, updatedPosition);
        if (!newOrders.IsNullOrEmpty())
            await this.OrdersRepository.AddFuturesOrdersAsync(newOrders, positionCryptoAutopilotId);
    }
}
