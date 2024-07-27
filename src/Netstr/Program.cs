using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplicationOptions<ConnectionOptions>("Connection")
    .AddApplicationOptions<LimitsOptions>("Limits")
    .AddMessaging()
    .AddDbContextFactory<NetstrDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("NetsrtDatabase")));

var application = builder.Build();

application
    .UseWebSockets()
    .AcceptWebSocketsConnections()
    .EnsureDbContextMigrations<NetstrDbContext>();

application.Run();