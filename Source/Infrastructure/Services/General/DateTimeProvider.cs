using Application.Interfaces.Services.General;

namespace Infrastructure.Services.General;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;
}
