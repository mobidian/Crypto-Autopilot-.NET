namespace Infrastructure.Services.Trading.Internal.Enums;

internal enum OrderMonitoringTaskStatus
{
    Unstarted,
    Running,
    Completed,
    Cancelled,
    Faulted,
}
