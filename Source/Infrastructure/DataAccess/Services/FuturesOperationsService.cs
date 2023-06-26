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
        await this.OrdersRepository.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);
    }

    public async Task UpdateFuturesPositionAndAddOrdersAsync(FuturesPosition updatedPosition, IEnumerable<FuturesOrder> newOrders)
    {
        var cryptoAutopilotId = updatedPosition.CryptoAutopilotId;
        var position = await this.PositionsRepository.GetByCryptoAutopilotId(cryptoAutopilotId) ?? throw new ArgumentException($"There wasn't any position with cryptoAutopilotId '{cryptoAutopilotId}' in the database", nameof(cryptoAutopilotId));
        var positionCryptoAutopilotId = position.CryptoAutopilotId;

        using var _ = await this.DbContext.Database.BeginTransactionalOperationAsync();
        await this.PositionsRepository.UpdateAsync(positionCryptoAutopilotId, updatedPosition);
        if (!newOrders.IsNullOrEmpty())
            await this.OrdersRepository.AddFuturesOrdersAsync(newOrders, positionCryptoAutopilotId);
    }
}
