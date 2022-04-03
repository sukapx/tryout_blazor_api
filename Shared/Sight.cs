using tryout_blazor_api.Shared.Map;

namespace tryout_blazor_api.Shared;

public class Sight
{
  public string? Name { get; set; }
  public Location? Location { get; set; }
  public ICollection<SightFact>? Facts { get; set; }
}
