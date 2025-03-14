﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MiniCityBuilder.Orleans.Grains.Helpers;
using MiniCityBuilder.Orleans.Host;
using Orleans.Configuration;
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
            app.UseOrleansDashboard();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksWithJsonResponse("/health");
            });

            //app.Map("/dashboard", x => x.UseOrleansDashboard());
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
        siloBuilder.UseLocalhostClustering(clusterId: "default", serviceId: "default");
        //siloBuilder.UseAdoNetClustering(options =>
        //{
        //    options.Invariant = "System.Data.SqlClient"; // Pour SQL Server
        //    options.ConnectionString = "Server=sqlserver;Database=OrleansCluster;Integrated Security=true;TrustServerCertificate=True";
        //});

        siloBuilder.UseDashboard(options =>
        {
            //options.Port = int.Parse(Environment.GetEnvironmentVariable("DASHBOARD_PORT") ?? "0");
            options.HostSelf = true;
            options.CounterUpdateIntervalMs = 5000;
        });

        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "default";
            options.ServiceId = "default";
        });

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