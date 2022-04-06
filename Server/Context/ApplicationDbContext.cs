using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Xml;
using tryout_blazor_api.Server.Models;
using tryout_blazor_api.Shared.Map;
using tryout_blazor_api.Shared.Play;

namespace tryout_blazor_api.Server
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Market> Markets { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CargoHold>(b =>
            {
                b.Property(c => c.Items)
                    .HasConversion(
                        d => JsonSerializer.Serialize(d, new JsonSerializerOptions { }),
                        s => JsonSerializer.Deserialize<Dictionary<MarketItemType, uint>>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }))
                    .HasMaxLength(4000)
                    .IsRequired();
            });

            Seed(modelBuilder);
        }

        protected void Seed(ModelBuilder modelBuilder)
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "Admin",
                Email = "admin@example.net",
                EmailConfirmed = true,
                SecurityStamp = string.Empty
            };
            admin.NormalizedUserName = admin.UserName.ToUpper();
            admin.NormalizedEmail = admin.Email.ToUpper();
            admin.PasswordHash = hasher.HashPassword(admin, "P4$$w0rd");
            modelBuilder.Entity<ApplicationUser>().HasData(admin);


            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "User",
                Email = "user@example.net",
                EmailConfirmed = true,
                SecurityStamp = string.Empty
            };
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();
            user.PasswordHash = hasher.HashPassword(user, "P4$$w0rd");
            modelBuilder.Entity<ApplicationUser>().HasData(user);
        }
    }
}
