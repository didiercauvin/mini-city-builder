using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniCityBuilder.Orleans.Grains.Helpers;
using MiniCityBuilder.Orleans.Host;
using Orleans.Configuration;
using Orleans.Hosting;

var siloPort = int.Parse(Environment.GetEnvironmentVariable("ORLEANS_SILO_PORT") ?? "11111");
var gatewayPort = int.Parse(Environment.GetEnvironmentVariable("ORLEANS_GATEWAY_PORT") ?? "30001");

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
        });

        config.ConfigureServices(services =>
        {
            services.AddRouting();
            services.AddHealthChecks().AddCheck<SiloHealthcheck>("silo");
        });
    })
    .UseOrleans((context, siloBuilder) =>
    {
        var connectionString = context.Configuration.GetConnectionString("OrleansCluster") ?? "Server=localhost;Database=OrleansCluster;Integrated Security=true;TrustServerCertificate=True";

        //siloBuilder.UseAzureStorageClustering(options => options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true"));

        //siloBuilder.UseLocalhostClustering(clusterId: "default", serviceId: "default");
        siloBuilder.UseAdoNetClustering(options =>
        {
            options.Invariant = "System.Data.SqlClient"; // Pour SQL Server
            options.ConnectionString = connectionString;
        });

        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "minicitybuilder-orleans";
            options.ServiceId = "minicitybuilder-orleans";
        });

        siloBuilder.Configure<EndpointOptions>(options =>
        {
            options.SiloPort = siloPort;
            options.GatewayPort = gatewayPort;
        });

        siloBuilder.UseDashboard(options =>
        {
            //options.Port = int.Parse(Environment.GetEnvironmentVariable("DASHBOARD_PORT") ?? "0");
            options.HostSelf = true;
            options.CounterUpdateIntervalMs = 5000;
        });

        siloBuilder
            .AddMemoryGrainStorage("playerStore")
            .AddMemoryGrainStorage("PubSubStore");
            //.UseSignalR();
        
    })
    .Build();

// Start the host
await host.StartAsync();


Console.WriteLine("Orleans is running...\n\nPress enter to stop silo");

Console.ReadLine();

await host.StopAsync();