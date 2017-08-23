using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APIWeather
{
    public class WeatherData
    {
        public String CityName { get; set; } //Название города

        public string Description { get; set; } //сама погода

        public float Temp { get; set; } //средняя температура

        public float WindSpeed { get; set; } //скорость ветра

        public int WindDeg { get; set; } //направление ветра
    }
}
