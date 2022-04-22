using System.Security.Claims;
using System.Text.Json.Serialization;
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

    [Authorize]
    [HttpGet(Name = "GetUserInfo")]
    [ActionName("GetUserInfo")]
    public UserInfo GetUserInfo()
    {
        return new UserInfo()
        {
            Id = this.User.GetId(),
            Claims = this.User.Claims.ToDictionary(claim => claim.Type, claim => claim.Value)
        };
    }
}

public class UserInfo
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }

  [JsonPropertyName("claims")]
  public Dictionary<string, string>? Claims { get; set; }
}

public static class UserHelpers
{
  public static string? GetId(this ClaimsPrincipal principal)
  {
    var userIdClaim = principal.FindFirst(c => c.Type == ClaimTypes.NameIdentifier) ?? principal.FindFirst(c => c.Type == "sub");
    if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
    {
      return userIdClaim.Value;
    }

    return null;
  }
}