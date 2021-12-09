using System;
using System.Collections.Generic;
using System.Linq;
using SolidTradeServer.Data.Domain;

namespace SolidTradeServer.Services
{
    public class IndexService
    {
        public IEnumerable<WeatherInfo> FetchWeatherInfos()
        {
            var rng = new Random();

            if (rng.Next(0, 5) < 2)
            {
                throw new Exception("Oops what happened?");
            }
                
            return Enumerable.Range(1, 5).Select(i => new WeatherInfo
            {
                Date = DateTime.Now.AddDays(i),
                TemperatureC = rng.Next(-10, 10),
            });
        }
    }
}