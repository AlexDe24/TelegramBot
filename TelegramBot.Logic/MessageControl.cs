using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBot.Logic.APITranslate;
using TelegramBot.Logic.APIWeather;
using TelegramBot.Logic.Repositories;

namespace TelegramBot.Logic
{
    /// <summary>
    /// Класс для обработки сообщений и ответа на них
    /// </summary>
    public class MessageControl
    {
        private List<long> _isWaitAnswerForWeather { get; set; }
        private List<long> _isWaitAnswerForRememder { get; set; }

        private ICanTranslate _translator;
        private ICanGetWeatherByName _weatherByName;
        private ICanGetWeatherByCoordinate _weatherByCoordinate;

        private Func<IUsersRepository> _createRepository;

        public MessageControl(ICanTranslate translator, ICanGetWeatherByName weatherByName, ICanGetWeatherByCoordinate weatherByCoordinate, List<long> isWaitAnswerForWeather, List<long> isWaitAnswerForRememder, Func<IUsersRepository> createRepository)
        {
            _isWaitAnswerForWeather = isWaitAnswerForWeather;
            _isWaitAnswerForRememder = isWaitAnswerForRememder;

            _weatherByCoordinate = weatherByCoordinate;
            _weatherByName = weatherByName;
            _translator = translator;

            _createRepository = createRepository;
        }

        /// <summary>
        /// Обработка комманд, если была отправленна геолокация
        /// </summary>
        /// <param name="latitude">Широта</param>
        /// <param name="longitude">Долгота</param>
        public string LocationCommands(double latitude, double longitude)
        {
            if (_weatherByCoordinate != null)
            {
                var weather = _weatherByCoordinate.GetWeather(latitude, longitude);

                return CreateWeatherResponseMessage(weather);
            }
            else
            {
                return "Прогноз погоды по координатам не поддерживается.";
            }
        }

        /// <summary>
        /// Обработка комманд, если было отправлено сообщение
        /// </summary>
        /// <param name="message">текст сообщения</param>
        public string MessageCommands(Message message)
        {
            if (_isWaitAnswerForWeather.Any(x => x == message.Chat.Id)) //проверка на ожидание города для прогноза
            {
                try
                {
                    _isWaitAnswerForWeather.Remove(message.Chat.Id);

                    var weather = _weatherByName.GetWeather(_translator.Translate(message.Text, "ru", "en"));

                    return CreateWeatherResponseMessage(weather);
                }
                catch (Exception)
                {
                    return "Такого города нет.";
                }
            }
            else if (_isWaitAnswerForRememder.Any(x => x == message.Chat.Id)) //проверка на ожидание города для запоминания
            {
                _isWaitAnswerForRememder.Remove(message.Chat.Id);

                using (var usersRepository = _createRepository())
                {
                    usersRepository.AddOfEditUserAsync(message.Chat.Id, _translator.Translate(message.Text, "ru", "en")).Wait();
                }

                return "Город сохранён.";
            }
            else
            {
                var split = message.Text.Split(' ');

                switch (split[0])
                {
                    case "Погода":
                    case "погода":
                    case "/погода":
                    case "Weather":
                    case "weather":
                    case "/weather":
                        if (split.Length > 1)
                        {
                            string newsplit = " ";

                            for (int i = 1; i < split.Length; i++)
                            {
                                newsplit += split[i] + " ";
                            }

                            var weather = _weatherByName.GetWeather(_translator.Translate(newsplit, "ru", "en"));

                            return CreateWeatherResponseMessage(weather);
                        }
                        else
                        {
                            _isWaitAnswerForWeather.Add(message.Chat.Id);
                            return "Введите, пожалуйста, город.";
                        }
                    case "Запомнить":
                    case "/remembercity":
                        if (split.Length > 1)
                        {
                            string newsplit = " ";

                            for (int i = 1; i < split.Length; i++)
                            {
                                newsplit += split[i] + " ";
                            }

                            newsplit = newsplit.Remove(0, 1);

                            try
                            {
                                using (var usersRepository = _createRepository())
                                {
                                    usersRepository.AddOfEditUserAsync(message.Chat.Id, _translator.Translate(newsplit, "ru", "en")).Wait();
                                }

                                return "Город сохранён.";
                            }
                            catch (Exception)
                            {
                                return "Такого города нет.";
                            }
                        }
                        else
                        {
                            _isWaitAnswerForRememder.Add(message.Chat.Id);
                            return "Введите, пожалуйста, город.";
                        }
                    case "/help":
                    case "Помощь":
                    case "/start":
                        return $"Привет, {message.Chat.FirstName}, я ПогодаБот!\n" +
                            "Отправь мне Погода и название города или геолокацию, чтобы получить прогноз погоды на сегодня!\n" +
                            "А так же ты можешь отправить мне Запомнить и название города, чтобы я запомнил город и ты мог всегда быстро посмотреть в нём погоду!";
                    default:
                        return "Такой команды нет.";
                }
            }
        }

        /// <summary>
        /// Создания ответного сообщения по погоде
        /// </summary>
        /// <param name="weather">прогноз</param>
        /// <returns>сообщение</returns>
        private string CreateWeatherResponseMessage(WeatherData weather)
        {
            var response = $"Погода на {DateTime.Now.ToString("dd/MM/yyyy")} в городе {_translator.Translate(weather.CityName, "en", "ru")}: \n" +
                $"{Convert(weather.Description) + WeatherEmoji(weather.Description)}\n" +
                $"Направление ветра: {WindDirection(weather.WindDeg)}\n" +
                $"Скорость ветра: {weather.WindSpeed} м/с \n" +
                $"Средняя температура: {weather.Temp} C°\n";

            return response;
        }

        /// <summary>
        /// Ответ по направлению ветра
        /// </summary>
        /// <param name="der"></param>
        /// <returns></returns>
        private string WindDirection(int der)
        {
            if (der >= 338 || der < 22)
                return "Север ⬆";
            else if (der >= 22 && der < 67)
                return "Северо-воток ↗";
            else if (der >= 67 && der < 112)
                return "Восток ➡";
            else if (der >= 112 && der < 157)
                return "Юго-Воток ↘";
            else if (der >= 157 && der < 202)
                return "Юг ⬇";
            else if (der >= 202 && der < 247)
                return "Юго-Запад ↙";
            else if(der >= 247 && der < 292)
                return "Запад ⬅";
            else
                return "Северо-Запад ↖";
        }

        /// <summary>
        /// Смайлик для погоды
        /// </summary>
        /// <param name="weather"></param>
        /// <returns></returns>
        private string WeatherEmoji(string weather)
        {

            //☀🌤⛅🌥🌦☁🌧⛈🌩⚡🌨🌫
            switch (weather)
            {
                case "ясно":
                    return " ☀";
                case "слегка облачно":
                    return " 🌤";
                case "облачно":
                    return " ⛅";
                case "пасмурно":
                    return " 🌥";
                case "туман":
                    return " 🌫";
                case "легкий дождь":
                    return " 🌦";
                case "дождь":
                    return " 🌧";
                case "гроза":
                    return " ⛈";
                case "снег":
                    return " 🌨";
                default:
                    return "";
            }
            
        }

        /// <summary>
        /// Для того, что бы прогноз начинался с заглавной
        /// </summary>
        /// <param name="str">погода</param>
        /// <returns>погода с заглавной</returns>
        private string Convert(string str)
        {
            string newStr = null;

            newStr += str[0];
            newStr = newStr.ToUpper();

            newStr += str.Remove(0, 1);

            return newStr;
        }
    }
}
