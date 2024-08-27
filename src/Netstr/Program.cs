using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Options;
using Netstr.RelayInformation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCors(x => x.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()))
    .AddHttpContextAccessor()
    .AddApplicationOptions<ConnectionOptions>("Connection")
    .AddApplicationOptions<RelayInformationOptions>("RelayInformation")
    .AddApplicationOptions<LimitsOptions>("Limits")
    .AddApplicationOptions<AuthOptions>("Auth")
    .AddMessaging()
    .AddScoped<IRelayInformationService, RelayInformationService>()
    .AddDbContextFactory<NetstrDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("NetsrtDatabase")));

var application = builder.Build();
var options = application.Services.GetRequiredService<IOptions<ConnectionOptions>>();

// Setup pipeline + init DB
application
    .UseCors()
    .UseWebSockets()
    .AcceptWebSocketsConnections()
    .EnsureDbContextMigrations<NetstrDbContext>();

// Relay Information Document
application.UseRouting();
application.MapWhen(
    ctx => ctx.Request.Headers["Accept"] == "application/nostr+json", 
    app => app.UseEndpoints(x => x.MapGet(options.Value.WebSocketsPath, ([FromServices] IRelayInformationService service) => service.GetDocument())));

// Start the application
application.Run();

// Required for tests
public partial class Program { }