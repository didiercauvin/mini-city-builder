using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniCityBuilder.Orleans.Contracts;
using MiniCityBuilder.Orleans.Grains;
using MiniCityBuilder.Orleans.Grains.Helpers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

//builder.AddServiceDefaults();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("3dcaeba6e31ca7a01b99c26dc7a036221a82c71dc6f6576d1557ff3abff1a096")),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        x.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Vérifier si un cookie nommé "jwt" existe
                context.Token = context.Request.Cookies["jwt"];
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<JwtMiddleware>();

app.UseHttpsRedirection();

app.UseSession();
app.UseRouting();
app.UseCookiePolicy();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.MapHub<NotificationHub>("/notifications");

app.Run();