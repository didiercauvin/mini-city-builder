using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MiniCityBuilder.Orleans.Contracts;

namespace MiniCityBuilder.Orleans.Grains;

public class NotificationHub : Hub
{
    private readonly IClusterClient _orleansClient;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(IClusterClient orleansClient,
        ILogger<NotificationHub> logger)
    {
        _orleansClient = orleansClient;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("User connected {connectionId}", Context.ConnectionId);

        return Task.CompletedTask;
    }

    public async Task SendMessage(string message) => await Clients.Groups("players").SendAsync("PlayerJoined", message);

    public async Task GetUsers()
    {
        var userManager = _orleansClient.GetGrain<IUserManagerGrain>(0);
        var users = await userManager.GetUsers();
        await Clients.Group("players").SendAsync("PlayerJoined", (string.Join(",", users)));
    }

    public async Task AddPlayerToGroup()
    {
        // Ajouter la connexion à un groupe "players"
        await Groups.AddToGroupAsync(Context.ConnectionId, "players");
    }
}