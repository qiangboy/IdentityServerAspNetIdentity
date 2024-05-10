using Application;
using Domain.Identities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(UserService userService) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public Task<List<ApplicationUser>> Get()
    {
        return userService.GetListAsync();
    }

    [HttpPost]
    [Authorize]
    public IActionResult Create()
    {
        var user = User;

        return Ok();
    }
}