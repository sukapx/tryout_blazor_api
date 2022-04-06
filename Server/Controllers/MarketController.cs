using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using tryout_blazor_api.Shared;
using tryout_blazor_api.Shared.Map;
using tryout_blazor_api.Shared.Play;

namespace tryout_blazor_api.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MarketController : ControllerBase
{
    // Distance to be close in meters
    static private readonly float DISTANCE_CLOSE = 20;

    private readonly ILogger<MarketController> _logger;
    private readonly ApplicationDbContext _db;

    public MarketController(
        ILogger<MarketController> logger,
        ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public IEnumerable<Market> Get()
    {
        var markets = _db.Markets.ToList();
        markets.ForEach(market =>
            _db.Entry(market).Reference(m => m.Location).Load());
        return markets;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<Market> Get(ulong id)
    {
        return await GetMarket(id);
    }

    [HttpPost]
    [Route("check/{id}")]
    public async Task<IActionResult> Check(ulong id, Location location)
    {
        var targetedMarket = await GetMarket(id);
        var distance = location.Distance(targetedMarket.Location!);

        _logger.LogInformation("Checking {location} for Market {id}, distance: {distance}", 
                                    JsonSerializer.Serialize(location), id, distance);

        if(distance < DISTANCE_CLOSE)
        {
            return Ok(new{ Result="near" });
        }
        return Ok(new{ Result="to far" });
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<Market> GetMarket(ulong id)
    {
        var market = _db.Markets.First(m => m.Id == id);
        if (market is null)
            throw new Exception("Non existing Market");

        await _db.Entry(market).Reference(m => m.Location).LoadAsync();

        return market;
    }
}
