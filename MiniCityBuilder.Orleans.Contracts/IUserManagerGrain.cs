namespace MiniCityBuilder.Orleans.Contracts;

public interface IUserManagerGrain : IGrainWithIntegerKey
{
    // Gestion des observateurs
    Task Subscribe(IUserLoginObserver observer);
    Task Unsubscribe(IUserLoginObserver observer);
    Task SendUpdateMessage(string message);
    Task RegisterUser(string login);
    Task<List<string>> GetUsers();
}