using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [Authorize] // TODO: Setup as a global filter
    [DatabaseAuthorizationFilter(Policy = "ContentsEditor", ClaimsProviderName = "Test")]
    [HttpGet(Name = "GetUserInfo")]
    public UserInfo GetUserInfo()
    {
        var userInfo = new UserInfo();
        if (User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogInformation("User is authenticated");
            userInfo.Id = User.GetId();
            userInfo.ClaimsList = User.Claims.Select<Claim, KeyValuePair<string, string>>(claim =>
            {
                return new KeyValuePair<string, string>(claim.Type, claim.Value);
            }).ToList();
        }
        else
        {
            _logger.LogError("User is not authenticated");
        }

        return userInfo;
    }
}