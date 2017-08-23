using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.APIWeather.WeatherResponeOpenWeatherMap;

namespace TelegramBot.Logic.APIWeather
{
    public interface ICanGetWeatherByName
    {
        /// <summary>
        /// Получение погоды на сегодня через название города
        /// </summary>
        /// <param name="city">Город</param>
        /// <returns>погода</returns>
        WeatherData GetWeather(string city);
    }
}
