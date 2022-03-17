using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using tryout_blazor_api.Shared;

namespace tryout_blazor_api.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SightController : ControllerBase
{
    static private List<Sight> sights = new () {
        new () {
            Name = "Brandenburger Tor",
            Location = new () { Latitude = 52.516272F, Longitude = 13.377722F, Altitude = 0 },
            Facts = new List<SightFact>() {
                new() { Fact = "Located in Berlin" },
                new() { Fact = "Architect: Carl Gotthard Langhans" },
                new() { Fact = "Construction started: 1788" },
                new() { Fact = "Completed: 1791" }
            }
        },
        new () {
            Name = "Siegessaeule",
            Location = new () { Latitude = 52.514444F, Longitude = 13.35F, Altitude = 0 },
            Facts = new List<SightFact>() {
                new() { Fact = "Located in Berlin" },
                new() { Fact = "Designer: Heinrich Strack" },
                new() { Fact = "Construction started: 1864" },
                new() { Fact = "Completed: 1873" }
            }
        }
    };

    private readonly ILogger<SightController> _logger;

    public SightController(ILogger<SightController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<Sight> Get()
    {
        return sights;
    }

    [HttpGet]
    [Route("{id}")]
    public Sight Get(int id)
    {
        if(id >= sights.Count)
            return sights[0];
        return sights[id];
    }
}
