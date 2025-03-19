using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var connectionString = builder.AddConnectionString("OrleansCluster").Resource;

var sql = builder.AddSqlServer("orleans-db");

//var db = sql.AddDatabase("OrleansCluster");
var db = sql.WithConnectionStringRedirection(connectionString);

var orleans = builder.AddOrleans("default");

orleans
    .WithClustering(db)
    .WithClusterId("minicitybuilder-orleans")
    .WithServiceId("minicitybuilder-orleans")
    .WithMemoryGrainStorage("playerStore")
    .WithMemoryGrainStorage("PubSubStore");

var orleansBuilder = builder.AddProject<MiniCityBuilder_Orleans_Host>("silo")
    .WithReference(orleans)
    .WithReference(db)
    .WithReplicas(1)
    .WithEnvironment("ASPNETCORE_URLS", "http://0.0.0.0:0")
    .WithEnvironment("ORLEANS_CLUSTERING_PROVIDER", "bbbb")
    //.WithEnvironment("ORLEANS_AZURE_STORAGE", "false")  // Désactiver Azure Storage si Aspire tente de l'utiliser
    .WithEnvironment("Orleans__Clustering__ProviderType", "AdoNet") // Forcer le provider
    .WaitFor(db);


//builder.AddProject<Projects.MiniCityBuilder_Front>("minicitybuilder-front")
//       .WithReference(orleans.AsClient())
//       .WithExternalHttpEndpoints()
//       .WithReference(db)
//       .WaitFor(db)
//       .WaitFor(orleansBuilder);

builder.Build().Run();
