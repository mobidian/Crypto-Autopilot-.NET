using Application.Data.Mapping;
using Application.Interfaces.Services;

using Domain.Models;
using Domain.Validation;

using FluentValidation;

using Infrastructure.Database.Contexts;
using Infrastructure.Database.Internal;

using Microsoft.EntityFrameworkCore;

using static Domain.Validation.FuturesOrdersConsistencyValidator;
using static Domain.Validation.FuturesOrderValidator;

namespace Infrastructure.Services;

public class FuturesTradesDBService : IFuturesTradesDBService
{
    private static readonly FuturesOrderValidator OrderValidator = new();
    private static readonly FuturesOrdersConsistencyValidator OrdersConsistencyValidator = new();
    private static readonly FuturesPositionValidator PositionValidator = new();
    private static readonly RelatedFuturesPositionAndOrdersValidator PositionAndOrdersValidator = new();

    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext)
    {
        this.DbContext = dbContext;
    }

    
    public async Task AddFuturesOrderAsync(FuturesOrder futuresOrder, Guid? positionId = null)
    {
        var ruleSet = positionId is null ? OrderDidNotOpenPosition : OrderOpenedPosition;
        await OrderValidator.ValidateAsync(futuresOrder, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(ruleSet).ThrowOnFailures());


        var entity = futuresOrder.ToDbEntity();
        if (positionId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).FirstAsync();
            await PositionAndOrdersValidator.ValidateAndThrowAsync((positionDbEntity.ToDomainObject(), new[] { futuresOrder }));

            entity.PositionId = positionDbEntity.Id;
        }

        using var transaction = await this.BeginTransactionAsync();

        await this.DbContext.FuturesOrders.AddAsync(entity);
        await this.DbContext.ValidateAndSaveChangesAsync();
    }
    public async Task AddFuturesOrdersAsync(IEnumerable<FuturesOrder> futuresOrders, Guid? positionId = null)
    {
        var ruleSet = positionId is null ? NoOrderOpensPosition : AllOrdersOpenPosition;
        await OrdersConsistencyValidator.ValidateAsync(futuresOrders, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(ruleSet).ThrowOnFailures());
        
        var futuresOrderDbEntities = futuresOrders.Select(x => x.ToDbEntity()).ToArray();
        if (positionId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).FirstAsync();
            await PositionAndOrdersValidator.ValidateAndThrowAsync((positionDbEntity.ToDomainObject(), futuresOrders));

            foreach (var futuresOrderDbEntity in futuresOrderDbEntities)
                futuresOrderDbEntity.PositionId = positionDbEntity.Id;
        }


        using var transaction = await this.BeginTransactionAsync();

        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.ValidateAndSaveChangesAsync();
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
        _ = updatedFuturesOrder ?? throw new ArgumentNullException(nameof(updatedFuturesOrder));

        var ruleSet = positionId is not null ? OrderOpenedPosition : OrderDidNotOpenPosition;
        await OrderValidator.ValidateAsync(updatedFuturesOrder, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(ruleSet).ThrowOnFailures());

        var dbEntity = await this.DbContext.FuturesOrders.Where(x => x.BybitID == bybitID).FirstOrDefaultAsync() ?? throw new DbUpdateException($"Could not find futures order with uniqueID == {bybitID}");
        if (positionId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).FirstAsync();
            await PositionAndOrdersValidator.ValidateAndThrowAsync((positionDbEntity.ToDomainObject(), new[] { updatedFuturesOrder }));

            dbEntity.PositionId = positionDbEntity.Id;
        }
        

        using var transaction = await this.BeginTransactionAsync();

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

        await this.DbContext.ValidateAndSaveChangesAsync();
    }
    public async Task DeleteFuturesOrderAsync(Guid bybitID)
    {
        using var _ = await this.BeginTransactionAsync();

        var order = await this.DbContext.FuturesOrders.Where(x => x.BybitID == bybitID).FirstOrDefaultAsync() ?? throw new DbUpdateException($"No order with bybitID {bybitID} was found in the database");
        this.DbContext.FuturesOrders.Remove(order);
        await this.DbContext.ValidateAndSaveChangesAsync();
    }

    public async Task AddFuturesPositionAsync(FuturesPosition position, IEnumerable<FuturesOrder> futuresOrders)
    {
        await PositionAndOrdersValidator.ValidateAndThrowAsync((position, futuresOrders));

        
        using var transaction = await this.BeginTransactionAsync();
        
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
        await this.DbContext.ValidateAndSaveChangesAsync();
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
        await PositionValidator.ValidateAndThrowAsync(updatedPosition);

        
        using var transaction = await this.BeginTransactionAsync();

        var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).FirstOrDefaultAsync() ?? throw new DbUpdateException($"Did not find a position with crypto autopilot id {positionId}");
        await PositionAndOrdersValidator.ValidateAndThrowAsync((updatedPosition, positionDbEntity.FuturesOrders!.Select(x => x.ToDomainObject())));
        
        positionDbEntity.CurrencyPair = updatedPosition.CurrencyPair.Name;
        positionDbEntity.Side = updatedPosition.Side;
        positionDbEntity.Margin = updatedPosition.Margin;
        positionDbEntity.Leverage = updatedPosition.Leverage;
        positionDbEntity.Quantity = updatedPosition.Quantity;
        positionDbEntity.EntryPrice = updatedPosition.EntryPrice;
        positionDbEntity.ExitPrice = updatedPosition.ExitPrice;
        
        await this.DbContext.ValidateAndSaveChangesAsync();
    }

    private async Task<TransactionalOperation> BeginTransactionAsync()
        => new TransactionalOperation(await this.DbContext.Database.BeginTransactionAsync());
}
