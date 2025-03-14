using k8s.KubeConfigModels;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

const string clusterId = "default"; // Ou utilisez l'ID que vous préférez

var orleans = builder.AddOrleans("default")
    .WithDevelopmentClustering()
    .WithMemoryGrainStorage("playerStore")
    .WithMemoryGrainStorage("PubSubStore");

var orleansBuilder = builder.AddProject<MiniCityBuilder_Orleans_Host>("silo")
    .WithReference(orleans)
    .WithReplicas(3)
    .WithEnvironment("ASPNETCORE_URLS", "http://0.0.0.0:0")
    .WithEnvironment("ORLEANS_CLUSTER_ID", clusterId);


builder.AddProject<Projects.MiniCityBuilder_Front>("minicitybuilder-front")
       .WithReference(orleans.AsClient())
       .WithExternalHttpEndpoints()
       .WaitFor(orleansBuilder);

builder.Build().Run();
