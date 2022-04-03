using tryout_blazor_api.Shared.Map;

namespace tryout_blazor_api.Shared.Play;

public enum MarketItemType
{
    Iron,
    Wheat,
    Water,
    Wood
}

public class MarketItem
{
    public MarketItemType Type { get; set; }
    public ulong Amount { get; set; }
}

public class Market
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Location Location { get; set; }

    public IEnumerable<MarketItem> Commodity { get; set; }
}
