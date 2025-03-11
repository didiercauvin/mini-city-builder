using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniCityBuilder.Orleans.Grains.Helpers;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.UseDashboard();
        siloBuilder
            .AddMemoryGrainStorage("playerStore")
            .AddMemoryGrainStorage("PubSubStore")
            .UseSignalR();
        
    })
    .ConfigureServices(sp =>
    {
        sp.AddRegisterHelpers();
    })
    .Build();


// Start the host
await host.StartAsync();


Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();