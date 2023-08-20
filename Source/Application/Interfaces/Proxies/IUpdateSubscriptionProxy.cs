using CryptoExchange.Net.Sockets;

namespace Application.Interfaces.Proxies;

public interface IUpdateSubscriptionProxy
{
    public void SetSubscription(UpdateSubscription subscription);

    public int SocketId { get; }
    public int Id { get; }

    public event Action ConnectionLost;
    public event Action ConnectionClosed;
    public event Action<TimeSpan> ConnectionRestored;
    public event Action ActivityPaused;
    public event Action ActivityUnpaused;
    public event Action<Exception> Exception;

    public Task CloseAsync();
    public Task ReconnectAsync();
}
