﻿namespace CryptoAutopilot.Contracts.Responses.Strategies;

public class StrategyEngineStoppedResponse
{
    public required Guid Guid { get; init; }
    public required string StoppedStrategyTypeName { get; init; }
    public required DateTime StopTime { get; init; }
}
