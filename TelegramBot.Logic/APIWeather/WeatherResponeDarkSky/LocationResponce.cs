using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APIWeather.WeatherResponeDarkSky
{
    class LocationResponce
    {
        public float Lat { get; set; }
        public float Lon { get; set; }

        public string Display_name { get; set; }
    }
}
