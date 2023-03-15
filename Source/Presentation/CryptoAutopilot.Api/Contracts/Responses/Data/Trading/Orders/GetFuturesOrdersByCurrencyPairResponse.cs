﻿using CryptoAutopilot.Api.Contracts.Responses.Common;

namespace CryptoAutopilot.Api.Contracts.Responses.Data.Trading.Orders;

public class GetFuturesOrdersByContractNameResponse
{
    public required string ContractName { get; init; }
    public required IEnumerable<FuturesOrderResponse> FuturesOrders { get; init; }
}
