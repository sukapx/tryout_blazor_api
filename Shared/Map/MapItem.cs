namespace tryout_blazor_api.Shared.Map;

public class MapItem
{
    public Location Location { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }


    public Action? OnClick;
}
