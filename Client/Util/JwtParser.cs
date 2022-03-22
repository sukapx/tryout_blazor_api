using System.Security.Claims;
using System.Text.Json;

namespace tryout_blazor_api.Client.Util;

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        Console.WriteLine("Parsing claims from JWT");
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        
        var jsonBytes = ParseBase64WithoutPadding(payload);
        
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
        foreach(var kvp in keyValuePairs)
        {
            Console.WriteLine($"{kvp.Key}: ({kvp.Value.GetType()})");
          if(kvp.Value.ToString().Contains("["))
          {
            foreach(var role in kvp.Value.EnumerateArray()) {
              Console.WriteLine($"{kvp.Key}: {role}");
              claims.Add(new Claim(kvp.Key, role.ToString()));
            }
          }else{
            claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
          }
        }
        return claims;
    }
    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}