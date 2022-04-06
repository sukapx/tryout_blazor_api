using Microsoft.AspNetCore.Identity;
using tryout_blazor_api.Server;
using tryout_blazor_api.Server.Models;
using tryout_blazor_api.Shared.Play;

namespace tryout_blazor_api.Server.Data;

public class DBSeed
{
    private UserManager<ApplicationUser>? userManager;
    private RoleManager<IdentityRole>? roleManager;

    public async Task Seed(IServiceScope scope)
    {
        // https://docs.microsoft.com/de-de/aspnet/core/security/authorization/secure-data?view=aspnetcore-6.0
        userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>()!;
        roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>()!;

        var userAdmin = await userManager.FindByEmailAsync("Admin")!;
        var userUser = await userManager.FindByEmailAsync("User")!;
/*
        await EnsureRole(new IdentityRole { Name = "Admin", NormalizedName = "Administrator" });
        await EnsureRole(new IdentityRole { Name = "User", NormalizedName = "Registered" });

        await EnsureUserRole(userAdmin, "Admin");
        await EnsureUserRole(userAdmin, "User");
        await EnsureUserRole(userUser, "User");
*/
        var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
        if (db is not null)
        {
            if (db.Markets.FirstOrDefault(m => m.Id == 1) is null)
            {
                await db.Markets.AddRangeAsync(new List<Market>{
                    new() {
                        Id = 1,
                        Name = "Spektesee",
                        Location = new() { Latitude = 52.5447F,  Longitude = 13.1675F },
                        Cargo = new() {
                            Items = new() {
                                { MarketItemType.Iron, 100 },
                                { MarketItemType.Wheat, 60 },
                                { MarketItemType.Wood, 10 }
                            }
                        }
                    },
                    new () {
                        Id = 2,
                        Name = "Altstadt",
                        Location = new() { Latitude = 52.5374F, Longitude = 13.2037F },
                        Cargo = new() {
                            Items = new() {
                                { MarketItemType.Iron, 100 },
                                { MarketItemType.Wheat, 60 },
                                { MarketItemType.Wood, 10 }
                            }
                        }
                    }
                });
            }
            db.SaveChanges();
        }
    }

    public async Task<ApplicationUser> EnsureUser(ApplicationUser user, string pass)
    {
        if (null == await userManager!.FindByEmailAsync(user.Email))
        {
            await userManager.CreateAsync(user, pass);
            return user;
        }
        else
        {
            return await userManager!.FindByEmailAsync(user.Email);
        }
    }

    public async Task EnsureRole(IdentityRole role)
    {
        if (!await roleManager!.RoleExistsAsync(role.Name))
        {
            await roleManager.CreateAsync(role);
        }
    }

    public async Task EnsureUserRole(ApplicationUser user, string role)
    {
        if (!await userManager!.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}

