using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.APITranslate;
using TelegramBot.Logic.APIWeather.WeatherResponeDarkSky;

namespace TelegramBot.Logic.APIWeather
{
    /// <summary>
    /// Класс для получения погоды через сервис DarkSky
    /// </summary>
    public class DarkSky : ICanGetWeatherByName, ICanGetWeatherByCoordinate
    {
        private ICanTranslate _translator;

        private string _token;
        private string _request;

        public DarkSky(ICanTranslate translator, string token, string request)
        {
            _token = token;
            _request = request;

            _translator = translator;
        }

        /// <summary>
        /// Получение погоды на сегодня через координаты
        /// </summary>
        /// <param name="latitude">Широта</param>
        /// <param name="longitude">Долгота</param>
        /// <returns></returns>
        public WeatherData GetWeather(double latitude, double longitude)
        {
            var latitudeString = latitude.ToString().Replace(",", ".");
            var longitudeString = longitude.ToString().Replace(",", ".");

            var client = new RestClient($"{_request}/{_token}/{latitudeString},{longitudeString}");
            var request = new RestRequest("/", Method.GET);

            IRestResponse<WeatherResponse> response = client.Execute<WeatherResponse>(request);
            var weatherResponse = response.Data;

           var weatherDate = new WeatherData()
            {
                CityName = "выбранном вами",
                Description = _translator.Translate(weatherResponse.Currently.Summary, "en", "ru"),
                WindDeg = weatherResponse.Currently.WindBearing,
                WindSpeed = weatherResponse.Currently.WindSpeed,
                Temp = (float)Math.Round((weatherResponse.Currently.Temperature - 32) / 1.8f)
            };

            return weatherDate;
        }

        /// <summary>
        /// Получение погоды на сегодня через название города
        /// </summary>
        /// <param name="city">Город</param>
        /// <returns></returns>
        public WeatherData GetWeather(string city)
        {
            var client = new RestClient($"http://nominatim.openstreetmap.org/search?city={city}&limit=9&format=json");
            var request = new RestRequest("/", Method.GET);

            IRestResponse<List<LocationResponce>> responseLocation = client.Execute<List<LocationResponce>>(request);
            var results = responseLocation.Data;

            var latitudeString = results[0].Lat.ToString().Replace(",", ".");
            var longitudeString = results[0].Lon.ToString().Replace(",", ".");

            client = new RestClient($"https://api.darksky.net/forecast/7a14e046be3afaf782b041c3dcf6ec59/{latitudeString},{longitudeString}");

            IRestResponse<WeatherResponse> response = client.Execute<WeatherResponse>(request);
            var weatherResponse = response.Data;

            var weatherDate = new WeatherData()
            {
                CityName = results[0].Display_name,
                Description = _translator.Translate(weatherResponse.Currently.Summary, "en", "ru"),
                WindDeg = weatherResponse.Currently.WindBearing,
                WindSpeed = weatherResponse.Currently.WindSpeed,
                Temp = (float)Math.Round((weatherResponse.Currently.Temperature - 32) / 1.8f)
            };

            return weatherDate;
        }//https://maps.googleapis.com/maps/api/place/nearbysearch/key=AIzaSyCFV-00WU7qZUw0hy4e9aONwZAV56Zwghw&location=1,1
    }//AIzaSyCFV-00WU7qZUw0hy4e9aONwZAV56Zwghw
}
