using Newtonsoft.Json;
using NLog;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Logic.Repositories;

namespace TelegramBot.Logic
{
    /// <summary>
    /// Класс работы с API
    /// </summary>
    public class APIClass
    {
        private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private ResponseMessage _responseMessage; //Класс для обработки сообщений и ответа на них
        private string _key; //токен доступа

        public APIClass(string key)
        {
            _responseMessage = new ResponseMessage(this);
            _key = key;
        }

        /// <summary>
        /// Получение запросов и ответ на них
        /// </summary>
        public async Task BotWork()
        {
            var bot = new Telegram.Bot.TelegramBotClient(_key);
            await bot.SetWebhookAsync("");

            int offset = 0; //инервал между ответами

            var ikm = new InlineKeyboardMarkup();

            while (true)
            {
                var updates = await bot.GetUpdatesAsync(offset);
                Message message;
                string responseMessage;

                foreach (var update in updates)
                {
                    switch (update.Type)
                    {
                        case UpdateType.UnkownUpdate:
                            break;
                        case UpdateType.EditedMessage:
                            message = update.EditedMessage;
                            responseMessage = _responseMessage.MessageCommands(message);

                            var editUpdate = updates.Where(x => x.EditedMessage != null).ToList();

                            var lastMessageNom = editUpdate.Where(x => x.EditedMessage.Chat.Id == message.Chat.Id).LastOrDefault().EditedMessage.MessageId;

                            try
                            {
                                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId + 1, responseMessage);
                            }
                            catch (Exception ex)
                            {
                                var a = ex;
                            }
                            break;

                        case UpdateType.MessageUpdate:
                            message = update.Message;
                            switch (message.Type)
                            {
                                case MessageType.TextMessage:

                                    responseMessage = _responseMessage.MessageCommands(message);

                                    if (_responseMessage.WaitAnswerForRememder.Any(x => x == message.Chat.Id) || _responseMessage.WaitAnswerForWeather.Any(x => x == message.Chat.Id))
                                    {
                                        await bot.SendTextMessageAsync(message.Chat.Id, responseMessage,
                                        false, false, 0, CreateInlineKeyboard());
                                    }
                                    else
                                    {
                                        await bot.SendTextMessageAsync(message.Chat.Id, responseMessage,
                                            false, false, 0, await CreateKeyboard(message.Chat.Id));
                                    }
                                    _logger.Info(message.Chat.FirstName + " - " + message.Text);
                                    break;
                                case MessageType.VideoMessage:
                                    await bot.SendTextMessageAsync(message.Chat.Id, "Сейчас посмотрю.");
                                    break;
                                case MessageType.StickerMessage:
                                    await bot.SendTextMessageAsync(message.Chat.Id, $"Очень смешно, {message.Chat.FirstName}");
                                    break;
                                default:
                                    await bot.SendTextMessageAsync(message.Chat.Id, "И что мне на это ответить?");
                                    break;
                            }
                            break;
                        case UpdateType.CallbackQueryUpdate:
                            await bot.SendTextMessageAsync(update.CallbackQuery.From.Id, "Возврат обратно.");
                            _responseMessage.WaitAnswerForRememder.Remove(update.CallbackQuery.From.Id);
                            _responseMessage.WaitAnswerForWeather.Remove(update.CallbackQuery.From.Id);
                            break;
                        default:
                            break;
                    }

                    offset = update.Id + 1;
                }
            }
        }

        /// <summary>
        /// Переводчик
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string Translate(string text, string lang)
        {
            try
            {
                var client = new RestClient("https://translate.yandex.net/api/v1.5/tr.json/translate?"
                  + "key=trnsl.1.1.20170818T125137Z.bc602a78a7863e84.26863776415c893e9c6e51cc8a03462540980d54"
                  + "&text=" + text
                  + "&lang=" + lang);

                var request = new RestRequest("/", Method.POST);

                IRestResponse<TranslatorResponse> response = client.Execute<TranslatorResponse>(request);
                var translatorResponse = response.Data.Text;

                translatorResponse = translatorResponse.Remove(0, 2);
                translatorResponse = translatorResponse.Remove(translatorResponse.Length - 2, 2);

                return translatorResponse;
            }
            catch (Exception)
            {

                return text;
            }
            
        }

        /// <summary>
        /// Клавиатура под сообщением
        /// </summary>
        /// <returns></returns>
        private InlineKeyboardMarkup CreateInlineKeyboard()
        {
            var inlineKeyboardMarkup = new InlineKeyboardMarkup()
            {
                InlineKeyboard = new InlineKeyboardButton[][]
                {
                    new InlineKeyboardButton[]
                    {
                        new InlineKeyboardButton("Отмена")
                    }
                }
            };

            return inlineKeyboardMarkup;
        }

        /// <summary>
        /// Создание клавиатуры
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<ReplyKeyboardMarkup> CreateKeyboard(long id)
        {
            DTO.User userInfo;

            using (var usersSQL = new UsersSQL())
            {
                userInfo = await usersSQL.GetUserAsync(id);
            }

            var replyKeyboardMarkup = new ReplyKeyboardMarkup()
            {
                ResizeKeyboard = true
            };

            if (userInfo != null)
            {
                switch (userInfo.City.Count)
                {
                    case 1:
                        replyKeyboardMarkup.Keyboard = new KeyboardButton[][]
                        {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Погода"),
                                new KeyboardButton($"Погода {Translate(userInfo.City[0].Name,"ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Запомнить")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Помощь")
                            }
                        };
                        break;
                    case 2:
                        replyKeyboardMarkup.Keyboard = new KeyboardButton[][]
                        {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Погода"),
                                new KeyboardButton($"Погода {Translate(userInfo.City[0].Name,"ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton($"Погода {Translate(userInfo.City[1].Name,"ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Запомнить")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Помощь")
                            }
                        };
                        break;
                    case 3:
                        replyKeyboardMarkup.Keyboard = new KeyboardButton[][]
                        {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Погода"),
                                new KeyboardButton($"Погода {Translate(userInfo.City[0].Name,"ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton($"Погода {Translate(userInfo.City[1].Name,"ru")}"),
                                new KeyboardButton($"Погода {Translate(userInfo.City[2].Name,"ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Запомнить")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Помощь")
                            }
                        };
                        break;
                    default:
                        break;
                }
                
            }
            else
            {
                replyKeyboardMarkup.Keyboard = new KeyboardButton[][]
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Погода")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Запомнить")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Помощь")
                    }
                };
            }

            return replyKeyboardMarkup;
        }

        /// <summary>
        /// Получение погоды на сегодня через RestSharp
        /// </summary>
        /// <param name="city">город</param>
        /// <returns>погода</returns>
        public WeatherResponse WeatherRestSharp(string city)
        {
            var client = new RestClient($"http://api.openweathermap.org/data/2.5/weather?q={city}&lang=ru&units=metric&appid=b1263a17d6a6ffb120b199f9ccde33a9");

            var request = new RestRequest("/", Method.POST);

            IRestResponse<WeatherResponse> response = client.Execute<WeatherResponse>(request);
            var weatherResponse = response.Data;

            return weatherResponse;
        }

        /// <summary>
        /// Получение погоды на сегодня
        /// </summary>
        /// <param name="city">город</param>
        /// <returns>погода</returns>
        public WeatherResponse Weather(string city)
        {
            string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&lang=ru&units=metric&appid=b1263a17d6a6ffb120b199f9ccde33a9";

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response); //парсинг ответа

            return weatherResponse;
        }
    }
}
