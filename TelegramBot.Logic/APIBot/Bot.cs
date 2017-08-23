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
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Logic.APITranslate;
using TelegramBot.Logic.APIWeather;
using TelegramBot.Logic.Repositories;

namespace TelegramBot.Logic
{
    /// <summary>
    /// Класс работы с API бота
    /// </summary>
    public class Bot
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private List<long> _isWaitAnswerForWeather { get; set; }
        private List<long> _isWaitAnswerForRememder { get; set; }

        private ICanGetWeatherByCoordinate _weatherByCoordinate;
        private ICanGetWeatherByName _weatherByName;
        private ICanTranslate _translator;

        private MessageControl _messageControl; //Класс для обработки сообщений и ответа на них
        private string _key; //токен доступа

        private Func<IUsersRepository> _createRepository;

        private CancellationToken _token;

        public Bot(string key, CancellationToken token, ICanGetWeatherByCoordinate weatherByCoordinate, ICanGetWeatherByName weatherByName, ICanTranslate translator, Func<IUsersRepository> createRepository)
        {
            _key = key;

            _token = token;

            _isWaitAnswerForWeather = new List<long>();
            _isWaitAnswerForRememder = new List<long>();

            _translator = translator;
            _weatherByName = weatherByName;
            _weatherByCoordinate = weatherByCoordinate;

            _createRepository = createRepository;

            _messageControl = new MessageControl(_translator, _weatherByName, _weatherByCoordinate, _isWaitAnswerForWeather, _isWaitAnswerForRememder, createRepository);
        }

        /// <summary>
        /// Получение сообщений и ответ на них
        /// </summary>
        public async Task BotWork()
        {
            var bot = new Telegram.Bot.TelegramBotClient(_key);
            await bot.SetWebhookAsync("");

            int offset = 0; //инервал между ответами

            while (!_token.IsCancellationRequested)
            {
                var updates = await bot.GetUpdatesAsync(offset);

                Message message;
                string responseMessage;

                foreach (var update in updates)
                {
                    try
                    {
                        switch (update.Type)
                        {
                            case UpdateType.UnkownUpdate:
                                break;
                            case UpdateType.EditedMessage:
                                break;
                            case UpdateType.MessageUpdate:
                                message = update.Message;
                                switch (message.Type)
                                {
                                    case MessageType.TextMessage:
                                        responseMessage = _messageControl.MessageCommands(message);

                                        if (_isWaitAnswerForRememder.Any(x => x == message.Chat.Id) || _isWaitAnswerForWeather.Any(x => x == message.Chat.Id))
                                        {
                                            await bot.SendTextMessageAsync(message.Chat.Id, responseMessage,
                                                false, false, 0, CreateInlineKeyboard());
                                        }
                                        else
                                        {
                                            await bot.SendTextMessageAsync(message.Chat.Id, responseMessage,
                                                false, false, 0, await CreateKeyboard(message.Chat.Id));
                                        }
                                        _logger.Info(message.Chat.FirstName + " " + message.Chat.Id + " - " + message.Text);
                                        break;
                                    case MessageType.VideoMessage:
                                        await bot.SendTextMessageAsync(message.Chat.Id, "Сейчас посмотрю.");
                                        break;
                                    case MessageType.StickerMessage:
                                        await bot.SendTextMessageAsync(message.Chat.Id, $"Очень смешно, {message.Chat.FirstName}.");
                                        break;
                                    case MessageType.LocationMessage:
                                        responseMessage = _messageControl.LocationCommands(message.Location.Latitude, message.Location.Longitude);
                                        await bot.SendTextMessageAsync(message.Chat.Id, responseMessage);
                                        break;
                                    default:
                                        await bot.SendTextMessageAsync(message.Chat.Id, "И что мне на это ответить?");
                                        break;
                                }
                                break;
                            case UpdateType.CallbackQueryUpdate:
                                await bot.EditMessageTextAsync(update.CallbackQuery.From.Id,update.CallbackQuery.Message.MessageId, "Возврат обратно.");
                                _isWaitAnswerForRememder.Remove(update.CallbackQuery.From.Id);
                                _isWaitAnswerForWeather.Remove(update.CallbackQuery.From.Id);
                                break;
                            default:
                                break;
                        }
                        offset = update.Id + 1;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message);
                    }
                }
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
        /// Создание основной клавиатуры
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns></returns>
        private async Task<ReplyKeyboardMarkup> CreateKeyboard(long id)
        {
            Entity.User userInfo;

            using (var usersRepository = _createRepository())
            {
                userInfo = await usersRepository.GetUserAsync(id);
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
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[0].Name, "en", "ru")}")
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
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[0].Name, "en", "ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[1].Name, "en", "ru")}")
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
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[0].Name, "en", "ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[1].Name, "en", "ru")}"),
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[2].Name, "en", "ru")}")
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
    }
}
