using Microsoft.AspNetCore.SignalR;
using MiniCityBuilder.Front;
using MiniCityBuilder.Orleans.Grains;
using MiniCityBuilder.Orleans.Grains.Helpers;
using SignalR.Orleans.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHostedService<PlayerEventConsumer>();
builder.Services.AddRegisterHelpers();

builder.Host.UseOrleansClient(static builder =>
{
    builder.UseLocalhostClustering();
    builder.AddMemoryStreams("game");
    builder.UseSignalR(configure: null);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.MapHub<NotificationHub>("/playerhub");

app.Run();