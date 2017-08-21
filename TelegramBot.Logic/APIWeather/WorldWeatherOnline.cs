using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.APIWeather.WeatherRespone;

namespace TelegramBot.Logic.APIWeather
{
    class WorldWeatherOnline : INterfaceWeather
    {
        public WeatherDate GetWeather(string city)
        {
            var client = new RestClient($"https://api.weather.yandex.ru/v1/forecast?geoid=213&l10n=true&extra=true");
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
