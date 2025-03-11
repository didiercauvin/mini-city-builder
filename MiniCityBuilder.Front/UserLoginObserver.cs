using MiniCityBuilder.Orleans.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCityBuilder.Front;

public class UserLoginObserver : IUserLoginObserver
{
    public Task ReceiveMessage(string message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}
