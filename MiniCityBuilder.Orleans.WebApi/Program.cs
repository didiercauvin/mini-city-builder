using static Microsoft.AspNetCore.Http.TypedResults;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient(static builder =>
{
    builder.UseLocalhostClustering();
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();