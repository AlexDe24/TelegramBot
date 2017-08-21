using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.APIWeather.WeatherRespone;

namespace TelegramBot.Logic.APIWeather
{
    public interface INterfaceWeather
    {
        WeatherDate GetWeather(string city);
    }
}
