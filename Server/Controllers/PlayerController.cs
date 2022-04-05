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
    private readonly MarketController _markets;

    private static Dictionary<string, Player> Players = new()
    {
        {
            "9c0464d6-a43a-4b06-90d8-0c2294779595",
            new()
            {
                Name = "User1",
                Cargo = new()
                {
                    Items = new()
                    {
                        { MarketItemType.Iron, 10 }
                    }
                }
            }
        }
    };

    public PlayerController(ILogger<PlayerController> logger,
        MarketController markets)
    {
        _logger = logger;
        _markets = markets;
    }

    [HttpGet]
    public Player? Get()
    {
        return GetPlayer();
    }

    [HttpPost]
    [Route("SetLocation")]
    public IActionResult SetLocation(Location location)
    {
        GetPlayer().Location = location;
        return Ok();
    }

    /// <summary>
    /// Transfers Items between Player and Market.
    /// </summary>
    /// <param name="amount">If positive, items will be transfered to Player</param>
    /// <returns></returns>
    [HttpGet]
    [Route("transfer/{marketId}/{type}/{amount}")]
    public IActionResult Transfer(int marketId, MarketItemType type, int amount)
    {
        var market = _markets.GetMarket(marketId);
        var player = GetPlayer();

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


    protected Player GetPlayer()
    {
        _logger.LogInformation($"Get Player");

        var playerID = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "ID")?.Value;
        _logger.LogInformation($"Id: {playerID}");

        Player player = null;
        if (!Players.ContainsKey(playerID))
        {
            player = new Player()
            {
                Name = HttpContext.User.Identity!.Name!
            };
            Players.Add(playerID, player);
        }

        return Players[playerID];
    }
}
