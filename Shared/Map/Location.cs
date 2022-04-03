namespace tryout_blazor_api.Shared.Map;

public class Location
{
  public float Latitude { get; set; } = 0;
  public float Longitude { get; set; } = 0;
  public float Altitude { get; set; } = 0;

  public float Distance(Location loc)
  {
    return Distance(Latitude, Longitude, loc.Latitude, loc.Longitude);
  }

  // https://www.movable-type.co.uk/scripts/latlong.html
  public float Distance(float lat1, float lon1, float lat2, float lon2)
  {
    var R = 6371000; // meters
    var dLat = (lat2-lat1) * MathF.PI / 180;
    var dLon = (lon2-lon1) * MathF.PI / 180;
    lat1 = (lat1) * MathF.PI / 180;
    lat2 = (lat2) * MathF.PI / 180;

    var a = MathF.Sin(dLat/2) * MathF.Sin(dLat/2)
      + MathF.Sin(dLon/2) * MathF.Sin(dLon/2)
        * MathF.Cos(lat1) * MathF.Cos(lat2); 
    var c = 2 * MathF.Atan2(MathF.Sqrt(a), MathF.Sqrt(1-a)); 
    var d = R * c;
    return d;
  }

  public float DirectionTo(Location loc) {
    return DirectionTo(Latitude, Longitude, loc.Latitude, loc.Longitude);
  }

  // https://www.movable-type.co.uk/scripts/latlong.html
  // return direction in degrees
  public float DirectionTo(float lat1, float lon1, float lat2, float lon2)
  {
    var dLon = (lon2-lon1) * MathF.PI / 180;
    lat1 = (lat1) * MathF.PI / 180;
    lat2 = (lat2) * MathF.PI / 180;

    var y = MathF.Sin(dLon) * MathF.Cos(lat2);
    var x = MathF.Cos(lat1)*MathF.Sin(lat2) -
              MathF.Sin(lat1)*MathF.Cos(lat2)*MathF.Cos(dLon);
    var θ = MathF.Atan2(y, x);
    return (θ*180/MathF.PI + 360) % 360;
  }

    public float[] ToArray() => new float[] { Latitude, Longitude };
}
