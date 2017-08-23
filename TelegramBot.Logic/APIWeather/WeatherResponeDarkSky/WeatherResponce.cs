using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APIWeather.WeatherResponeDarkSky
{
    public class WeatherResponse
    {
        public string TimeZone { get; set; }
        public WeatherInfo Currently { get; set; }
    }
}
