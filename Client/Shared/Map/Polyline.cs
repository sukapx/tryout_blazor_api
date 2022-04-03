using Microsoft.JSInterop;

namespace tryout_blazor_api.Client.Shared.Map;

public class Polyline : IAsyncDisposable
{
    protected IJSObjectReference map;
    private IJSObjectReference? obj;
    private IJSRuntime JS;

    public Polyline(IJSRuntime js, IJSObjectReference map)
    {
        this.JS = js;
        this.map = map;
    }

    public async Task Route(IEnumerable<double[]> position)
    {
        if (obj is not null)
        {
            await obj.InvokeVoidAsync("removeFrom", map);
        }
        obj = await JS.InvokeAsync<IJSObjectReference>("L.polyline", position);
        obj = await obj.InvokeAsync<IJSObjectReference>("addTo", map);
    }

    public async ValueTask DisposeAsync()
    {
        if (obj is not null)
        {
            await obj.InvokeVoidAsync("removeFrom", map);
        }
    }
}
