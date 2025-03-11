namespace MiniCityBuilder.Orleans.Contracts;

public interface INotificationHub
{
    Task<List<string>> GetUsers();
}

public interface INotificationHubClient
{
    Task PlayerJoined(string message);
}