using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.APIWeather.WeatherRespone;

namespace TelegramBot.Logic.APIWeather
{
    public class OpenWeatherMap : INterfaceWeather
    {
        /// <summary>
        /// Получение погоды на сегодня через RestSharp
        /// </summary>
        /// <param name="city">город</param>
        /// <returns>погода</returns>
        public WeatherDate GetWeather(string city)
        {
            var client = new RestClient($"http://api.openweathermap.org/data/2.5/weather?q={city}&lang=ru&units=metric&appid=b1263a17d6a6ffb120b199f9ccde33a9");

            var request = new RestRequest("/", Method.POST);

            IRestResponse<WeatherResponse> response = client.Execute<WeatherResponse>(request);
            var weatherResponse = response.Data;

            var weatherDate = new WeatherDate()
            {
                CityName = weatherResponse.Name,
                Description = weatherResponse.Weather[0].Description,
                WindDeg = weatherResponse.Wind.Deg,
                WindSpeed = weatherResponse.Wind.Speed,
                Temp = weatherResponse.Main.Temp
            };

            return weatherDate;
        }
    }
}
