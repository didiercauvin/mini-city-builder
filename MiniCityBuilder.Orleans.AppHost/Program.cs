using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var connectionString = builder.AddConnectionString("clusterdb").Resource;

var db = builder
            .AddSqlServer("orleans-db")
            .WithConnectionStringRedirection(connectionString);

var orleans = builder.AddOrleans("default")
    .WithDevelopmentClustering()
    .WithMemoryGrainStorage("playerStore")
    .WithMemoryGrainStorage("PubSubStore");

var orleansBuilder = builder.AddProject<MiniCityBuilder_Orleans_Host>("silo")
    .WithReference(orleans)
    .WithReplicas(3)
    .WithEnvironment("ASPNETCORE_URLS", "http://0.0.0.0:0")
    .WithReference(db)
    .WaitFor(db);


builder.AddProject<Projects.MiniCityBuilder_Front>("minicitybuilder-front")
       .WithReference(orleans.AsClient())
       .WithExternalHttpEndpoints()
       .WaitFor(orleansBuilder);

builder.Build().Run();
