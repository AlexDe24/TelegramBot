using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APIWeather
{
    public interface ICanGetWeatherByCoordinate
    {
        /// <summary>
        /// Получение погоды на сегодня через координаты
        /// </summary>
        /// <param name="latitude">Широта</param>
        /// <param name="longitude">Долгота</param>
        /// <returns></returns>
        WeatherData GetWeather(double latitude, double longitude);
    }
}
