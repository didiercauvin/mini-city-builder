using Microsoft.Extensions.Hosting;

using var host = Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.UseDashboard();

        // siloBuilder.AddMemoryGrainStorage("roomStore");
        // siloBuilder.AddMemoryGrainStorage("playerStore");
    })
    .Build();


// Start the host
await host.StartAsync();

Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();