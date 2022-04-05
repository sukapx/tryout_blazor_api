using tryout_blazor_api.Shared.Map;

namespace tryout_blazor_api.Shared.Play;


public class Player
{
    public string Name { get; set; }
    public Location Location { get; set; }

    public Market? DockedTo { get; set; }
    public CargoHold Cargo { get; set; } = new();

}

