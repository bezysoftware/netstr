using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Options;
using Netstr.RelayInformation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("NetstrDatabase");

// Setup Serilog logging
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

builder.Services
    .AddCors(x => x.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()))
    .AddHttpContextAccessor()
    .AddApplicationOptions<ConnectionOptions>("Connection")
    .AddApplicationOptions<RelayInformationOptions>("RelayInformation")
    .AddApplicationOptions<LimitsOptions>("Limits")
    .AddApplicationOptions<AuthOptions>("Auth")
    .AddMessaging()
    .AddScoped<IRelayInformationService, RelayInformationService>()
    .AddDbContextFactory<NetstrDbContext>(x => x.UseNpgsql(connectionString));

var application = builder.Build();
var options = application.Services.GetRequiredService<IOptions<ConnectionOptions>>();

application.Logger.LogInformation($"DB connection string: {connectionString}");

// Setup pipeline + init DB
application
    .UseCors()
    .UseWebSockets()
    .AcceptWebSocketsConnections()
    .EnsureDbContextMigrations<NetstrDbContext>();

// Relay Information Document
application.MapGet(
    options.Value.WebSocketsPath, 
    (HttpRequest request, [FromServices] IRelayInformationService service) =>
    {
        if (request.Headers["Accept"] == "application/nostr+json")
        {
            return Results.Ok(service.GetDocument());
        } 
        else
        {
            return Results.Text("Welcome to Netstr");
        }
    });

// Start the application
application.Run();

// Required for tests
public partial class Program { }