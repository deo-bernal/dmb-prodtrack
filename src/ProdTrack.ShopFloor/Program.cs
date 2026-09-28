using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProdTrack.ApiClient;
using ProdTrack.ShopFloor;
using ProdTrack.ShopFloor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Same origin as the Server: the Identity cookie is sent automatically (BFF style, no tokens in the browser).
var origin = new Uri(new Uri(builder.HostEnvironment.BaseAddress).GetLeftPart(UriPartial.Authority) + "/");
builder.Services.AddScoped(_ => new ProdTrackApiClient(new HttpClient { BaseAddress = origin }));
builder.Services.AddScoped<StationPreference>();
builder.Services.AddScoped<LiveUpdates>();

await builder.Build().RunAsync();
