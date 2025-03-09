using Microsoft.AspNetCore.SignalR;
using MiniCityBuilder.Orleans.Contracts;
using MiniCityBuilder.Orleans.Grains;
using Orleans.Streams;

namespace MiniCityBuilder.Front;

public class PlayerEventConsumer: BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IClusterClient _orleansClient;

    public PlayerEventConsumer(IHubContext<NotificationHub>  hubContext, IClusterClient orleansClient)
    {
        _hubContext = hubContext;
        _orleansClient = orleansClient;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // var streamProvider = _orleansClient.GetStreamProvider("game");
        // var stream = streamProvider.GetStream<PlayerDto>("players", new Guid("c3b9c03e-6d4c-4d61-a376-4f4c26d3bd76"));
        //
        // await stream.SubscribeAsync(async (player, token) =>
        // {
        //     await _hubContext.Clients.All.SendAsync("PlayerJoined", player.UserName);
        // });
        
        
        var notificationGrain = _orleansClient.GetGrain<IPlayerNotificationGrain>("notifications");
        var lastProcessedCount = 0;
    
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Récupérer les connexions récentes
                var recentConnections = await notificationGrain.GetRecentConnections(100);
            
                // Traiter uniquement les nouvelles connections
                if (recentConnections.Count > lastProcessedCount)
                {
                    var newConnections = recentConnections.Skip(lastProcessedCount).ToList();
                
                    foreach (var playerName in newConnections)
                    {
                        await _hubContext.Clients.All.SendAsync("PlayerConnected", playerName);
                    }
                
                    lastProcessedCount = recentConnections.Count;
                }
            }
            catch (Exception ex)
            {
                
            }
        
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}