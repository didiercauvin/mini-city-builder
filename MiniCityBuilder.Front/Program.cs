using MiniCityBuilder.Orleans.Contracts;
using MiniCityBuilder.Orleans.Grains;
using MiniCityBuilder.Orleans.Grains.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    // Adds an IClusterClient to the service provider.
    .AddOrleansClient(clientBuilder =>
    {
        // Tell the client how to connect to Orleans (you'll need to customize this for yourself)
        clientBuilder.UseLocalhostClustering();
        // Tells the client how to connect to the SignalR.Orleans backplane.
        clientBuilder.UseSignalR(config: null);
    })
    .AddSignalR()
    .AddOrleans();  // Adds SignalR hubs to the web application

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRegisterHelpers();

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

app.MapHub<NotificationHub>("/notifications");

//var grainFactory = app.Services.GetRequiredService<IGrainFactory>();
//var friend = grainFactory.GetGrain<IUserManagerGrain>(0);

//var manager = new UserLoginObserver();
//var obj = grainFactory.CreateObjectReference<IUserLoginObserver>(manager);

//await friend.Subscribe(obj);

app.Run();