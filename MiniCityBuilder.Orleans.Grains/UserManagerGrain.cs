using Microsoft.Extensions.Logging;
using MiniCityBuilder.Orleans.Contracts;
using Orleans.Utilities;
using System;

namespace MiniCityBuilder.Orleans.Grains;

public class UserManagerGrain : Grain, IUserManagerGrain
{
    private readonly ObserverManager<IUserLoginObserver> _subsManager;
    private List<string> _users = new();

    public UserManagerGrain(ILogger<UserManagerGrain> logger)
    {
        _subsManager =
        new ObserverManager<IUserLoginObserver>(
                TimeSpan.FromMinutes(5), logger);
    }

    public Task Subscribe(IUserLoginObserver observer)
    {
        _subsManager.Subscribe(observer, observer);

        return Task.CompletedTask;
    }

    public Task Unsubscribe(IUserLoginObserver observer)
    {
        _subsManager.Unsubscribe(observer);

        return Task.CompletedTask;
    }

    public Task SendUpdateMessage(string message)
    {
        _subsManager.Notify(s => s.ReceiveMessage(message));

        return Task.CompletedTask;
    }

    public Task<List<string>> GetUsers()
    {
        return Task.FromResult(_users);
    }

    public Task RegisterUser(string login)
    {
        _users.Add(login);
        return Task.CompletedTask;
    }
}