using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Logic.Entity;

namespace TelegramBot.Logic.Repositories
{
    public interface IUsersRepository : IDisposable
    {
        /// <summary>
        /// Добавление пользователя или изменение города
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="cityName">название города</param>
        /// <returns></returns>
        Task AddOfEditUserAsync(long userId, string cityName);

        /// <summary>
        /// Чтение пользователя с городами 
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <returns></returns>
        Task<User> GetUserAsync(long userId);
    }
}
