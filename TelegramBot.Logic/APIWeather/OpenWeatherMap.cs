using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.APIWeather.WeatherResponeOpenWeatherMap;

namespace TelegramBot.Logic.APIWeather
{
    /// <summary>
    /// Класс для получения погоды через сервис OpenWeatherMap
    /// </summary>
    public class OpenWeatherMap : ICanGetWeatherByName
    {
        private string _token;
        private string _request;

        public OpenWeatherMap(string token, string request)
        {
            _token = token;
            _request = request;
        }

        /// <summary>
        /// Получение погоды на сегодня через название города
        /// </summary>
        /// <param name="city">Город</param>
        /// <returns>погода</returns>
        public WeatherData GetWeather(string city)
        {
            var client = new RestClient($"{_request}q={city}&lang=ru&units=metric&appid={_token}");

            var request = new RestRequest("/", Method.POST);

            IRestResponse<WeatherResponse> response = client.Execute<WeatherResponse>(request);
            var weatherResponse = response.Data;

            var weatherDate = new WeatherData()
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
