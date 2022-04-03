using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace tryout_blazor_api.Client.Shared.Map;


public class Marker : IAsyncDisposable
{
    protected IJSObjectReference map;
    public IJSObjectReference icon;
    private IJSObjectReference? obj;
    private DotNetObjectReference<Marker> thisRef;
    private IJSRuntime JS;
    public event Action? OnClick;

    public Marker(IJSRuntime js, IJSObjectReference map, IJSObjectReference icon)
    {
        this.JS = js;
        this.map = map;
        this.icon = icon;
        thisRef = DotNetObjectReference.Create(this);
    }

    public async Task SetTo(float[] position)
    {
        if (obj is not null)
        {
            await obj.InvokeVoidAsync("removeFrom", map);
        }
        obj = await JS.InvokeAsync<IJSObjectReference>("L.marker", position, new { icon });
        obj = await obj.InvokeAsync<IJSObjectReference>("addTo", map);
        await JS.InvokeVoidAsync("HelperAddOnClick", new object [] { obj, thisRef, "Callback" });
    }

    public async Task BindPopup(string message)
    {
        IJSObjectReference popup = await obj.InvokeAsync<IJSObjectReference>("bindPopup", $"<div>{message}</div>");
    }


    [JSInvokable]
    public void Callback()
    {
        Console.WriteLine($"Marker Callback");
        OnClick?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        if (obj is not null)
        {
            await obj.InvokeVoidAsync("removeFrom", map);
        }
    }
}
