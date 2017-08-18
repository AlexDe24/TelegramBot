using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;
using TelegramBot.Logic.DTO;

namespace TelegramBot.Logic
{
    public class BaseContext : DbContext
    {
        public BaseContext() : base("TelegramConnection")
        {
        }

        public DbSet<User> Users { get; set; }
    }
}