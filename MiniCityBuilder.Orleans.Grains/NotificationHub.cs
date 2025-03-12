using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MiniCityBuilder.Orleans.Contracts;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace MiniCityBuilder.Orleans.Grains;

public class NotificationHub : Hub
{
    private readonly IClusterClient _orleansClient;
    private readonly ILogger<NotificationHub> _logger;

    private static ConcurrentDictionary<string, string> ConnectedPlayers = new();

    public NotificationHub(IClusterClient orleansClient,
        ILogger<NotificationHub> logger)
    {
        _orleansClient = orleansClient;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var player = httpContext?.Request.Query["player"];

        if (!string.IsNullOrEmpty(player))
        {
            _logger.LogInformation("User connected {connectionId}", Context.ConnectionId);

            // TODO: Vérifier ici si le token est valide (JWT validation, base de données, etc.)

            ConnectedPlayers[Context.ConnectionId] = player.ToString();

            var connected = ConnectedPlayers.Values.Select(p => p.ToString()).Distinct().ToList();
            // Envoyer la liste des joueurs connectés au nouvel arrivant
            await Clients.All.SendAsync("ReceiveConnectedPlayers", connected);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectedPlayers.TryRemove(Context.ConnectionId, out var playerName))
        {
            // Notifier les autres joueurs de la déconnexion
            await Clients.All.SendAsync("PlayerDisconnected", playerName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message) => await Clients.Groups("players").SendAsync("PlayerJoined", message);

    public async Task AddPlayerToGroup()
    {
        // Ajouter la connexion à un groupe "players"
        await Groups.AddToGroupAsync(Context.ConnectionId, "players");
    }
}