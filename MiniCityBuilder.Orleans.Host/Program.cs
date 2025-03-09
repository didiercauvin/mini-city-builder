using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniCityBuilder.Orleans.Grains;
using MiniCityBuilder.Orleans.Grains.Helpers;
using MiniCityBuilder.Orleans.Host;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices(services =>
        {
            services.AddSignalR();
        });

        webBuilder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/playerhub");
            });
        });
    })
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.UseDashboard();

        siloBuilder
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryGrainStorage("playerStore")
            .AddMemoryStreams("game");
        
        siloBuilder.RegisterHub<NotificationHub>();
    })
    .ConfigureServices(sp =>
    {
        sp.AddRegisterHelpers();
        sp.AddSingleton<OrleansNotificationService>();
    })
    .Build();

// Start the host
await host.StartAsync();


Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();