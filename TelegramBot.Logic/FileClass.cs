using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic
{
    public class FileClass
    {
        /// <summary>
        /// Получение запросов от бота
        /// </summary>
        async public void BotWork()
        {
            string key = "430520142:AAF9c0PIq-VWg_MehdYDJq9rULHrxlB7zMM";

            var bot = new Telegram.Bot.TelegramBotClient(key);
            await bot.SetWebhookAsync("");

            int offset = 0;

            while (true)
            {
                var updates = await bot.GetUpdatesAsync(offset); 

                foreach (var update in updates)
                {
                    var message = update.Message;

                    if (message != null)
                        if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                        {
                            MessageCommands(message, bot);
                        }
                    offset = update.Id + 1;
                }

            }
        }

        /// <summary>
        /// Обработка комманд, если было отправлено сообщение
        /// </summary>
        /// <param name="message">текст сообщения</param>
        /// <param name="bot">бот</param>
        async void MessageCommands(Telegram.Bot.Types.Message message, Telegram.Bot.TelegramBotClient bot)
        {
            var split = message.Text.Split(' ');
            switch (split[0])
            {
                case "/weather":
                    if (split.Length == 1)
                        await bot.SendTextMessageAsync(message.Chat.Id, "Введите, пожалуйста, город.");
                    else
                        try
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, WeatherResponseMessage(split[1]));
                        }
                        catch (Exception)
                        {
                            await bot.SendTextMessageAsync(message.Chat.Id, "Такого города нет.");
                        }

                    break;

                default:
                    await bot.SendTextMessageAsync(message.Chat.Id, "Такой команды нет.");
                    break;
            }
        }

        /// <summary>
        /// Создания ответного сообщения
        /// </summary>
        /// <param name="city">город</param>
        /// <returns>сообщение</returns>
        string WeatherResponseMessage(string city)
        {
            string response;
            var fullResponse = Weather(city);

            response = $"Погода в городе {fullResponse.Name}: \n" +
                $"{Convert(fullResponse.Weather[0].Description)}\n" +
                $"Средняя температура: {fullResponse.Main.Temp}C°\n" +
                $"Минимальная температура: {fullResponse.Main.Temp_Min}C°\n" +
                $"Максимальная температура: {fullResponse.Main.Temp_Max}C°";

            return response;
        }

        /// <summary>
        /// Для того, что бы прогноз начинался с заглавной
        /// </summary>
        /// <param name="str">погода</param>
        /// <returns>погода с заглавной</returns>
        string Convert(string str)
        {
            string newStr = null;

            newStr += str[0];
            newStr = newStr.ToUpper();

            newStr += str.Remove(0, 1);

            return newStr;
        }

        /// <summary>
        /// Получение погоды на сегодня
        /// </summary>
        /// <param name="city">город</param>
        /// <returns>погода</returns>
        WeatherResponse Weather(string city)
        {
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&lang=ru&units=metric&appid=b1263a17d6a6ffb120b199f9ccde33a9";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

            return weatherResponse;
        }
    }
}
