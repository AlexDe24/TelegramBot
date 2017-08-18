using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBot.Logic.Repositories;

namespace TelegramBot.Logic
{
    /// <summary>
    /// Класс для обработки сообщений и ответа на них
    /// </summary>
    public class ResponseMessage
    {
        public List<long> WaitAnswerForWeather;
        public List<long> WaitAnswerForRememder;

        private APIClass _apiWork;

        public ResponseMessage(APIClass apiWork)
        {
            WaitAnswerForWeather = new List<long>();
            WaitAnswerForRememder = new List<long>();

            _apiWork = apiWork;
        }

        /// <summary>
        /// Обработка комманд, если было отправлено сообщение
        /// </summary>
        /// <param name="message">текст сообщения</param>
        /// <param name="bot">бот</param>
        public string MessageCommands(Message message)
        {
            if (WaitAnswerForWeather.Any(x => x == message.Chat.Id))
            {
                try
                {
                    WaitAnswerForWeather.Remove(message.Chat.Id);

                    return CreateWeatherResponseMessage(_apiWork.Translate(message.Text, "en"));
                }
                catch (Exception)
                {
                    return "Такого города нет.";
                }
            }
            else if (WaitAnswerForRememder.Any(x => x == message.Chat.Id))
            {
                WaitAnswerForRememder.Remove(message.Chat.Id);

                using (var usersSQL = new UsersSQL())
                {
                    usersSQL.AddOfEditUserAsync(message.Chat.Id, _apiWork.Translate(message.Text, "en")).Wait();
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

                            return CreateWeatherResponseMessage(_apiWork.Translate(newsplit, "en"));
                        }
                        else
                        {
                            WaitAnswerForWeather.Add(message.Chat.Id);
                            return "Введите, пожалуйста, город.";
                        }
                    case "Запомнить":
                    case "/remembercity":
                        if (split.Length > 1)
                        {
                            string newsplit = " ";

                            for (int i = 1; i < split.Length; i++)
                            {
                                newsplit += split[0] + " ";
                            }

                            try
                            {
                                WaitAnswerForWeather.Remove(message.Chat.Id);

                                return CreateWeatherResponseMessage(_apiWork.Translate(newsplit, "en"));
                            }
                            catch (Exception)
                            {
                                return "Такого города нет.";
                            }
                        }
                        else
                        {
                            WaitAnswerForRememder.Add(message.Chat.Id);
                            return "Введите, пожалуйста, город.";
                        }
                    case "/help":
                    case "Помощь":
                    case "/start":
                        return $"Привет, {message.Chat.FirstName}, я ПогодаБот!\n" +
                            "Отправь мне /weather или /погода и название города, чтобы получить прогноз погоды на сегодня!\n" +
                            "А так же ты можешь отправить мне /remembercity, чтобы я запомнил город и ты мог всегда быстро посмотреть в нём погоду!";
                    default:
                        return "Такой команды нет.";
                }
            }
        }

        /// <summary>
        /// Создания ответного сообщения по погоде
        /// </summary>
        /// <param name="city">город</param>
        /// <returns>сообщение</returns>
        private string CreateWeatherResponseMessage(string city)
        {
            var fullResponse = _apiWork.WeatherRestSharp(city);

            var response = $"Погода на {DateTime.Now.ToString("dd/MM/yyyy")} в городе {_apiWork.Translate(fullResponse.Name, "ru")}: \n" +
                $"{Convert(fullResponse.Weather[0].Description) + WeatherEmoji(fullResponse.Weather[0].Description)}\n" +
                $"Направление ветра: {WindDirection(fullResponse.Wind.Deg)}\n" +
                $"Скорость ветра: {fullResponse.Wind.Speed} м/с \n" +
                $"Средняя температура: {fullResponse.Main.Temp} C°\n";

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
                    return " 🦉";
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
