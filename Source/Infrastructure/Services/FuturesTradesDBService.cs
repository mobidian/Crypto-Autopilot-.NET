using Application.Data.Mapping;
using Application.Data.Validation;
using Application.Interfaces.Services;

using Domain.Models;

using FluentValidation;
using FluentValidation.Results;

using Infrastructure.Database.Contexts;
using Infrastructure.Database.Internal;

using Microsoft.EntityFrameworkCore;

using static Application.Data.Validation.FuturesOrdersConsistencyValidator;
using static Application.Data.Validation.FuturesOrderValidator;

namespace Infrastructure.Services;

public class FuturesTradesDBService : IFuturesTradesDBService
{
    private static readonly FuturesOrderValidator OrderValidator = new();
    private static readonly FuturesOrdersConsistencyValidator OrdersConsistencyValidator = new();
    private static readonly FuturesPositionValidator PositionValidator = new();

    private readonly FuturesTradingDbContext DbContext;
    public FuturesTradesDBService(FuturesTradingDbContext dbContext)
    {
        this.DbContext = dbContext;
    }


    public async Task AddFuturesOrderAsync(FuturesOrder futuresOrder, Guid? positionId = null)
    {
        _ = futuresOrder ?? throw new ArgumentNullException(nameof(futuresOrder));
        
        var validation = await OrderValidator.ValidateAsync(futuresOrder, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(OrderOpenedPosition));
        if (validation.IsValid && positionId is null)
        {
            var innerException = new ArgumentException("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.", new ValidationException(validation.Errors));
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }
        else if (!validation.IsValid && positionId is not null)
        {
            var innerException = new ArgumentException("Only a created market order or a filled limit order can be associated with a position.", new ValidationException(validation.Errors));
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }


        var entity = futuresOrder.ToDbEntity();
        if (positionId is not null)
        {
            var position = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).SingleAsync();
            if (position.Side != futuresOrder.PositionSide)
            {
                var innerException = new ArgumentException($"The position side of the order did not match the side of the position with CryptoAutopilotId {positionId}");
                throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
            }

            entity.PositionId = position.Id;
        }

        using var transaction = await this.BeginTransactionAsync();

        await this.DbContext.FuturesOrders.AddAsync(entity);
        await this.DbContext.SaveChangesAsync();
    }
    public async Task AddFuturesOrdersAsync(IEnumerable<FuturesOrder> futuresOrders, Guid? positionId = null)
    {
        var validationResult = positionId switch
        {
            null => await OrdersConsistencyValidator.ValidateAsync(futuresOrders, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(NoOrderOpensPosition)),
            _ => await OrdersConsistencyValidator.ValidateAsync(futuresOrders, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(AllOrdersOpenPosition)),
        };

        if (!validationResult.IsValid)
        {
            var innerException = new ValidationException(validationResult.Errors);
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }



        var futuresOrderDbEntities = futuresOrders.Select(x => x.ToDbEntity()).ToArray();
        if (positionId is not null)
        {
            var positionDbEntity = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).SingleAsync();
            if (positionDbEntity.Side != futuresOrders.First().PositionSide)
            {
                var innerException = new ArgumentException($"The position side of the orders did not match the side of the position with CryptoAutopilotId {positionId}");
                throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
            }

            foreach (var futuresOrderDbEntity in futuresOrderDbEntities)
                futuresOrderDbEntity.PositionId = positionDbEntity.Id;
        }


        using var transaction = await this.BeginTransactionAsync();

        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.SaveChangesAsync();
    }
    public async Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync()
    {
        var orders = this.DbContext.FuturesOrders
            .OrderBy(x => x.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .AsEnumerable();
        
        return await Task.FromResult(orders);
    }
    public async Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair)
    {
        var orders = this.DbContext.FuturesOrders
            .Where(x => x.CurrencyPair == currencyPair)
            .OrderBy(x => x.CurrencyPair)
            .OrderByDescending(x => x.CreateTime)
            .Select(x => x.ToDomainObject())
            .AsEnumerable();
        
        return await Task.FromResult(orders);
    }
    public async Task UpdateFuturesOrderAsync(Guid bybitID, FuturesOrder updatedFuturesOrder, Guid? positionId = null)
    {
        _ = updatedFuturesOrder ?? throw new ArgumentNullException(nameof(updatedFuturesOrder));

        var validation = await OrderValidator.ValidateAsync(updatedFuturesOrder, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(OrderOpenedPosition));
        if (validation.IsValid && positionId is null)
        {
            var innerException = new ArgumentException("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.", new ValidationException(validation.Errors));
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }
        else if (!validation.IsValid && positionId is not null)
        {
            var innerException = new ArgumentException("Only a created market order or a filled limit order can be associated with a position.", new ValidationException(validation.Errors));
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }
         

        using var transaction = await this.BeginTransactionAsync();

        var dbEntity = await this.DbContext.FuturesOrders.Where(x => x.BybitID == bybitID).SingleOrDefaultAsync() ?? throw new DbUpdateException($"Could not find futures order with uniqueID == {bybitID}");
        if (positionId is not null)
        {
            var position = await this.DbContext.FuturesPositions.Where(x => x.CryptoAutopilotId == positionId).SingleAsync();
            if (position.Side != updatedFuturesOrder.PositionSide)
            {
                var innerException = new ArgumentException($"The position side of the order did not match the side of the position with CryptoAutopilotId {positionId}");
                throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
            }

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

    public async Task AddFuturesPositionAsync(FuturesPosition position, IEnumerable<FuturesOrder> futuresOrders)
    {
        var positionValidationResult = await PositionValidator.ValidateAsync(position);
        if(!positionValidationResult.IsValid)
        {
            var innerException = new ValidationException(positionValidationResult.Errors);
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }

        var ordersValidationResult = await OrdersConsistencyValidator.ValidateAsync(futuresOrders, x => x.IncludeRulesNotInRuleSet().IncludeRuleSets(AllOrdersOpenPosition));
        if (!ordersValidationResult.IsValid)
        {
            var innerException = new ValidationException(ordersValidationResult.Errors);
            throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
        }


        var positionDbEntity = position.ToDbEntity();
        var futuresOrderDbEntities = futuresOrders.Select(x => x.ToDbEntity()).ToArray();
        foreach (var futuresOrderDbEntity in futuresOrderDbEntities)
        {
            if (futuresOrderDbEntity.PositionSide != positionDbEntity.Side)
            {
                var innerException = new ArgumentException($"The position side of the specified orders does not match the side of the specified position");
                throw new DbUpdateException("An error occurred while saving the entity changes. See the inner exception for details.", innerException);
            }
        }


        using var transaction = await this.BeginTransactionAsync();

        await this.DbContext.FuturesPositions.AddAsync(positionDbEntity);
        await this.DbContext.SaveChangesAsync();
        
        foreach (var futuresOrderDbEntity in futuresOrderDbEntities)
            futuresOrderDbEntity.PositionId = positionDbEntity.Id;

        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.SaveChangesAsync();
    }
    

    private async Task<TransactionalOperation> BeginTransactionAsync()
        => new TransactionalOperation(await this.DbContext.Database.BeginTransactionAsync());
}
