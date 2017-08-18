﻿using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.DTO;

namespace TelegramBot.Logic.Repositories
{
    class UsersSQL : IDisposable
    {
        private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        BaseContext _BaseCt;

        public UsersSQL()
        {
            _BaseCt = new BaseContext();
        }

        public void Dispose()
        {
            _BaseCt.Dispose();
        }

        /// <summary>
        /// Добавление пользователя или изменение города
        /// </summary>
        public async Task AddOfEditUserAsync(long userId, string cityName)
        {
            var baseUser = await _BaseCt.Users.Where(x => x.UserId == userId).Include(x => x.City).FirstOrDefaultAsync().ConfigureAwait(false);

            try
            {
                if (baseUser == null)
                {
                    User user = new User()
                    {
                        UserId = userId,
                        City = new List<City>()
                    };

                    City city = new City()
                    {
                        Name = cityName,
                        User = user
                    };

                    user.City.Add(city);

                    _BaseCt.Users.Add(user);
                }
                else
                {
                    City city = new City()
                    {
                        Name = cityName,
                        User = baseUser
                    };

                    if (baseUser.City.Count == 3)
                    {
                        baseUser.City.RemoveAt(0);
                    }

                    baseUser.City.Add(city);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }

            _BaseCt.SaveChanges();
        }

        /// <summary>
        /// Чтение пользователя
        /// </summary>
        public async Task<User> GetUserAsync(long userId)
        {
            try
            {
                return await _BaseCt.Users.Where(x => x.UserId == userId).Include(x => x.City).FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);

                return null;
            }
        }
    }
}