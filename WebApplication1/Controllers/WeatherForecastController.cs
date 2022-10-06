using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        // private readonly ITokenService _tokenService;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger
            // , ITokenService tokenService
        )
        {
            _logger = logger;
            //_tokenService = tokenService;
        }
        
        static SsoVinorsoft ssoHelper = new SsoVinorsoft(
            "http://117.4.247.68:10825/realms/DemoRealm/",
            "express-1",
            "Ih1aaQ6Jv1EFagzZjCRT8KIT2Nl9NovB",
            "http://localhost:5000/sso.html",
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAl44n+kHYSyKY6LR+1t3QYhfVI6yobWi8sTSKMP9q3RZDHjkQNs8BMIx3MIOrx3h4yg6ony6TsVzt6BbKK6GP/Bz8fqh0nhlI90aGfd+06arMXcg2vnSMIoxns8rnC20vN/vpdOKCM5u4QLwBQMcQbA7Y7n0KBEHPhB+i1+nP9tWILihLVEQ9cpuHj+qCGqBq1E+CZV4hb8tyYMKuAxKzA/EF4O6ABpt1r6pP56CDRTUBzzzxrqDkssZ/abqbjkSngEbEixuvtgDu6WAuMlq0QlvoM24s117Cu24PC6hrGgXB/n7IkeDMtNaR8iselHsk1L3YY9DLijR16c+9J3g/NwIDAQAB"
        );

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            // Console.WriteLine(accessToken);

            if (!ssoHelper.validateToken(accessToken))
            {
                // return null;
                return Enumerable.Range(1, 1).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index)
                }).ToArray();
            }
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        public class Code
        {
            public string code { get; set; }
        }
        public Token Post(Code code)
        {
            return ssoHelper.GetCode(code.code);
        }
    }
}
