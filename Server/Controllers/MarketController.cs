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

    protected static List<Market> Markets = new()
    {
        new()
        {
            Name = "Spektesee",
            Location = new()
            {
                Latitude = 52.5447F,
                Longitude = 13.1675F
            },
            Cargo = new()
            {
                Items = new ()
                {
                    { MarketItemType.Iron, 100 },
                    { MarketItemType.Wheat, 60 },
                    { MarketItemType.Wood, 10 }
                }
            }
        },
        new()
        {
            Name = "Altstadt",
            Location = new()
            {
                Latitude = 52.5374F,
                Longitude = 13.2037F
            },
            Cargo = new()
            {
                Items = new()
                {
                    { MarketItemType.Iron, 100 },
                    { MarketItemType.Wheat, 60 },
                    { MarketItemType.Wood, 10 }
                }
            }
        }
    };


    private readonly ILogger<MarketController> _logger;

    public MarketController(ILogger<MarketController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<Market> Get()
    {
        return Markets;
    }

    [HttpGet]
    [Route("{id}")]
    public Market Get(int id)
    {
        return GetMarket(id);
    }

    [HttpPost]
    [Route("check/{id}")]
    public IActionResult Check(int id, Location location)
    {
        var targetedMarket = GetMarket(id);
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
    public Market GetMarket(int id)
    {
        if (id < 0 || id >= Markets.Count)
            throw new Exception("Non existing Sight");

        return Markets[id];
    }
}
