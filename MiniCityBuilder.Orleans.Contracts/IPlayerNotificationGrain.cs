namespace MiniCityBuilder.Orleans.Contracts;

public interface IPlayerNotificationGrain : IGrainWithStringKey
{
    Task NotifyPlayerConnected(string playerName);
    
    Task<List<string>> GetRecentConnections(int count);
}