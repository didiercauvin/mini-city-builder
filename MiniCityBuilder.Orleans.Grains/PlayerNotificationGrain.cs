using Microsoft.AspNetCore.SignalR;
using MiniCityBuilder.Orleans.Contracts;
using Orleans.Streams;

namespace MiniCityBuilder.Orleans.Grains;

public class ReceiverGrain: Grain, IPlayerNotificationGrain
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly List<string> _recentConnections = new();

    public ReceiverGrain(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
//     public override async Task OnActivateAsync(CancellationToken cancellationToken)
//     {
//         // Create a GUID based on our GUID as a grain
//         var guid = this.GetPrimaryKey();
//
// // Get one of the providers which we defined in config
//         var streamProvider = this.GetStreamProvider("game");
//
// // Get the reference to a stream
//         var streamId = StreamId.Create("RANDOMDATA", guid);
//         var stream = streamProvider.GetStream<PlayerDto>(streamId);
//
// // Set our OnNext method to the lambda which simply prints the data.
// // This doesn't make new subscriptions, because we are using implicit
// // subscriptions via [ImplicitStreamSubscription].
//         await stream.SubscribeAsync(async (player, token) =>
//         {
//             await _hubContext.Clients.All.SendAsync("PlayerJoined", player.UserName);
//         });
//         
//         // await stream.SubscribeAsync<int>(
//         //     async (data, token) =>
//         //     {
//         //         Console.WriteLine(data);
//         //         await Task.CompletedTask;
//         //     });
//     }

    public async Task NotifyPlayerConnected(string playerName)
    {
        _recentConnections.Add(playerName);
        if (_recentConnections.Count > 100) _recentConnections.RemoveAt(0);
        
        // Si vous pouvez injecter le HubContext dans le grain
        await _hubContext.Clients.All.SendAsync("PlayerJoined", playerName);
    }
    
    public Task<List<string>> GetRecentConnections(int count)
    {
        return Task.FromResult(_recentConnections.TakeLast(count).ToList());
    }
}