using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APIWeather.WeatherResponeDarkSky
{
    public class WeatherInfo
    {
        public float Temperature { get; set; }
        public string Summary { get; set; }

        public float WindSpeed { get; set; }
        public int WindBearing { get; set; }
    }
}
