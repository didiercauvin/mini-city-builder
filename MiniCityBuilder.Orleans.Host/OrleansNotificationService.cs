using Microsoft.AspNetCore.SignalR;
using MiniCityBuilder.Orleans.Grains;
using Orleans.Streams;

namespace MiniCityBuilder.Orleans.Host;

public class OrleansNotificationService
{
    private readonly IClusterClient _orleansClient;
    private readonly IHubContext<NotificationHub> _hubContext;

    public OrleansNotificationService(IClusterClient orleansClient, IHubContext<NotificationHub> hubContext)
    {
        _orleansClient = orleansClient;
        _hubContext = hubContext;
    }

    public async Task StartListening()
    {
        var streamProvider = _orleansClient.GetStreamProvider("game");
        var stream = streamProvider.GetStream<string>("game");

        await stream.SubscribeAsync(async (message, _) =>
        {
            // Envoyer l'événement à tous les clients SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
        });
    }
}