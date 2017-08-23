using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APIWeather.WeatherResponeOpenWeatherMap
{
    public class WeatherResponse
    {
        public TemperatureInfo Main { get; set; } //поле Main(Температура)

        public List<WeatherInfo> Weather { get; set; } //поле Weather

        public WindInfo Wind { get; set; } //поле Wind

        public string Name { get; set; } //Название города
    }
}
