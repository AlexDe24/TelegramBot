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
using TelegramBot.Logic.APITranslate;
using TelegramBot.Logic.Repositories;

namespace TelegramBot.Logic
{
    /// <summary>
    /// Класс работы с API
    /// </summary>
    public class APIClass
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private INterfaceTranslator _translator;
        private ResponseMessage _responseMessage; //Класс для обработки сообщений и ответа на них
        private string _key; //токен доступа

        public APIClass(string key)
        {
            _translator = new Translator();
            _responseMessage = new ResponseMessage();
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

            while (true)
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
                                message = update.EditedMessage;
                                responseMessage = _responseMessage.MessageCommands(message);

                                var editUpdate = updates.Where(x => x.EditedMessage != null).ToList();

                                var lastMessageNom = editUpdate.Where(x => x.EditedMessage.Chat.Id == message.Chat.Id).LastOrDefault().EditedMessage.MessageId;

                                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "123");
                                
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
                                        _logger.Info(message.Chat.FirstName + " " + message.Chat.Id + " - " + message.Text);
                                        break;
                                    case MessageType.VideoMessage:
                                        await bot.SendTextMessageAsync(message.Chat.Id, "Сейчас посмотрю.");
                                        break;
                                    case MessageType.StickerMessage:
                                        await bot.SendTextMessageAsync(message.Chat.Id, $"Очень смешно, {message.Chat.FirstName}.");
                                        break;
                                    default:
                                        await bot.SendTextMessageAsync(message.Chat.Id, "И что мне на это ответить?");
                                        break;
                                }
                                break;
                            case UpdateType.CallbackQueryUpdate:
                                await bot.EditMessageTextAsync(update.CallbackQuery.From.Id,update.CallbackQuery.Message.MessageId, "Возврат обратно.");

                                //await bot.SendTextMessageAsync(update.CallbackQuery.From.Id, "Возврат обратно.");
                                _responseMessage.WaitAnswerForRememder.Remove(update.CallbackQuery.From.Id);
                                _responseMessage.WaitAnswerForWeather.Remove(update.CallbackQuery.From.Id);
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
        /// Создание клавиатуры
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns></returns>
        private async Task<ReplyKeyboardMarkup> CreateKeyboard(long id)
        {
            Entity.User userInfo;

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
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[0].Name,"ru")}")
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
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[0].Name,"ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[1].Name,"ru")}")
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
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[0].Name,"ru")}")
                            },
                            new KeyboardButton[]
                            {
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[1].Name,"ru")}"),
                                new KeyboardButton($"Погода {_translator.Translate(userInfo.City[2].Name,"ru")}")
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
