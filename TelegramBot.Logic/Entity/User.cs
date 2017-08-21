using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.Entity
{
    public class User
    {
        public int Id { get; set; }

        public long UserId { get; set; }

        public List<City> City { get; set; }
    }
}
