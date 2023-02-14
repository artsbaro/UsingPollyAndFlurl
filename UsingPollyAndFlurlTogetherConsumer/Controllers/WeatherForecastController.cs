using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UsingPollyAndFlurlTogetherConsumer.Models;

namespace UsingPollyAndFlurlTogetherConsumer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var weekday = await "https://localhost:44357/api/weekday"
                    .GetJsonAsync<WeekdayModel>();

                Debug.WriteLine("[App]: successful");

                return Ok(weekday);
            }
            catch (Exception e)
            {
                Debug.WriteLine("[App]: Failed - " + e.Message);
                throw;
            }
        }
    }
}