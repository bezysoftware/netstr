using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Middleware;
using Netstr.Options;
using Netstr.RelayInformation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("NetstrDatabase");

// Setup Serilog logging
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services
    .AddCors(x => x.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()))
    .AddControllersWithViews().Services
    .AddHttpContextAccessor()
    .AddApplicationOptions<ConnectionOptions>("Connection")
    .AddApplicationOptions<RelayInformationOptions>("RelayInformation")
    .AddApplicationOptions<LimitsOptions>("Limits")
    .AddApplicationOptions<AuthOptions>("Auth")
    .AddMessaging()
    .AddHostedService<UserCacheStartupService>()
    .AddScoped<IRelayInformationService, RelayInformationService>()
    .AddDbContextFactory<NetstrDbContext>(x => x.UseNpgsql(connectionString));

var application = builder.Build();
var options = application.Services.GetRequiredService<IOptions<ConnectionOptions>>();

// Setup pipeline + init DB
application
    .UseCors()
    .UseWebSockets()
    .UseStaticFiles()
    .UseRouting()
    .AcceptWebSocketsConnections()
    .EnsureDbContextMigrations<NetstrDbContext>();

// Controllers maps
application.MapDefaultControllerRoute();

// Start the application
application.Run();

// Required for tests
public partial class Program { }