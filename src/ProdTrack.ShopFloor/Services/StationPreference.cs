using Microsoft.JSInterop;

namespace ProdTrack.ShopFloor.Services;

/// <summary>Remembers the tablet's station in localStorage so a device stays bound to its workstation.</summary>
public sealed class StationPreference(IJSRuntime js)
{
    private const string Key = "prodtrack.station";

    public ValueTask<string?> GetAsync() => js.InvokeAsync<string?>("localStorage.getItem", Key);

    public ValueTask SetAsync(string stationCode) => js.InvokeVoidAsync("localStorage.setItem", Key, stationCode);
}
