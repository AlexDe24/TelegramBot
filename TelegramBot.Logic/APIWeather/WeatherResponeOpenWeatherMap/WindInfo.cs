using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APIWeather.WeatherResponeOpenWeatherMap
{
    public class WindInfo
    {
        public float Speed { get; set; } //скорость ветра

        public int Deg { get; set; } //направление ветра
    }
}
