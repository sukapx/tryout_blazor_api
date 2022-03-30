using Microsoft.AspNetCore.Identity;

namespace tryout_blazor_api.Server.Models;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}
