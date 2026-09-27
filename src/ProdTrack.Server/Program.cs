using ProdTrack.Application;
using ProdTrack.Infrastructure;
using ProdTrack.Server.Endpoints;
using ProdTrack.Server.Errors;
using ProdTrack.Server.Hosting;
using ProdTrack.Server.Logging;
using ProdTrack.Server.Realtime;
using ProdTrack.Server.Security;

var builder = WebApplication.CreateBuilder(args);

builder.AddProdTrackLogging();                                               // PT-004
builder.Services.AddProdTrackApplication();
builder.Services.AddProdTrackInfrastructure(builder.Configuration);          // EF Core, Identity stores, audit, sequences
builder.Services.AddProdTrackLocalProviders(builder.Configuration, builder.Environment.ContentRootPath); // Cloud:Provider=Local
var authMode = builder.AddProdTrackSecurity();                               // PT-009, PT-010
builder.Services.AddProdTrackErrorHandling();                                // PT-005
builder.Services.AddProdTrackWeb();                                          // Blazor, SignalR, OpenAPI, health checks

var app = builder.Build();

await app.InitializeDatabaseAsync(authMode);

app.UseProdTrackPipeline();
app.MapProdTrackHealthChecks();
app.MapProdTrackApi();
app.MapHub<ProductionHub>(ProdTrack.Contracts.Realtime.ProductionHubContract.Route);
app.MapAccountEndpoints(authMode);
app.MapProdTrackUi();

await app.RunAsync();

/// <summary>Entry point; public for WebApplicationFactory integration tests.</summary>
public partial class Program;
