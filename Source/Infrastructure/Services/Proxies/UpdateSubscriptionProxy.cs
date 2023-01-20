using Application.Interfaces.Proxies;

using CryptoExchange.Net.Sockets;

namespace Infrastructure.Services.Proxies;

public class UpdateSubscriptionProxy : IUpdateSubscriptionProxy
{
    internal UpdateSubscription Subscription = default!;
    public void SetSubscription(UpdateSubscription subscription) => this.Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription), $"The {nameof(UpdateSubscription)} cannot be NULL");

    public int SocketId => this.Subscription.SocketId;
    public int Id => this.Subscription.Id;

    public event Action ConnectionLost
    {
        add => this.Subscription.ConnectionLost += value;
        remove => this.Subscription.ConnectionLost -= value;
    }
    public event Action ConnectionClosed
    {
        add => this.Subscription.ConnectionClosed += value;
        remove => this.Subscription.ConnectionClosed -= value;
    }
    public event Action<TimeSpan> ConnectionRestored
    {
        add => this.Subscription.ConnectionRestored += value;
        remove => this.Subscription.ConnectionRestored -= value;
    }
    public event Action ActivityPaused
    {
        add => this.Subscription.ActivityPaused += value;
        remove => this.Subscription.ActivityPaused -= value;
    }
    public event Action ActivityUnpaused
    {
        add => this.Subscription.ActivityUnpaused += value;
        remove => this.Subscription.ActivityUnpaused -= value;
    }
    public event Action<Exception> Exception
    {
        add => this.Subscription.Exception += value;
        remove => this.Subscription.Exception -= value;
    }
    
    public Task CloseAsync() => this.Subscription.CloseAsync();
    public Task ReconnectAsync() => this.Subscription.ReconnectAsync();
}
