using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using MiniCityBuilder.Orleans.Grains.Helpers;
using MiniCityBuilder.Orleans.Host;
using Orleans.Runtime;
using System.Reflection.PortableExecutable;
using static Microsoft.AspNetCore.Http.TypedResults;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .ConfigureServices(sp =>
    {
        sp.AddRegisterHelpers();
    })
    .ConfigureWebHostDefaults(config =>
    {
        config.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksWithJsonResponse("/health");
            });
        });

        // N'oubliez pas d'ajouter ceci pour configurer les services web
        config.ConfigureServices(services =>
        {
            services.AddRouting();
            services.AddHealthChecks().AddCheck<SiloHealthcheck>("silo");
        });
    })
    .UseOrleans(siloBuilder =>
    {
        var clusterId = Environment.GetEnvironmentVariable("ORLEANS_CLUSTER_ID") ?? "default";

        siloBuilder.UseLocalhostClustering(clusterId: clusterId, serviceId: clusterId);
        siloBuilder.UseDashboard();
        siloBuilder
            .AddMemoryGrainStorage("playerStore")
            .AddMemoryGrainStorage("PubSubStore")
            .UseSignalR();
        
    })
    .Build();

// Start the host
await host.StartAsync();


Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();