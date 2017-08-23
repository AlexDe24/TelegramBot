using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.Entity;

namespace TelegramBot.Logic.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private BaseContext _BaseCt;

        public UsersRepository()
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
        /// <param name="userId">id пользователя</param>
        /// <param name="cityName">название города</param>
        /// <returns></returns>
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
        /// Чтение пользователя с городами 
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <returns></returns>
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
