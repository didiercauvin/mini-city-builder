using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCityBuilder.Orleans.Contracts;

public interface IUserLoginObserver : IGrainObserver
{
    Task ReceiveMessage(string message);
}
