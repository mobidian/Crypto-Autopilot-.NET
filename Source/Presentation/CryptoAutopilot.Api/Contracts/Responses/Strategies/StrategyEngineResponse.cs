﻿namespace CryptoAutopilot.Api.Contracts.Responses.Strategies;

public class StrategyEngineResponse
{
    public required Guid Guid { get; init; }
    public required string StartedStrategyTypeName { get; init; }
    public required bool IsRunning { get; init; }
}
