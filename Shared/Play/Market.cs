using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tryout_blazor_api.Shared.Map;

namespace tryout_blazor_api.Shared.Play;

public enum MarketItemType
{
    Iron,
    Wheat,
    Water,
    Wood
}

public class Market
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    [Required]
    public string Name { get; set; }
    public string Description { get; set; } = "";

    [Required]
    public Location Location { get; set; }

    [Required]
    public CargoHold Cargo { get; set; } = new() { Cargospace = 1000 };
}
