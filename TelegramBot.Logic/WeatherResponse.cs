using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic
{
    public class WeatherResponse
    {
        public TemperatureInfo Main { get; set; }

        public List<WeatherInfo> Weather { get; set; }

        public string Name { get; set; }
    }
}
