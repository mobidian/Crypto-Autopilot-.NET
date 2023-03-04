namespace Infrastructure.Services.Trading.Binance.Internal.Enums;

internal enum OrderMonitoringTaskStatus
{
    Unstarted,
    Running,
    Completed,
    Cancelled,
    Faulted,
}
