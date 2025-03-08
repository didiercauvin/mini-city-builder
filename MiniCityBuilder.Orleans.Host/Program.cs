using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MiniCityBuilder.Orleans.Grains.Helpers;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddEnvironmentVariables();
    })
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.UseDashboard();

        siloBuilder.AddMemoryGrainStorage("playerStore");
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