using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Map;
using tryout_blazor_api.Shared.Play;

namespace tryout_blazor_api.Server.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class PlayerController : ControllerBase
{
    // Distance to be close in meters
    static private readonly float DISTANCE_CLOSE = 20;

    private readonly ILogger<PlayerController> _logger;
    private readonly ApplicationDbContext _db;

    public PlayerController(
        ILogger<PlayerController> logger,
        ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public async Task<Player?> Get()
    {
        return await GetPlayer();
    }

    [HttpPost]
    [Route("SetLocation")]
    public async Task<IActionResult> SetLocation(Location location)
    {
        var player = await GetPlayer();
        player.Location = location;
        await _db.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Transfers Items between Player and Market.
    /// </summary>
    /// <param name="amount">If positive, items will be transfered to Player</param>
    /// <returns></returns>
    [HttpGet]
    [Route("transfer/{marketId}/{type}/{amount}")]
    public async Task<IActionResult> Transfer(ulong marketId, MarketItemType type, int amount)
    {
        var market = _db.Markets.First(m => m.Id == marketId);
        var player = await GetPlayer();

        if (player.Location.DirectionTo(market.Location) > DISTANCE_CLOSE)
        {
            return Problem("To far away");
        }

        if (!player.Cargo.CanChangeCargo(type, amount))
            return Problem("Problem on players Cargo");

        if (!market.Cargo.CanChangeCargo(type, -amount))
            return Problem("Problem on Market");

        player.Cargo.ChangeCargo(type, amount);
        market.Cargo.ChangeCargo(type, -amount);

        return Ok();
    }


    protected async Task<Player> GetPlayer()
    {
        _logger.LogInformation($"Get Player");

        var playerID = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "ID")?.Value;
        _logger.LogInformation($"Id: {playerID}");

        Player? player = _db.Players.FirstOrDefault(p => p.Owner == playerID);
        if (player is null)
        {
            player = new Player()
            {
                Name = HttpContext.User.Identity!.Name!,
                Owner = playerID
            };
            await _db.AddAsync(player.Cargo);
            await _db.Players.AddAsync(player);
            await _db.SaveChangesAsync();
        }

        return player;
    }
}
